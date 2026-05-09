export type PaymentMethod = 'Cash' | 'BankTransfer' | 'Card' | 'Other';

export type PaymentStatus = 'TotalNotSet' | 'Unpaid' | 'Partial' | 'Paid' | 'Overpaid';

export interface WorkOrderPaymentListParams {
  includeCancelled?: boolean;
}

export interface PaymentListParams {
  search?: string;
  customerId?: string;
  workOrderId?: string;
  method?: PaymentMethod;
  paymentDateFrom?: string;
  paymentDateTo?: string;
  includeCancelled?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PaymentPagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface WorkOrderPayment {
  id: string;
  workOrderId: string;
  paymentDate: string;
  amount: number;
  method: PaymentMethod;
  methodLabel: string;
  reference: string | null;
  notes: string | null;
  isCancelled: boolean;
  createdAtUtc: string;
  cancelledAtUtc: string | null;
  cancellationReason: string | null;
}

export interface PaymentListItem {
  id: string;
  workOrderId: string;
  orderNumber: string;
  customerDisplayName: string;
  patientName: string;
  paymentDate: string;
  amount: number;
  method: PaymentMethod;
  methodLabel: string;
  reference: string | null;
  isCancelled: boolean;
}

export interface PaymentSummary {
  workOrderId: string;
  orderNumber: string;
  totalAmount: number | null;
  paidAmount: number;
  balance: number | null;
  paymentStatus: PaymentStatus;
  paymentStatusLabel: string;
  activePaymentsCount: number;
  cancelledPaymentsCount: number;
}

export interface PaymentCreateRequest {
  paymentDate: string;
  amount: number;
  method: PaymentMethod;
  reference?: string | null;
  notes?: string | null;
}

export interface PaymentMutationResponse {
  payment: WorkOrderPayment;
  summary: PaymentSummary;
}

export interface PaymentOption {
  value: string;
  label: string;
}

export interface PaymentMethodOption {
  value: PaymentMethod;
  label: string;
}
