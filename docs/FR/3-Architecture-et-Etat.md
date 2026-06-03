# 3 — Architecture & État

Un résumé honnête et de haut niveau de la façon dont le client moderne est protégé et de l'état d'un revival
côté serveur privé. **Ce document omet volontairement toute instruction de contournement.**

## Comment le client moderne est protégé

Le client commercial est durci contre le fait d'être pointé vers un serveur tiers :

- **Pinning de certificat** — le client valide les certificats serveur contre son propre matériel épinglé et
  **ignore le magasin de confiance de l'OS** ; un certificat auto-signé pour `*.rec.net` est donc rejeté.
- **DNS-over-HTTPS** — les noms d'hôtes sont résolus via un canal chiffré plutôt qu'en DNS classique.
- **Anti-triche** — un composant anti-triche dédié (commercial, durci) tourne avec le jeu et réagit à toute
  altération du processus en cours d'exécution.
- **Lanceur natif** — un lanceur séparé et durci effectue des contrôles de démarrage (dont un version-check)
  avant que le jeu ne tourne.
- **IL2CPP** — la logique du jeu est compilée en natif (Unity IL2CPP) avec un protecteur d'obfuscation, ce qui
  rend l'analyse statique difficile.

Ensemble, ces protections sont conçues spécifiquement pour résister à la redirection du client vers un serveur
non officiel.

## Ce qui marche

- Le **protocole réseau est documenté** ([2 — Référence API](2-Reference-API.md)) : endpoints, sous-domaines,
  auth (OAuth/JWT/JWKS/OIDC), et la forme de la réponse du version-check.
- Un **serveur privé** (« CreateRoom ») peut répondre à ces requêtes — y compris un version-check « prêt à
  jouer » — avec des outils web standards. Le côté serveur d'un revival est atteignable.

## Ce qui est ouvert (le mur)

Faire utiliser un serveur privé au client **commercial** n'est pas résolu ici. En résumé :

- Les protections ci-dessus (pinning + le version-check propre au lanceur durci + l'anti-triche) verrouillent
  le client d'une façon qu'une personne seule ne peut pas contourner de façon responsable.
- C'est cohérent avec la communauté au sens large : connecter de façon fiable le client moderne à un serveur
  privé reste non résolu.

Cette documentation se concentre donc sur le **protocole et le côté serveur**, et traite la connexion du
client commercial comme un problème **d'échelle équipe, hors périmètre**.

## Conclusion honnête

- Le **serveur est la partie atteignable et réutilisable** — et la carte de protocole ici est exactement ce
  dont un serveur a besoin.
- **Jouer au client moderne sur un serveur privé est un effort collaboratif de longue haleine**, pas quelque
  chose qu'une personne seule peut finir. La voie réaliste est un effort communautaire collaboratif plus large.

## Communauté

- Rejoins la communauté **CreateRoom** sur Discord : <https://discord.gg/kVXNAqhmPf>.

---

_Projet d'interopérabilité / documentation autour d'un service en ligne disparu. Ne contient aucun code ou
asset du jeu et aucune instruction pour contourner des protections techniques._
