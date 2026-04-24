import * as Sentry from "@sentry/angular";
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

Sentry.init({
    dsn: "https://8bf2cecee8d72280d2a547e595ee7fa6@o4511275001643008.ingest.de.sentry.io/4511275002167376",
    integrations: [Sentry.browserTracingIntegration()],
    tracesSampleRate: 1,
    enableLogs: true,
    sendDefaultPii: true
})

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));