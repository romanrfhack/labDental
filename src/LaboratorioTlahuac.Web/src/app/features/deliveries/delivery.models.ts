export type DeliveryStatus =
  | 'PendingAssignment'
  | 'Assigned'
  | 'OutForDelivery'
  | 'Delivered'
  | 'FailedDelivery';

export interface DeliveryListParams {
  status?: DeliveryStatus;
  assignedToMe?: boolean;
  page?: number;
  pageSize?: number;
}

export interface DeliveryPagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface DeliveryResponse {
  id: string;
  workOrderId: string;
  orderNumber: string;
  customerId: string;
  customerDisplayName: string;
  customerAddress: string | null;
  customerContactName: string | null;
  customerPhone: string | null;
  customerWhatsApp: string | null;
  internalDoctorId: string | null;
  internalDoctorFullName: string | null;
  patientName: string;
  referenceNumber: string | null;
  workSummary: string;
  deliveryDate: string | null;
  workOrderStatus: string;
  workOrderStatusLabel: string;
  status: DeliveryStatus;
  statusLabel: string;
  assignedToUserId: string | null;
  assignedToUserFullName: string | null;
  recipientName: string | null;
  deliveryNotes: string | null;
  failedReason: string | null;
  assignedAtUtc: string | null;
  outForDeliveryAtUtc: string | null;
  deliveredAtUtc: string | null;
  failedAtUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface DeliveryCreateRequest {
  deliveryNotes?: string | null;
}

export interface DeliveryAssignRequest {
  assignedToUserId: string | null;
  deliveryNotes?: string | null;
}

export interface DeliveryOutForDeliveryRequest {
  deliveryNotes?: string | null;
}

export interface DeliveryCompleteRequest {
  recipientName: string;
  deliveryNotes?: string | null;
}

export interface DeliveryFailedRequest {
  failedReason: string;
  deliveryNotes?: string | null;
}

export interface DeliveryRetryRequest {
  deliveryNotes?: string | null;
}
