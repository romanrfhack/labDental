import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { ApiClient } from '../../core/http/api-client';
import {
  CustomerDetail,
  CustomerListItem,
  CustomerListParams,
  CustomerStatusRequest,
  CustomerUpsertRequest,
  InternalDoctor,
  InternalDoctorListParams,
  InternalDoctorUpsertRequest,
  PagedResponse
} from './customer.models';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);
  private readonly authService = inject(AuthService);

  list(params: CustomerListParams = {}): Observable<PagedResponse<CustomerListItem>> {
    return this.http.get<PagedResponse<CustomerListItem>>(this.apiClient.getUrl('/api/customers'), {
      params: this.toCustomerListParams(params),
      withCredentials: true
    });
  }

  getById(id: string): Observable<CustomerDetail> {
    return this.http.get<CustomerDetail>(this.apiClient.getUrl(`/api/customers/${id}`), {
      withCredentials: true
    });
  }

  create(request: CustomerUpsertRequest): Observable<CustomerDetail> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<CustomerDetail>(this.apiClient.getUrl('/api/customers'), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  update(id: string, request: CustomerUpsertRequest): Observable<CustomerDetail> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<CustomerDetail>(this.apiClient.getUrl(`/api/customers/${id}`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  updateStatus(id: string, isActive: boolean): Observable<CustomerDetail> {
    const request: CustomerStatusRequest = { isActive };

    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<CustomerDetail>(this.apiClient.getUrl(`/api/customers/${id}/status`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  listInternalDoctors(
    customerId: string,
    params: InternalDoctorListParams = {}
  ): Observable<InternalDoctor[]> {
    return this.http.get<InternalDoctor[]>(
      this.apiClient.getUrl(`/api/customers/${customerId}/internal-doctors`),
      {
        params: this.toInternalDoctorListParams(params),
        withCredentials: true
      }
    );
  }

  createInternalDoctor(
    customerId: string,
    request: InternalDoctorUpsertRequest
  ): Observable<InternalDoctor> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<InternalDoctor>(
          this.apiClient.getUrl(`/api/customers/${customerId}/internal-doctors`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  updateInternalDoctor(
    customerId: string,
    doctorId: string,
    request: InternalDoctorUpsertRequest
  ): Observable<InternalDoctor> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<InternalDoctor>(
          this.apiClient.getUrl(`/api/customers/${customerId}/internal-doctors/${doctorId}`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  updateInternalDoctorStatus(
    customerId: string,
    doctorId: string,
    isActive: boolean
  ): Observable<InternalDoctor> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<InternalDoctor>(
          this.apiClient.getUrl(`/api/customers/${customerId}/internal-doctors/${doctorId}/status`),
          { isActive },
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  private toCustomerListParams(params: CustomerListParams) {
    let httpParams = new HttpParams();

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.type) {
      httpParams = httpParams.set('type', params.type);
    }

    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', String(params.isActive));
    }

    if (params.page) {
      httpParams = httpParams.set('page', String(params.page));
    }

    if (params.pageSize) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }

    return httpParams;
  }

  private toInternalDoctorListParams(params: InternalDoctorListParams) {
    let httpParams = new HttpParams();

    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', String(params.isActive));
    }

    return httpParams;
  }
}
