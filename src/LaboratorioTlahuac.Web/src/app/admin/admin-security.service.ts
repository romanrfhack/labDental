import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap } from 'rxjs';

import { AuthService } from '../core/auth/auth.service';
import { ApiClient } from '../core/http/api-client';
import {
  AdminPagedResponse,
  AdminRoleDetail,
  AdminRoleListItem,
  AdminRolePermissionsRequest,
  AdminUserCreateRequest,
  AdminUserDetail,
  AdminUserListItem,
  AdminUserListParams,
  AdminUserPermissionOverrideRequest,
  AdminUserPermissionsRequest,
  AdminUserRolesRequest,
  AdminUserStatusRequest,
  AdminUserTemporaryPasswordRequest,
  AdminUserUpdateRequest
} from './admin-security.models';

@Injectable({ providedIn: 'root' })
export class AdminSecurityService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);
  private readonly authService = inject(AuthService);

  listUsers(params: AdminUserListParams = {}): Observable<AdminPagedResponse<AdminUserListItem>> {
    return this.http.get<AdminPagedResponse<AdminUserListItem>>(this.apiClient.getUrl('/api/admin/users'), {
      params: this.toUserListParams(params),
      withCredentials: true
    });
  }

  getUserById(id: string): Observable<AdminUserDetail> {
    return this.http.get<AdminUserDetail>(this.apiClient.getUrl(`/api/admin/users/${id}`), {
      withCredentials: true
    });
  }

  createUser(request: AdminUserCreateRequest): Observable<AdminUserDetail> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<AdminUserDetail>(this.apiClient.getUrl('/api/admin/users'), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  updateUser(id: string, request: AdminUserUpdateRequest): Observable<AdminUserDetail> {
    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<AdminUserDetail>(this.apiClient.getUrl(`/api/admin/users/${id}`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  updateUserStatus(id: string, isActive: boolean): Observable<AdminUserDetail> {
    const request: AdminUserStatusRequest = { isActive };

    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<AdminUserDetail>(this.apiClient.getUrl(`/api/admin/users/${id}/status`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  assignUserRoles(id: string, roleIds: string[]): Observable<AdminUserDetail> {
    const request: AdminUserRolesRequest = { roleIds };

    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.patch<AdminUserDetail>(this.apiClient.getUrl(`/api/admin/users/${id}/roles`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  updateUserPermissions(
    id: string,
    overrides: AdminUserPermissionOverrideRequest[]
  ): Observable<AdminUserDetail> {
    const request: AdminUserPermissionsRequest = { overrides };

    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<AdminUserDetail>(this.apiClient.getUrl(`/api/admin/users/${id}/permissions`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  setTemporaryPassword(id: string, temporaryPassword: string): Observable<AdminUserDetail> {
    const request: AdminUserTemporaryPasswordRequest = { temporaryPassword };

    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.post<AdminUserDetail>(
          this.apiClient.getUrl(`/api/admin/users/${id}/temporary-password`),
          request,
          {
            headers,
            withCredentials: true
          }
        )
      )
    );
  }

  listRoles(): Observable<AdminRoleListItem[]> {
    return this.http.get<AdminRoleListItem[]>(this.apiClient.getUrl('/api/admin/roles'), {
      withCredentials: true
    });
  }

  getRoleById(id: string): Observable<AdminRoleDetail> {
    return this.http.get<AdminRoleDetail>(this.apiClient.getUrl(`/api/admin/roles/${id}`), {
      withCredentials: true
    });
  }

  updateRolePermissions(id: string, permissionIds: string[]): Observable<AdminRoleDetail> {
    const request: AdminRolePermissionsRequest = { permissionIds };

    return this.authService.getCsrfHeaders().pipe(
      switchMap((headers) =>
        this.http.put<AdminRoleDetail>(this.apiClient.getUrl(`/api/admin/roles/${id}/permissions`), request, {
          headers,
          withCredentials: true
        })
      )
    );
  }

  private toUserListParams(params: AdminUserListParams) {
    let httpParams = new HttpParams();

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', String(params.isActive));
    }

    if (params.roleId) {
      httpParams = httpParams.set('roleId', params.roleId);
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
