import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClient } from '../../core/http/api-client';
import { DashboardSummary } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);

  getSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(this.apiClient.getUrl('/api/dashboard/summary'), {
      withCredentials: true
    });
  }
}
