// CreateRoom server self-test — replays the modern client's boot/login/room/photon sequence against
// the running server and validates each step. No retail client needed: this proves the server's
// happy-path end-to-end and surfaces gaps. Run the server first, then: node selftest.mjs
import crypto from 'node:crypto';

const BASE = process.env.BASE || 'http://localhost:8080';
let pass = 0, fail = 0;
const b64url = s => Buffer.from(s, 'base64url');

async function step(label, method, path, validate, body) {
  try {
    const opt = { method, headers: {} };
    if (body !== undefined) { opt.headers['content-type'] = 'application/json'; opt.body = JSON.stringify(body); }
    const r = await fetch(BASE + path, opt);
    const text = await r.text();
    let json; try { json = text ? JSON.parse(text) : {}; } catch { json = null; }
    if (r.status !== 200) throw new Error(`HTTP ${r.status}`);
    if (json === null) throw new Error('non-JSON body: ' + text.slice(0, 60));
    const msg = validate ? validate(json, r) : null;   // validate returns a string note, or throws
    console.log(`  ✓ ${label.padEnd(34)} ${method} ${path}${msg ? '  → ' + msg : ''}`);
    pass++;
    return json;
  } catch (e) {
    console.log(`  ✗ ${label.padEnd(34)} ${method} ${path}  → FAIL: ${e.message}`);
    fail++;
    return null;
  }
}

console.log(`\n=== CreateRoom self-test @ ${BASE} ===\n`);

// 1) OIDC discovery + JWKS (client fetches these to validate the OAuth token)
const disco = await step('OIDC discovery', 'GET', '/.well-known/openid-configuration',
  j => { if (!j.issuer || !j.jwks_uri) throw new Error('missing issuer/jwks_uri'); return 'issuer ' + j.issuer; });
const jwks = await step('JWKS', 'GET', '/Auth/.well-known/openid-configuration/jwks',
  j => { if (!j.keys?.length) throw new Error('no keys'); return j.keys.length + ' key(s), kid ' + j.keys[0].kid; });

// 2) OAuth token
const tok = await step('OAuth token', 'POST', '/Auth/connect/token',
  j => { if (!j.access_token) throw new Error('no access_token'); if (j.token_type !== 'Bearer') throw new Error('token_type ' + j.token_type); return 'Bearer, expires ' + j.expires_in + 's'; },
  { grant_type: 'password', username: 'local', password: 'x' });

// 2b) CRITICAL: verify the JWT signs against the JWKS (this is what the client enforces)
try {
  if (!tok?.access_token || !jwks?.keys?.length) throw new Error('missing token or jwks');
  const [h, p, s] = tok.access_token.split('.');
  if (!s) throw new Error('token is not a 3-part JWS');
  const hdr = JSON.parse(b64url(h).toString());
  const key = jwks.keys.find(k => k.kid === hdr.kid) || jwks.keys[0];
  if (hdr.kid && key.kid !== hdr.kid) throw new Error('kid mismatch: token ' + hdr.kid + ' vs jwks ' + key.kid);
  if (hdr.alg !== 'RS256') throw new Error('alg ' + hdr.alg + ' (expected RS256)');
  const pub = crypto.createPublicKey({ key: { kty: 'RSA', n: key.n, e: key.e }, format: 'jwk' });
  const ok = crypto.createVerify('RSA-SHA256').update(`${h}.${p}`).verify(pub, b64url(s));
  if (!ok) throw new Error('signature does NOT verify against JWKS');
  const claims = JSON.parse(b64url(p).toString());
  console.log(`  ✓ ${'JWT verifies against JWKS'.padEnd(34)} (crypto)    → RS256 ok, sub ${claims.sub}, iss ${claims.iss}`);
  pass++;
} catch (e) { console.log(`  ✗ ${'JWT verifies against JWKS'.padEnd(34)} (crypto)    → FAIL: ${e.message}`); fail++; }

