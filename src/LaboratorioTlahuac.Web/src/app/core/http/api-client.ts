import { HttpClient } from '@angular/common/http';
import { inject, Injectable, InjectionToken } from '@angular/core';

export interface HealthResponse {
  status: string;
  application: string;
}

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => ''
});

@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getHealth() {
    return this.http.get<HealthResponse>(`${this.baseUrl}/health`, {
      withCredentials: true
    });
  }

  getUrl(path: string) {
    return `${this.baseUrl}${path}`;
  }
}
