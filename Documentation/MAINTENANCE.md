# MAINTENANCE

## Objectif

Ce document décrit les procédures de maintenance de l'application (Angular frontend, API .NET Core, base de données PostgreSQL), ainsi que la fréquence des interventions et les risques associés.


## Procédures de mise à jour

### Frontend (Angular)

* Vérifier les mises à jour :

  ```bash
  npm outdated
  ```

* Mettre à jour les dépendances :

  ```bash
  npm update
  ```

* Mise à jour majeure Angular :

  ```bash
  ng update
  ```

---

### Backend (.NET Core)

* Vérifier les mises à jour :

  ```bash
  dotnet list package --outdated
  ```

* Mettre à jour les dépendances :

  ```bash
  dotnet add package <nom_package>
  ```

---

### Base de données (PostgreSQL)

* Appliquer les migrations :

  ```bash
  dotnet ef database update
  ```

* Sauvegarde :

  ```bash
  pg_dump -U <user> -d <database> > backup.sql
  ```

* Restauration :

  ```bash
  psql -U <user> -d <database> < backup.sql
  ```

---

## ⏱️ Fréquence de maintenance

* Hebdomadaire :

  * Vérification des logs backend
  * Surveillance des erreurs frontend

* Mensuelle :

  * Mise à jour des dépendances mineures (Angular, .NET)
  * Vérification des performances

* Trimestrielle :

  * Mise à jour majeure des frameworks
  * Audit de sécurité
  * Nettoyage de la base de données

---

## 👀 Surveillance

* Logs backend (.NET)

* Console navigateur (Angular)

* Base de données :

  * requêtes lentes
  * utilisation CPU / mémoire

* Outils possibles :

  * Serilog / logs fichiers
  * pgAdmin pour PostgreSQL

---

## ⚠️ Risques connus

* Incompatibilité lors de mises à jour majeures Angular ou .NET
* Perte de données en cas de mauvaise migration
* Dégradation des performances (requêtes SQL lentes)
* Mauvaise configuration en production

---

## 🔄 Plan de rollback

En cas de problème après déploiement :

1. Revenir à la version précédente :

   ```bash
   git checkout <version_precedente>
   ```

2. Restaurer la base de données :

   ```bash
   psql -U <user> -d <database> < backup.sql
   ```

3. Redéployer l'application

4. Analyser les logs pour identifier le problème

---

## 🔐 Bonnes pratiques

* Toujours tester en environnement de développement avant production
* Sauvegarder la base de données avant chaque migration
* Documenter les changements importants
* Utiliser un contrôle de version (Git)
