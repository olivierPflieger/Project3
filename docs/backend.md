# DataShare Back-end

## Pré-requis pour le bon fonctionnement du Back-end

- [SDK .NET 8](https://dotnet.microsoft.com/fr-fr/download/dotnet/8.0)
- [Docker Desktop version 4.65.0](https://docs.docker.com/desktop/setup/install/windows-install)
- CLI Entity Framework Core (dotnet-ef)

Pour installer CLI Entity Framework Core, ouvrir une console Powershell et éxécutez :

```
dotnet tool install --global dotnet-ef --version 8.*
```     

## Utilisation générale (hors première utilisation)

Démarrez Docker Desktop (ou le service Docker)

Ouvrez une console Powershell à la racine du projet DataShare_API et exécutez :

```
.\start-database.ps1
```
Puis, pour démarrer le serveur

```
dotnet run
```

## Première utilisation

Vous venez de cloner le projet GIT. Suivez les instructions suivantes pour configurer et démarrer le projet back-End pour la 
première fois.

### Variables d'environnement

Ce projet utilise les "User Secrets" de .NET pour gérer les variables d'environnement.

Après avoir cloné le dépôt, vous devez configurer vos propres variables d'environnement pour que le projet puisse démarrer correctement.

#### Jwt Key

Pour générer une clé, utilisez par exemple OpenSSL. Ouvrer un terminal (bash ou cygwin64) et exécutez :

```
openssl rand -base64 32
```
*Note : Assurez-vous que la clé locale fait au moins 256 bits (environ 32 caractères) pour que l'algorithme HMAC-SHA256 fonctionne correctement.*

Une fois la clé générée, ouvrez une console Powershell à la racine du projet DataShare_API et exécutez les commandes suivantes :

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "votre-cle-secrete-locale"
```

#### AWS Access Key

L'accès au bucket est fait via un compte IAM en place dans AWS.
Ouvrez une console Powershell à la racine du projet DataShare_API et exécutez les commandes suivantes :

```bash
dotnet user-secrets set "AWS:AccessKey" "ACCESS_KEY"
dotnet user-secrets set "AWS:SecretKey" "SECRET_KEY"
dotnet user-secrets set "AWS:Region" "eu-west-3"
```

#### Database identifiants

Ouvrez une console Powershell à la racine du projet backend et exécutez les commandes suivantes :

```bash
dotnet user-secrets set "POSTGRES_HOST" "localhost"
dotnet user-secrets set "POSTGRES_PORT" "5432"
dotnet user-secrets set "POSTGRES_DB" "datashare"
dotnet user-secrets set "POSTGRES_USER" "toChange"
dotnet user-secrets set "POSTGRES_PASSWORD" "toChange"
```

#### Vérification

Une fois en place, afin de vérifier que les variables soient bien configurées, vous pouvez exécuter :

```bash
dotnet user-secrets list
```
Les variables suivantes sont en place

```
POSTGRES_USER = ...
POSTGRES_PORT = ...
POSTGRES_PASSWORD = ...
POSTGRES_HOST = ...
POSTGRES_DB = ...
Jwt:Key = ...
AWS:SecretKey = ...
AWS:Region = ...
AWS:AccessKey = ...
```

#### Démarrez le back-End

1. Démarrez Docker Desktop (ou le service Docker)

2. Pour démarrer la database et l'API, ouvrez une console Powershell à la racine du projet DataShare_API et exécutez :

```
./start-database.ps1
```
Puis, pour lancer le serveur, exécutez : 

```
dotnet run
```

3. Une fois l'API démarrée, l'interface Swagger qui décrit l'API est disponible à l'adresse suivante :

```
http://localhost:5051/swagger/
```

## Mettre à jour la base de données

Les modifications de structure de la base de données devront faite l'objet d'un commentaire précis dans le commit

```
feat!: Migration requise - ...
```

Dans ce cas, afin d'appliquer les nouveaux changements effectués sur la base de données, stoppez l'application et éxecutez :

```
./start-database.ps1
```
Puis, pour relancer le serveur, éxecutez : 

```
dotnet run
```

## Se connecter à la base de données

### Via un outil graphique

Utilisez un outil comme **DBeaver**, **pgAdmin** ou **heidiSQL** avec les identifiants présents dans les variables d'environnement

- **Hôte** : `localhost`
- **Port** : `5432`
- **Base de données** : *(voir `POSTGRES_DB` dans docker-compose.yml)*
- **Utilisateur** : *(voir `POSTGRES_USER` dans docker-compose.yml)*
- **Mot de passe** : *(voir `POSTGRES_PASSWORD` dans docker-compose.yml)*

### Via la ligne de commande (CLI Docker)

Pour accéder directement à la base via un terminal:

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

Pour supprimer toutes les entrées dans la database ainsi que supprimer tous les fichiers dans AWS, ouvrez une console Powershell dans le répertoire DataShare_API\Scripts et exécutez :

```
.\clean_database_and_bucket.ps1
```

## SWAGGER

Accédez à l'interface Swagger à l'adresse suivante :  
[http://localhost:5051/swagger/](http://localhost:5051/swagger/)


## POSTMAN

La collection Postman utilisée pour ce projet est disponible dans \Resources\Project3.postman_collection.json

# Tests unitaires et d'intégration du back-End

Ce document explique comment éxecuter les tests du Back-End

[Voir la documentation](TESTING_BackEnd.md)