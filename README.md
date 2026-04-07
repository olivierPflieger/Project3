# Nom du Projet

Projet DataShare

# BackEnd

## Pré-requis pour le bon fonctionnement du backEnd :

- SDK .NET 8
- Docker Desktop
- Outil CLI Entity Framework Core (dotnet-ef). 
 
Pour installer CLI Entity Framework Core, éxécutez la commande suivante dans votre terminal :

```
dotnet tool install --global dotnet-ef --version 8.*
```     

## Guide de Démarrage du backEnd

Ce document explique comment lancer l'environnement de développement, 
incluant la base de données PostgreSQL via Docker et le serveur de l'API .NET.

## Vous venez de cloner le projet ? (Première utilisation)

1. Assurez-vous que Docker Desktop (ou le service Docker) est lancé sur votre machine.

2. Ouvrez un terminal à la racine du projet et exécutez :

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

## Utilisation générale

### Démarrer le backEnd et la base de données (Docker)

Assurez-vous que Docker Desktop (ou le service Docker) est lancé sur votre machine.  
Ouvrez un terminal à la racine du projet et exécutez :

```
docker-compose up -d
```

### Mettre à jour la base de données

Au besoin, afin d'appliquer les nouveaux changements de structure effectués sur la base de données, 
toujours dans le terminal à la racine du projet, exécutez :

```
dotnet ef database update
``` 

### Se connecter à la base de données

#### Via un outil graphique (Recommandé)

Utilisez un outil comme **DBeaver** ou **pgAdmin** avec les identifiants présents dans le fichier `docker-compose.yml` :
- **Hôte** : `localhost`
- **Port** : `5432`
- **Base de données** : *(voir `POSTGRES_DB` dans docker-compose.yml)*
- **Utilisateur** : *(voir `POSTGRES_USER` dans docker-compose.yml)*
- **Mot de passe** : *(voir `POSTGRES_PASSWORD` dans docker-compose.yml)*

#### Via la ligne de commande (CLI Docker)

Pour accéder directement à la base via le terminal du conteneur :

```
docker exec -it postgres_db psql -U admin -d datashare -W
```

*(Renseignez le mot de passe par la valeur réelle du fichier `docker-compose.yml`).*

#### Via Docker Desktop

- Dans Docker Desktop, développez l'arborescence du conteneur `project3-backend`.
- Cliquez sur le conteneur `postgres_db`, puis allez dans l'onglet Exec pour ouvrir une session terminal.
- Exécutez : 

```
psql -U admin -d datashare -W
```
*(Renseignez le mot de passe par la valeur réelle du fichier `docker-compose.yml`).*

#### Commandes utiles une fois connecté :
- `\dt` : Lister les tables
- `\d "table"` : Voir la structure d'une table
- `SELECT * FROM "table";` : Voir les données d'une table
- `\q` : Quitter

#### SWAGGER
Accédez à l'interface Swagger à l'adresse suivante :  
[http://localhost:5051/swagger/](http://localhost:5051/swagger/)

# FrontEnd

## Installation des dépendances

```bash
npm install
```

## Development server

Ouvrez un terminal sur le répertoire Project3/ClientApp et exécutez :

```bash
ng serve
```

Une fois le serveur démarré, naviguez vers `http://localhost:4200/`

## Configuration pour le développement local

### Jwt Key
Ce projet utilise les "User Secrets" de .NET pour gérer la clé secrète JWT en local sans la commiter sur Git. 
Après avoir cloné le dépôt, vous devez configurer votre propre clé JWT locale pour que l'authentification fonctionne.

Une fois la clé générée, ouvrez un terminal à la racine du projet backend et exécutez les commandes suivantes :

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "votre-cle-secrete-locale"
```

*Note : Assurez-vous que la clé locale fait au moins 256 bits (environ 32 caractères) pour que l'algorithme HMAC-SHA256 fonctionne correctement.*

Pour voir le contenu de vos secrets locaux, vous pouvez exécuter :

```bash
dotnet user-secrets list
```

### AWS Access Key

Ce projet utilise également les "User Secrets" pour gérer les clés d'accès AWS en local

Après avoir obtenu vos clés d'accès AWS, ouvrez un terminal à la racine du projet backend et exécutez les commandes suivantes :

```bash
dotnet user-secrets set "AWS:AccessKey" "VOTRE_ACCESS_KEY_ICI"
dotnet user-secrets set "AWS:SecretKey" "VOTRE_SECRET_KEY_ICI"
dotnet user-secrets set "AWS:Region" "eu-west-3"
```
