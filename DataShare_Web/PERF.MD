# Performance Testing

Ce projet utilise `k6` via Docker pour exécuter le scénario de performance défini dans [perf/upload-download.js](/D:/_OpenClassRooms_/Project3/DataShare_Web/perf/upload-download.js).

Le scénario couvre :

- création d'un utilisateur de test
- authentification et récupération du token
- upload d'un fichier authentifié
- download protégé par mot de passe
- suppression du fichier

Chaque run exporte automatiquement un rapport HTML dans `perf/reports/`.

## Prérequis

- Docker Desktop démarré
- backend accessible sur `http://localhost:5051`

Par défaut, le wrapper Docker cible `http://host.docker.internal:5051`.

## Commandes utiles

Lancer un test smoke :

```powershell
npm run perf:smoke
```

Lancer un test load :

```powershell
npm run perf:load
```

Choisir explicitement le profil :

```powershell
npm run perf:k6 -- load
```

Forcer un chemin de rapport HTML :

```powershell
$env:K6_REPORT_FILE="perf/reports/load-latest.html"
npm run perf:load
```

Ouvrir un rapport sous Windows :

```powershell
start .\perf\reports\k6-load-YYYYMMDD-HHmmss.html
```

## Variables d'environnement

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
K6_UPLOAD_RESPONSE_P95=2000
K6_DOWNLOAD_RESPONSE_P95=2000
```

Exemple avec plus de VUs :

```powershell
$env:K6_VUS="10"
$env:K6_DURATION="1m"
npm run perf:load
```

Exemple avec un fichier plus lourd :

```powershell
$env:K6_VUS="5"
$env:K6_DURATION="1m"
$env:K6_UPLOAD_FILE="perf/fixtures/upload-test-5mb.bin"
npm run perf:load
```

## Campagne progressive

Le script [scripts/run-k6-campaign.ps1](/D:/_OpenClassRooms_/Project3/DataShare_Web/scripts/run-k6-campaign.ps1) exécute une campagne complète et affiche à la fin les chemins des rapports générés.

Runs inclus :

- `5 VUs`, fichier de base, `1m`
- `10 VUs`, fichier de base, `1m`
- `20 VUs`, fichier de base, `2m`
- `5 VUs`, fichier `5 Mo`, `3m`


Le script crée automatiquement si besoin :

- [perf/fixtures/upload-test-5mb.bin](/D:/_OpenClassRooms_/Project3/DataShare_Web/perf/fixtures/upload-test-5mb.bin)
- [perf/fixtures/upload-test-10mb.bin](/D:/_OpenClassRooms_/Project3/DataShare_Web/perf/fixtures/upload-test-10mb.bin)

Lancer la campagne :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-k6-campaign.ps1
```

Vérifier la campagne sans exécuter K6 :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-k6-campaign.ps1 -DryRun
```

Les rapports sont rangés dans un sous-dossier du type :

```text
perf/reports/campaign-YYYYMMDD-HHmmss/
```

## Lecture rapide des résultats

Points à regarder dans la sortie console ou le rapport HTML :

- `checks`
  - doit rester proche de `100%`
- `http_req_failed`
  - doit rester proche de `0%`
- `http_req_duration{operation:upload}`
  - temps de réponse des uploads
- `http_req_duration{operation:download}`
  - temps de réponse des downloads
- `p(95)` ou `P95`
  - 95% des requêtes sont sous cette durée

Interprétation simple :

- checks OK + erreurs HTTP à `0%` : scénario fonctionnellement valide
- `P95` sous les seuils : performance acceptable selon le budget défini
- `P95` qui monte fortement quand les VUs montent : saturation progressive
- erreurs HTTP qui apparaissent : limite de stabilité atteinte

## Notes

- `smoke` utilise un faible volume pour valider rapidement le scénario
- `load` fait tourner plusieurs utilisateurs virtuels en parallèle pendant une durée donnée
- les rapports HTML sont générés automatiquement à chaque run
