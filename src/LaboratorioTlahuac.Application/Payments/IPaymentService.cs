namespace LaboratorioTlahuac.Application.Payments;

public interface IPaymentService
{
    Task<PaymentServiceResult<IReadOnlyCollection<PaymentResponse>>> ListForWorkOrderAsync(
        Guid workOrderId,
        WorkOrderPaymentListQuery query,
        CancellationToken cancellationToken = default);

    Task<PaymentServiceResult<PaymentSummaryResponse>> GetSummaryAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default);

    Task<PaymentServiceResult<PaymentMutationResponse>> CreateAsync(
        Guid workOrderId,
        PaymentCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentServiceResult<PaymentMutationResponse>> CancelAsync(
        Guid workOrderId,
        Guid paymentId,
        PaymentCancelRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentServiceResult<PaymentPagedResponse<PaymentListItemResponse>>> ListAsync(
        PaymentListQuery query,
        CancellationToken cancellationToken = default);

    IReadOnlyCollection<PaymentOptionResponse> GetMethods();

    IReadOnlyCollection<PaymentOptionResponse> GetStatuses();
}
