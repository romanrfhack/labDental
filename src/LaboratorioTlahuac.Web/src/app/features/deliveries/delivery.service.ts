import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { ApiClient } from '../../core/http/api-client';
import {
  DeliveryAssignRequest,
  DeliveryCompleteRequest,
  DeliveryCreateRequest,
  DeliveryFailedRequest,
  DeliveryListParams,
  DeliveryOutForDeliveryRequest,
  DeliveryPagedResponse,
  DeliveryResponse
} from './delivery.models';

@Injectable({ providedIn: 'root' })
export class DeliveryService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);
  private readonly authService = inject(AuthService);

  list(params: DeliveryListParams = {}): Observable<DeliveryPagedResponse<DeliveryResponse>> {
    return this.http.get<DeliveryPagedResponse<DeliveryResponse>>(this.apiClient.getUrl('/api/deliveries'), {
      params: this.toListParams(params),
      withCredentials: true
    });
  }

  getById(id: string): Observable<DeliveryResponse> {
    return this.http.get<DeliveryResponse>(this.apiClient.getUrl(`/api/deliveries/${id}`), {
      withCredentials: true
    });
  }

  getByWorkOrderId(workOrderId: string): Observable<DeliveryResponse> {
    return this.http.get<DeliveryResponse>(this.apiClient.getUrl(`/api/work-orders/${workOrderId}/delivery`), {
      withCredentials: true
    });
  }

  createForWorkOrder(
    workOrderId: string,
    request: DeliveryCreateRequest = { deliveryNotes: null }
  ): Observable<DeliveryResponse> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<DeliveryResponse>(
          this.apiClient.getUrl(`/api/work-orders/${workOrderId}/delivery`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  assign(id: string, request: DeliveryAssignRequest): Observable<DeliveryResponse> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<DeliveryResponse>(this.apiClient.getUrl(`/api/deliveries/${id}/assign`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  markOutForDelivery(
    id: string,
    request: DeliveryOutForDeliveryRequest = { deliveryNotes: null }
  ): Observable<DeliveryResponse> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<DeliveryResponse>(
          this.apiClient.getUrl(`/api/deliveries/${id}/out-for-delivery`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  complete(id: string, request: DeliveryCompleteRequest): Observable<DeliveryResponse> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<DeliveryResponse>(this.apiClient.getUrl(`/api/deliveries/${id}/complete`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  markFailed(id: string, request: DeliveryFailedRequest): Observable<DeliveryResponse> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<DeliveryResponse>(this.apiClient.getUrl(`/api/deliveries/${id}/failed`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  private toListParams(params: DeliveryListParams): HttpParams {
    let httpParams = new HttpParams();

    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }

    if (params.assignedToMe !== undefined) {
      httpParams = httpParams.set('assignedToMe', String(params.assignedToMe));
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
