namespace LaboratorioTlahuac.Application.WorkOrders;

public interface IWorkOrderService
{
    Task<WorkOrderServiceResult<WorkOrderPagedResponse<WorkOrderListItemResponse>>> ListAsync(
        WorkOrderListQuery query,
        CancellationToken cancellationToken = default);

    Task<WorkOrderServiceResult<WorkOrderDetailResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<WorkOrderServiceResult<WorkOrderDetailResponse>> CreateAsync(
        WorkOrderUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkOrderServiceResult<WorkOrderDetailResponse>> UpdateAsync(
        Guid id,
        WorkOrderUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkOrderServiceResult<WorkOrderDetailResponse>> ChangeStatusAsync(
        Guid id,
        WorkOrderChangeStatusRequest request,
        CancellationToken cancellationToken = default);

    IReadOnlyCollection<WorkOrderStatusResponse> GetStatuses();
}
