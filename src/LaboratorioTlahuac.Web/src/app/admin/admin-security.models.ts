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

export interface AdminUserDetail extends AdminUserListItem {}

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

export interface AdminRolePermissionsRequest {
  permissionIds: string[];
}

export interface AdminRoleListItem {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  userCount: number;
  permissionCount: number;
  permissions: AdminPermission[];
}

export interface AdminRoleDetail extends AdminRoleListItem {
  activeUserCount: number;
}
