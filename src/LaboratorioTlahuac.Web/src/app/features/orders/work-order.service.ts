import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { ApiClient } from '../../core/http/api-client';
import {
  WorkOrderChangeStatusRequest,
  WorkOrderDetail,
  WorkOrderListItem,
  WorkOrderListParams,
  WorkOrderPagedResponse,
  WorkOrderStatusOption,
  WorkOrderUpsertRequest
} from './work-order.models';

@Injectable({ providedIn: 'root' })
export class WorkOrderService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);
  private readonly authService = inject(AuthService);

  list(params: WorkOrderListParams = {}): Observable<WorkOrderPagedResponse<WorkOrderListItem>> {
    return this.http.get<WorkOrderPagedResponse<WorkOrderListItem>>(
      this.apiClient.getUrl('/api/work-orders'),
      {
        params: this.toListParams(params),
        withCredentials: true
      }
    );
  }

  getById(id: string): Observable<WorkOrderDetail> {
    return this.http.get<WorkOrderDetail>(this.apiClient.getUrl(`/api/work-orders/${id}`), {
      withCredentials: true
    });
  }

  create(request: WorkOrderUpsertRequest): Observable<WorkOrderDetail> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<WorkOrderDetail>(this.apiClient.getUrl('/api/work-orders'), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  update(id: string, request: WorkOrderUpsertRequest): Observable<WorkOrderDetail> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<WorkOrderDetail>(this.apiClient.getUrl(`/api/work-orders/${id}`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  changeStatus(id: string, request: WorkOrderChangeStatusRequest): Observable<WorkOrderDetail> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<WorkOrderDetail>(
          this.apiClient.getUrl(`/api/work-orders/${id}/status`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  getStatuses(): Observable<WorkOrderStatusOption[]> {
    return this.http.get<WorkOrderStatusOption[]>(this.apiClient.getUrl('/api/work-orders/statuses'), {
      withCredentials: true
    });
  }

  private toListParams(params: WorkOrderListParams): HttpParams {
    let httpParams = new HttpParams();

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.customerId) {
      httpParams = httpParams.set('customerId', params.customerId);
    }

    if (params.internalDoctorId) {
      httpParams = httpParams.set('internalDoctorId', params.internalDoctorId);
    }

    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }

    if (params.receivedDateFrom) {
      httpParams = httpParams.set('receivedDateFrom', params.receivedDateFrom);
    }

    if (params.receivedDateTo) {
      httpParams = httpParams.set('receivedDateTo', params.receivedDateTo);
    }

    if (params.deliveryDateFrom) {
      httpParams = httpParams.set('deliveryDateFrom', params.deliveryDateFrom);
    }

    if (params.deliveryDateTo) {
      httpParams = httpParams.set('deliveryDateTo', params.deliveryDateTo);
    }

    if (params.includeCancelled !== undefined) {
      httpParams = httpParams.set('includeCancelled', String(params.includeCancelled));
    }

    if (params.page) {
      httpParams = httpParams.set('page', String(params.page));
    }

    if (params.pageSize) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }

    return httpParams;
  }
}
