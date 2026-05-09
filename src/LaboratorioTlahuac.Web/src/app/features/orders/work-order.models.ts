import { CustomerType } from '../customers/customer.models';

export type WorkOrderStatus =
  | 'Received'
  | 'InProcess'
  | 'FirstTrial'
  | 'SecondTrial'
  | 'ReadyForDelivery'
  | 'Delivered'
  | 'Cancelled';

export interface WorkOrderListParams {
  search?: string;
  customerId?: string;
  internalDoctorId?: string;
  status?: WorkOrderStatus;
  receivedDateFrom?: string;
  receivedDateTo?: string;
  deliveryDateFrom?: string;
  deliveryDateTo?: string;
  includeCancelled?: boolean;
  page?: number;
  pageSize?: number;
}

export interface WorkOrderPagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface WorkOrderListItem {
  id: string;
  orderNumber: string;
  customerId: string;
  customerDisplayName: string;
  internalDoctorId: string | null;
  internalDoctorFullName: string | null;
  patientName: string;
  workDescription: string;
  dentalColor: string | null;
  receivedDate: string;
  deliveryDate: string | null;
  status: WorkOrderStatus;
  statusLabel: string;
  totalAmount: number | null;
  isCancelled: boolean;
}

export interface WorkOrderDetail {
  id: string;
  orderNumber: string;
  customerId: string;
  customerDisplayName: string;
  customerType: CustomerType;
  internalDoctorId: string | null;
  internalDoctorFullName: string | null;
  patientName: string;
  receivedDate: string;
  referenceNumber: string | null;
  workDescription: string;
  dentalColor: string | null;
  firstTrialDate: string | null;
  secondTrialDate: string | null;
  deliveryDate: string | null;
  status: WorkOrderStatus;
  statusLabel: string;
  totalAmount: number | null;
  notes: string | null;
  isCancelled: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  statusHistory: WorkOrderStatusHistory[];
}

export interface WorkOrderStatusHistory {
  id: string;
  fromStatus: WorkOrderStatus | null;
  fromStatusLabel: string | null;
  toStatus: WorkOrderStatus;
  toStatusLabel: string;
  notes: string | null;
  changedAtUtc: string;
}

export interface WorkOrderUpsertRequest {
  customerId: string;
  internalDoctorId?: string | null;
  patientName: string;
  receivedDate: string;
  referenceNumber?: string | null;
  workDescription: string;
  dentalColor?: string | null;
  firstTrialDate?: string | null;
  secondTrialDate?: string | null;
  deliveryDate?: string | null;
  totalAmount?: number | null;
  notes?: string | null;
}

export interface WorkOrderChangeStatusRequest {
  status: WorkOrderStatus;
  notes?: string | null;
}

export interface WorkOrderStatusOption {
  value: WorkOrderStatus;
  label: string;
}
