# DataShare Back-End

## Pré-requis pour le bon fonctionnement du backEnd

- SDK .NET 8
- Docker Desktop
- Outil CLI Entity Framework Core (dotnet-ef). 
 
Pour installer CLI Entity Framework Core, ouvrir un terminal et éxécutez :

```
dotnet tool install --global dotnet-ef --version 8.*
```     

## Guide de Démarrage du backEnd

Ce document explique comment lancer l'environnement de développement, 
incluant la base de données PostgreSQL via Docker et le serveur de l'API .NET.

## Vous venez de cloner le projet ? (Première utilisation)

1. Assurez-vous que Docker Desktop (ou le service Docker) est lancé sur votre machine.

2. Ouvrez un terminal à la racine du projet DataShare_API et exécutez :

```
docker-compose up -d
```

3. Puis exécutez : (**Obligatoire** la première fois, pour créer les tables à partir des fichiers de migration existants dans le code).
 
```
dotnet ef database update
``` 

4. Enfin, afin de démarrer l'API, exécutez :

```
dotnet run
``` 

5. Une fois l'API démarrée, accédez à l'interface Swagger à l'adresse suivante :  

```
http://localhost:5051/swagger/
```

## Utilisation générale (hors première utilisation)

### Démarrer le backEnd et la base de données (Docker)

Assurez-vous que Docker Desktop (ou le service Docker) est lancé sur votre machine.  
Ouvrez un terminal à la racine du projet DataShare_API et exécutez :

```
docker-compose up -d
```

### Mettre à jour la base de données

Au besoin, afin d'appliquer les nouveaux changements de structure effectués sur la base de données, 
toujours dans le terminal à la racine du projet DataShare_API, exécutez :

```
dotnet ef database update
``` 

## Se connecter à la base de données

### Via un outil graphique (Recommandé)

Utilisez un outil comme **DBeaver**, **pgAdmin** ou **heidiSQL** avec les identifiants présents dans le fichier `docker-compose.yml` :

- **Hôte** : `localhost`
- **Port** : `5432`
- **Base de données** : *(voir `POSTGRES_DB` dans docker-compose.yml)*
- **Utilisateur** : *(voir `POSTGRES_USER` dans docker-compose.yml)*
- **Mot de passe** : *(voir `POSTGRES_PASSWORD` dans docker-compose.yml)*

### Via la ligne de commande (CLI Docker)

Pour accéder directement à la base via le terminal du conteneur :

```
docker exec -it postgres_db psql -U admin -d datashare -W
```

*(Renseignez le mot de passe par la valeur réelle du fichier `docker-compose.yml`).*

### Via Docker Desktop

- Dans Docker Desktop, développez l'arborescence du conteneur `datashare_api`.
- Cliquez sur le conteneur `postgres_db`, puis allez dans l'onglet Exec pour ouvrir une session terminal.
- Exécutez : 

```
psql -U admin -d datashare -W
```
*(Renseignez le mot de passe par la valeur réelle du fichier `docker-compose.yml`).*

### Commandes utiles une fois connecté :
- `\dt` : Lister les tables
- `\d "table"` : Voir la structure d'une table
- `SELECT * FROM "table";` : Voir les données d'une table
- 'DELETE FROM "table";
- `\q` : Quitter

### Cleaning de la base

Pour supprimer toutes les entrées dans la database ainsi que supprimer tous les fichiers dans AWS,
ouvrez un terminal dans le répertoire DataShare_API\Scripts et exécutez :

```
cd Scripts
.\clean_database_and_bucket.ps1
```

## SWAGGER

Accédez à l'interface Swagger à l'adresse suivante :  
[http://localhost:5051/swagger/](http://localhost:5051/swagger/)

## POSTMAN

La collection Postman utilisée pour ce projet est disponible dans \Resources\Project3.postman_collection.json

# Tests unitaires et d'intégration du back-End

Ce document explique comment éxecuter les tests du Back-End

[Voir la documentation](README_BackEnd.md)