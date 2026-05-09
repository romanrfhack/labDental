import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { ApiClient } from '../../core/http/api-client';
import {
  PaymentCreateRequest,
  PaymentListItem,
  PaymentListParams,
  PaymentMethodOption,
  PaymentMutationResponse,
  PaymentOption,
  PaymentPagedResponse,
  PaymentSummary,
  WorkOrderPayment,
  WorkOrderPaymentListParams
} from './payment.models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);
  private readonly authService = inject(AuthService);

  list(params: PaymentListParams = {}): Observable<PaymentPagedResponse<PaymentListItem>> {
    return this.http.get<PaymentPagedResponse<PaymentListItem>>(this.apiClient.getUrl('/api/payments'), {
      params: this.toListParams(params),
      withCredentials: true
    });
  }

  listForWorkOrder(
    workOrderId: string,
    params: WorkOrderPaymentListParams = {}
  ): Observable<WorkOrderPayment[]> {
    let httpParams = new HttpParams();

    if (params.includeCancelled !== undefined) {
      httpParams = httpParams.set('includeCancelled', String(params.includeCancelled));
    }

    return this.http.get<WorkOrderPayment[]>(
      this.apiClient.getUrl(`/api/work-orders/${workOrderId}/payments`),
      {
        params: httpParams,
        withCredentials: true
      }
    );
  }

  getSummary(workOrderId: string): Observable<PaymentSummary> {
    return this.http.get<PaymentSummary>(
      this.apiClient.getUrl(`/api/work-orders/${workOrderId}/payments/summary`),
      {
        withCredentials: true
      }
    );
  }

  create(workOrderId: string, request: PaymentCreateRequest): Observable<PaymentMutationResponse> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<PaymentMutationResponse>(
          this.apiClient.getUrl(`/api/work-orders/${workOrderId}/payments`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  cancel(workOrderId: string, paymentId: string, reason: string): Observable<PaymentMutationResponse> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<PaymentMutationResponse>(
          this.apiClient.getUrl(`/api/work-orders/${workOrderId}/payments/${paymentId}/cancel`),
          { reason },
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  getMethods(): Observable<PaymentMethodOption[]> {
    return this.http.get<PaymentMethodOption[]>(this.apiClient.getUrl('/api/payments/methods'), {
      withCredentials: true
    });
  }

  getStatuses(): Observable<PaymentOption[]> {
    return this.http.get<PaymentOption[]>(this.apiClient.getUrl('/api/payments/statuses'), {
      withCredentials: true
    });
  }

  private toListParams(params: PaymentListParams): HttpParams {
    let httpParams = new HttpParams();

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.customerId) {
      httpParams = httpParams.set('customerId', params.customerId);
    }

    if (params.workOrderId) {
      httpParams = httpParams.set('workOrderId', params.workOrderId);
    }

    if (params.method) {
      httpParams = httpParams.set('method', params.method);
    }

    if (params.paymentDateFrom) {
      httpParams = httpParams.set('paymentDateFrom', params.paymentDateFrom);
    }

    if (params.paymentDateTo) {
      httpParams = httpParams.set('paymentDateTo', params.paymentDateTo);
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
