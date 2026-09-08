export interface AdminPagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface AdminRoleSummary {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
}

export interface AdminPermission {
  id: string;
  key: string;
  description: string;
}

export type AdminPermissionOverrideEffect = 'Allow' | 'Deny';

export interface AdminUserPermissionState extends AdminPermission {
  inherited: boolean;
  effective: boolean;
  overrideEffect: AdminPermissionOverrideEffect | null;
  sourceRoles: string[];
}

export interface AdminUserListParams {
  search?: string;
  isActive?: boolean;
  roleId?: string;
  page?: number;
  pageSize?: number;
}

export interface AdminUserListItem {
  id: string;
  email: string;
  fullName: string;
  isActive: boolean;
  roles: AdminRoleSummary[];
  lastLoginAtUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface AdminUserDetail extends AdminUserListItem {
  isPermissionOverrideEditingLocked: boolean;
  permissions: AdminUserPermissionState[];
}

export interface AdminUserCreateRequest {
  email: string;
  fullName: string;
  temporaryPassword: string;
  roleIds: string[];
}

export interface AdminUserUpdateRequest {
  email: string;
  fullName: string;
}

export interface AdminUserStatusRequest {
  isActive: boolean;
}

export interface AdminUserRolesRequest {
  roleIds: string[];
}

export interface AdminUserTemporaryPasswordRequest {
  temporaryPassword: string;
}

export interface AdminUserPermissionOverrideRequest {
  permissionId: string;
  effect: AdminPermissionOverrideEffect;
}

export interface AdminUserPermissionsRequest {
  overrides: AdminUserPermissionOverrideRequest[];
}

export interface AdminRolePermissionsRequest {
  permissionIds: string[];
}

export interface AdminRoleListItem {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  isPermissionEditingLocked: boolean;
  userCount: number;
  permissionCount: number;
  permissions: AdminPermission[];
}

export interface AdminRoleDetail extends AdminRoleListItem {
  activeUserCount: number;
  availablePermissions: AdminPermission[];
}
