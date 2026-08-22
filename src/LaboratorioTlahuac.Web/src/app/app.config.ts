import { provideHttpClient, withFetch, withInterceptors, withXsrfConfiguration } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { TitleStrategy, provideRouter } from '@angular/router';

import { environment } from '../environments/environment';
import { authExpiredInterceptor } from './core/interceptors/auth-expired.interceptor';
import { API_BASE_URL } from './core/http/api-client';
import { AppTitleStrategy } from './core/seo/app-title-strategy';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    { provide: TitleStrategy, useClass: AppTitleStrategy },
    provideHttpClient(
      withFetch(),
      withInterceptors([authExpiredInterceptor]),
      withXsrfConfiguration({
        cookieName: 'XSRF-TOKEN',
        headerName: 'X-XSRF-TOKEN'
      })
    ),
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl }
  ]
};
