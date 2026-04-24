
 
import { Component } from '@angular/core';
import * as Sentry from "@sentry/angular";

@Component({
  selector: 'app-sentry-exemple',
  template: `<button (click)="throwError()">Test Sentry</button>`
})
export class SentryExempleComponent {

  throwError() {
    throw new Error('🔥 Test Sentry Angular');
  }

  sendManualError() {
    Sentry.captureException(new Error('Erreur manuelle'));
  }
}