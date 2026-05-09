export type CustomerType = 'Doctor' | 'Clinic' | 'Other';

export interface CustomerListParams {
  search?: string;
  type?: CustomerType;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface InternalDoctorListParams {
  isActive?: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CustomerListItem {
  id: string;
  type: CustomerType;
  displayName: string;
  contactName: string | null;
  phone: string | null;
  whatsApp: string | null;
  email: string | null;
  isActive: boolean;
}

export interface CustomerDetail {
  id: string;
  type: CustomerType;
  displayName: string;
  legalName: string | null;
  contactName: string | null;
  phone: string | null;
  whatsApp: string | null;
  email: string | null;
  address: string | null;
  notes: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  internalDoctors: InternalDoctor[];
}

export interface CustomerUpsertRequest {
  type: CustomerType;
  displayName: string;
  legalName?: string | null;
  contactName?: string | null;
  phone?: string | null;
  whatsApp?: string | null;
  email?: string | null;
  address?: string | null;
  notes?: string | null;
}

export interface CustomerStatusRequest {
  isActive: boolean;
}

export interface InternalDoctor {
  id: string;
  customerId: string;
  fullName: string;
  phone: string | null;
  whatsApp: string | null;
  email: string | null;
  notes: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface InternalDoctorUpsertRequest {
  fullName: string;
  phone?: string | null;
  whatsApp?: string | null;
  email?: string | null;
  notes?: string | null;
}