// 3) Boot gate: version check must say ValidForPlay (0/0)
await step('Version check (boot gate)', 'GET', '/api/versioncheck/islandedversions',
  j => { if (j.VersionStatus !== 0 || j.UpdateNotificationStage !== 0) throw new Error(`status ${j.VersionStatus}/${j.UpdateNotificationStage} (need 0/0)`); return 'ValidForPlay'; });

// 4) Config
await step('App config', 'GET', '/api/config/v2', j => { if (!Array.isArray(j)) throw new Error('not an array'); return j.length + ' flags'; });
await step('Game configs', 'GET', '/api/gameconfigs/v1/all', j => Array.isArray(j) ? j.length + ' configs' : (() => { throw new Error('not array'); })());

// 5) Account
await step('Self account', 'GET', '/Accounts/account/me',
  j => { if (!j.accountId) throw new Error('no accountId'); return j.username + ' (#' + j.accountId + ')'; });

// 6) Matchmaking / session lifecycle
await step('Matchmaking player', 'GET', '/Matchmaking/player', j => 'acct #' + j.accountId);
await step('Matchmaking login', 'POST', '/Matchmaking/player/login', j => 'ok', {});
await step('Heartbeat', 'POST', '/Matchmaking/player/heartbeat', () => 'ok', {});
await step('Connection info', 'GET', '/Matchmaking/player/connection-info',
  j => { if (!j.host || !j.port) throw new Error('no host/port'); return j.host + ':' + j.port; });

// 7) Rooms
await step('Dorm room', 'GET', '/Room_server/dormroom/me', j => { if (!j.RoomId) throw new Error('no RoomId'); return j.Name; });
const featured = await step('Featured rooms', 'GET', '/Room_server/featuredrooms/current', j => { if (!Array.isArray(j) || j.length < 1) throw new Error('expected >=1 featured room'); return j.length + ' rooms (e.g. "' + j[0].Name + '")'; });
await step('Room listing', 'GET', '/Room_server/rooms', j => { if (!Array.isArray(j) || j.length < 3) throw new Error('expected >=3 rooms'); return j.length + ' rooms'; });
const wantRoom = featured?.[0];
await step('Room lookup is coherent', 'GET', '/Room_server/rooms/' + (wantRoom?.RoomId ?? 10),
  j => { if (wantRoom && j.Name !== wantRoom.Name) throw new Error(`room ${j.RoomId} name "${j.Name}" != listing "${wantRoom.Name}"`); return `room ${j.RoomId} = "${j.Name}" matches the listing`; });

// 8) Photon realtime token (last gate before joining a room)
await step('Photon access token', 'GET', '/Room_server/photon_access_token',
  j => { if (!j.Token) throw new Error('no Token'); return 'token ok, region ' + j.Region; });

// 9) Extended API surface — spot-check shapes across categories
await step('Commerce catalog', 'GET', '/Commerce/api/catalog/v1/all', j => { if (!Array.isArray(j)) throw new Error('not array'); return j.length + ' items'; });
await step('Currency balance', 'GET', '/api/storefronts/v4/balance/Tokens', j => { if (j.currency !== 'Tokens' || typeof j.amount !== 'number') throw new Error('bad balance shape'); return j.currency + '=' + j.amount; });
await step('Avatar rendering enabled', 'GET', '/api/customAvatarItems/v1/isRenderingEnabled', j => { if (j !== true) throw new Error('expected true'); return 'true'; });
await step('Sanitize isPure', 'GET', '/api/sanitize/v1/isPure', j => { if (j !== true) throw new Error('expected true'); return 'true'; });
await step('My inventions', 'GET', '/api/inventions/v2/mine', j => { if (!Array.isArray(j)) throw new Error('not array'); return j.length + ' inventions'; });
await step('Room inventory by id', 'GET', '/econ/roomInventory/room/10', j => { if (!Array.isArray(j)) throw new Error('not array'); return 'array ok'; });
await step('Mute (social action)', 'POST', '/api/relationships/v1/mute', () => 'ok', {});

console.log(`\n=== ${pass} passed, ${fail} failed ===\n`);
process.exit(fail ? 1 : 0);
