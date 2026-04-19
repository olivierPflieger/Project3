## Tests de performances K6

Ce projet utilise `k6` via Docker pour exécuter le scénario de performance défini dans [perf/upload-download.js](/D:/_OpenClassRooms_/Project3/DataShare_Web/perf/upload-download.js).

Le scénario couvre :

- La création d'un utilisateur de test
- L'authentification et récupération du token
- L'upload d'un fichier authentifié de taille aléatoire fixée entre valeur min-max
- Le download protégé par mot de passe
- La suppression du fichier

### Pré-requis

1. Assurez-vous que Docker Desktop (ou le service Docker) est lancé sur votre machine.

Par défaut, le wrapper Docker cible `http://host.docker.internal:5051`.

2. Assurez-vous que le back-End soit correctement lancé


## Exécuter la campagne de tests

Pour lancer la campagne de tests, ouvrir un terminal à la racine du projet DataShare_Web et éxécutez :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-k6-campaign.ps1
```

Pour vérifier la campagne sans exécuter K6 :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-k6-campaign.ps1 -DryRun
```

Les rapports sont rangés dans le sous-dossier :

```text
perf/reports/campaign-YYYYMMDD-HHmmss/
```

Pour chaque rapport HTML, la campagne génère aussi :

```text
01-random-file-50vu.summary.md
01-random-file-50vu.summary.json
```
## Configuration de la campagne

Le wrapper [scripts/run-k6.cjs](/D:/_OpenClassRooms_/Project3/DataShare_Web/scripts/run-k6.cjs) prend en charge :

```powershell
K6_BASE_URL=http://host.docker.internal:5051
K6_UPLOAD_FILE=perf/fixtures/upload-test.txt
K6_REPORT_FILE=perf/reports/custom-report.html
K6_VUS=5
K6_DURATION=1m
K6_ITERATIONS=1
K6_FILE_PASSWORD=k6-secret
K6_FILE_TAGS=k6,perf
K6_FILE_EXPIRATION=1
K6_UPLOAD_RANDOM_RANGE=true
K6_UPLOAD_MIN_MB=1
K6_UPLOAD_MAX_MB=100
K6_UPLOAD_SOURCE_MAX_MB=100
K6_UPLOAD_RESPONSE_P95=2000
K6_DOWNLOAD_RESPONSE_P95=2000
K6_SUMMARY_FILE=perf/reports/custom-summary.md
K6_SUMMARY_JSON_FILE=perf/reports/custom-summary.json
```

Exemple avec 50 VUS, pour une durée de 2mn et une taille aléatoire entre `1 MiB` et `10 MiB` a chaque iteration :

```powershell
$env:K6_VUS="50"
$env:K6_DURATION="2m"
$env:K6_UPLOAD_RANDOM_RANGE="true"
$env:K6_UPLOAD_MIN_MB="1"
$env:K6_UPLOAD_MAX_MB="100"
$env:K6_UPLOAD_SOURCE_MAX_MB="10"
$env:K6_UPLOAD_RESPONSE_P95="10000"
$env:K6_DOWNLOAD_RESPONSE_P95="10000"
```

Exemple avec 20 VUS pour une durée de 3mn et une taille aléatoire entre `1 MiB` et `50 MiB` a chaque iteration :

```powershell
$env:K6_VUS="20"
$env:K6_DURATION="3m"
$env:K6_UPLOAD_RANDOM_RANGE="true"
$env:K6_UPLOAD_MIN_MB="1"
$env:K6_UPLOAD_MAX_MB="50"
$env:K6_UPLOAD_SOURCE_MAX_MB="100"
$env:K6_UPLOAD_RESPONSE_P95="15000"
$env:K6_DOWNLOAD_RESPONSE_P95="15000"
```