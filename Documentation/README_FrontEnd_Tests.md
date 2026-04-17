## Tests Cypress end-to-end 

ATTENTION : Pour l'éxecution des tests e2e, assurez-vous que le back-End ainsi que le front-End soit 
correctement démarré

Pour lancer les tests e2e sans couverture de code, ouvrir un terminal à la racine du projet DataShare_Web et éxécutez :

```bash
npm run e2e
```

Pour lancer les tests e2e avec couverture de code, ouvrir un terminal à la racine du projet DataShare_Web et éxécutez :

```bash
npm run cy:coverage
```

Au préalable, si vous souhaitez faire un clean du rapport de couverture, ouvrir un terminal à la racine 
du projet DataShare_Web et éxécutez :

```bash
npm run coverage:clean
```

Pour générer un rapport de couverture, une fois les tests e2e executés, ouvrir un terminal à la racine 
du projet DataShare_Web et éxécutez :

```bash
npm run coverage:report
```

Le rapport est disponible dans le répertoire `/coverage/combined/index.html`.

## Console Cypress

Pour ouvrir la console Cypress, ouvrir un terminal à la racine du projet DataShare_Web et éxécutez :

```bash
npx cypress open
```

## Tests de performances K6

### Pré-requis

1. Assurez-vous que Docker Desktop (ou le service Docker) est lancé sur votre machine.

2. Assurez-vous que le back-End soit correctement lancé

Available commands:

```bash
npm run perf:smoke
npm run perf:load
```

These commands run the scenario defined in `perf/upload-download.js` against the backend and cover:

- test user registration
- login and JWT retrieval
- authenticated file upload
- password-protected file download

By default, Docker reaches the host backend through `http://host.docker.internal:5051`.
Each run also exports an HTML report to `perf/reports/` with a timestamped filename.

Supported environment variables:

```bash
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

Examples:

```bash
npm run perf:smoke
$env:K6_VUS=10; $env:K6_DURATION='2m'; npm run perf:load
$env:K6_REPORT_FILE='perf/reports/load-latest.html'; npm run perf:load
$env:K6_UPLOAD_FILE='perf/fixtures/upload-test.txt'; npm run perf:k6 -- load
```

Open the generated report on Windows with:

```bash
start .\perf\reports\k6-smoke-YYYYMMDD-HHmmss.html
```

Interpreting the results:

- `http_req_failed` should stay close to `0%`
- `http_req_duration` is tagged by operation so upload and download thresholds are reported separately
- `checks` must stay above `99%`

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
