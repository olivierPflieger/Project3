# EtudiantFrontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 19.2.16.

## Dependances installation

```bash
npm install
```

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Jest](https://jestjs.io/) test runner, use the following command:
Coverage report is generated in ..\coverage

```bash
npm run jest
```

## Running end-to-end tests

NOTE : Ensure both backEnd and frontEnd are running
to run backEnd, check README.md in BackEnd solution
to run frontEnd, run ng serve

For end-to-end (e2e) testing without coverage, run:

```bash
npm run e2e
```

For end-to-end (e2e) testing with coverage, run:

```bash
npm run cy:coverage
```

This command builds the app in development mode, instruments the generated JavaScript, serves the instrumented build on `http://127.0.0.1:4201`, and runs Cypress against it.

Additionally, if you need to clean the coverage report, run :

```bash
npm run coverage:clean
```

To generate the merged report after `npm run jest` and `npm run cy:coverage`, run:

```bash
npm run coverage:report
```

The combined coverage report is generated in `coverage/combined`.

In order to open Cypress console, run : 

```bash
npx cypress open
```

## Running performance tests with K6

K6 performance tests are executed through Docker so no local K6 binary is required.

Prerequisites:

- Docker Desktop must be running
- the backend API must be reachable on `http://localhost:5051`

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

Supported environment variables:

```bash
K6_BASE_URL=http://host.docker.internal:5051
K6_UPLOAD_FILE=perf/fixtures/upload-test.txt
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
$env:K6_UPLOAD_FILE='perf/fixtures/upload-test.txt'; npm run perf:k6 -- load
```

Interpreting the results:

- `http_req_failed` should stay close to `0%`
- `http_req_duration` is tagged by operation so upload and download thresholds are reported separately
- `checks` must stay above `99%`

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
