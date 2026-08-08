import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { ApiClient } from '../../core/http/api-client';
import {
  CatalogPriceRequest,
  CatalogProduct,
  CatalogProductUpsertRequest,
  CatalogSection,
  CatalogSectionUpsertRequest,
  CatalogStatusRequest
} from './catalog.models';

@Injectable({ providedIn: 'root' })
export class AdminCatalogService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);
  private readonly authService = inject(AuthService);

  getSections(): Observable<CatalogSection[]> {
    return this.http.get<CatalogSection[]>(this.apiClient.getUrl('/api/admin/catalog/sections'), {
      withCredentials: true
    });
  }

  createSection(request: CatalogSectionUpsertRequest): Observable<CatalogSection> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<CatalogSection>(this.apiClient.getUrl('/api/admin/catalog/sections'), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  updateSection(id: string, request: CatalogSectionUpsertRequest): Observable<CatalogSection> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<CatalogSection>(this.apiClient.getUrl(`/api/admin/catalog/sections/${id}`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  setSectionStatus(id: string, request: CatalogStatusRequest): Observable<CatalogSection> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<CatalogSection>(
          this.apiClient.getUrl(`/api/admin/catalog/sections/${id}/status`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  getProducts(sectionId?: string): Observable<CatalogProduct[]> {
    const params = sectionId ? new HttpParams().set('sectionId', sectionId) : undefined;

    return this.http.get<CatalogProduct[]>(this.apiClient.getUrl('/api/admin/catalog/products'), {
      params,
      withCredentials: true
    });
  }

  createProduct(request: CatalogProductUpsertRequest): Observable<CatalogProduct> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<CatalogProduct>(this.apiClient.getUrl('/api/admin/catalog/products'), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  updateProduct(id: string, request: CatalogProductUpsertRequest): Observable<CatalogProduct> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<CatalogProduct>(this.apiClient.getUrl(`/api/admin/catalog/products/${id}`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  setProductStatus(id: string, request: CatalogStatusRequest): Observable<CatalogProduct> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<CatalogProduct>(
          this.apiClient.getUrl(`/api/admin/catalog/products/${id}/status`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  updateProductPrice(id: string, request: CatalogPriceRequest): Observable<CatalogProduct> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<CatalogProduct>(
          this.apiClient.getUrl(`/api/admin/catalog/products/${id}/price`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  uploadProductImage(productId: string, file: File): Observable<CatalogProduct> {
    const formData = new FormData();
    formData.append('file', file);

    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<CatalogProduct>(
          this.apiClient.getUrl(`/api/admin/catalog/products/${productId}/image`),
          formData,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  clearProductImage(productId: string): Observable<CatalogProduct> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.delete<CatalogProduct>(
          this.apiClient.getUrl(`/api/admin/catalog/products/${productId}/image`),
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }
}
