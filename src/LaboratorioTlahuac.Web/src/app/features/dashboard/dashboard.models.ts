import { PaymentMethod } from '../payments/payment.models';
import { WorkOrderStatus } from '../orders/work-order.models';

export interface DashboardSummary {
  generatedAtUtc: string;
  customerSummary: CustomerSummary | null;
  operationalSummary: OperationalSummary | null;
  financialSummary: FinancialSummary | null;
}

export interface CustomerSummary {
  activeCustomersCount: number;
  activeDoctorsCount: number;
  activeClinicsCount: number;
  inactiveCustomersCount: number;
}

export interface OperationalSummary {
  activeWorkOrdersCount: number;
  deliveredWorkOrdersCount: number;
  cancelledWorkOrdersCount: number;
  dueTodayCount: number;
  overdueCount: number;
  upcomingDueCount: number;
  byStatus: WorkOrderStatusSummary[];
  latestWorkOrders: DashboardWorkOrder[];
  dueSoonWorkOrders: DashboardWorkOrder[];
}

export interface WorkOrderStatusSummary {
  status: WorkOrderStatus;
  label: string;
  count: number;
}

export interface DashboardWorkOrder {
  id: string;
  orderNumber: string;
  customerDisplayName: string;
  patientName: string;
  status: WorkOrderStatus;
  statusLabel: string;
  deliveryDate: string | null;
}

export interface FinancialSummary {
  totalReceivable: number;
  ordersWithPendingBalanceCount: number;
  paidOrdersCount: number;
  partialPaymentOrdersCount: number;
  unpaidOrdersCount: number;
  overpaidOrdersCount: number;
  cancelledPaymentsCount: number;
  latestPayments: DashboardPayment[];
}

export interface DashboardPayment {
  id: string;
  workOrderId: string;
  orderNumber: string;
  customerDisplayName: string;
  patientName: string;
  paymentDate: string;
  amount: number;
  method: PaymentMethod;
  methodLabel: string;
}
