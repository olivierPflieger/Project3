# Politique de sécurité

## Versions supportées

Dans le cadre de ce MVP, uniquement la branche `main` est maintenue.

## Pratiques de sécurité

Ce projet utilise les fonctionnalités avancées de sécurité proposées par GitHub afin de garantir la qualité et la sécurité du code.

## Analyse statique du code

**CodeQL** est utilisé pour effectuer des analyses automatiques du code.
Ces analyses sont exécutées automatiquement sur les *pull requests* ainsi que régulièrement sur le dépôt.

## Sécurité des dépendances

Les outils intégrés de GitHub sont utilisés :

* `Dependabot alerts` et `Dependabot malware` sont activés afin de générer des alertes.
* `Dependabot version updates` est désactivé, afin de ne pas risquer de générer de régression dans l'application.
En cas de mise en production, ce point devra faire l'objet d'une prise de décision.


## Détection de secrets

La détection de secrets est activée afin d’éviter l’exposition accidentelle d’informations sensibles, telles que :

* clés API
* tokens
* identifiants


## Protection des pull requests

Des contrôles de sécurité sont appliqués avant toute intégration :

* Niveau d'alertes de sécurité : High and higher
* Niveau d'alertes standard : Only errors