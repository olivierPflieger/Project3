import * as Sentry from "@sentry/angular";
import {
  ApplicationConfig,
  provideZoneChangeDetection,
  ErrorHandler,
  provideAppInitializer,
  inject,
} from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { authInterceptor } from './core/interceptors/auth.interceptor';

import { routes } from './app.routes';
import {provideHttpClient, withInterceptors} from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    {
      provide: ErrorHandler,
      useValue: Sentry.createErrorHandler()
    },
    {
      provide: Sentry.TraceService,
      deps: [Router]
    },
    provideAppInitializer(() => {
      inject(Sentry.TraceService);
    })
  ]
};