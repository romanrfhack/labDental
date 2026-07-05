namespace LaboratorioTlahuac.Application.Deliveries;

public interface IDeliveryService
{
    Task<DeliveryServiceResult<DeliveryPagedResponse<DeliveryResponse>>> ListAsync(
        DeliveryListQuery query,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> GetByWorkOrderIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> CreateForWorkOrderAsync(
        Guid workOrderId,
        DeliveryCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> AssignAsync(
        Guid id,
        DeliveryAssignRequest request,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> MarkOutForDeliveryAsync(
        Guid id,
        DeliveryOutForDeliveryRequest request,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> CompleteAsync(
        Guid id,
        DeliveryCompleteRequest request,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> MarkFailedAsync(
        Guid id,
        DeliveryFailedRequest request,
        CancellationToken cancellationToken = default);

    Task<DeliveryServiceResult<DeliveryResponse>> RetryAsync(
        Guid id,
        DeliveryRetryRequest request,
        CancellationToken cancellationToken = default);
}
