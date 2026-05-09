namespace LaboratorioTlahuac.Application.Customers;

public interface ICustomerService
{
    Task<CustomerServiceResult<PagedResponse<CustomerListItemResponse>>> ListAsync(
        CustomerListQuery query,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<CustomerDetailResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<CustomerDetailResponse>> CreateAsync(
        CustomerUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<CustomerDetailResponse>> UpdateAsync(
        Guid id,
        CustomerUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<CustomerDetailResponse>> UpdateStatusAsync(
        Guid id,
        CustomerStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<IReadOnlyCollection<InternalDoctorResponse>>> ListInternalDoctorsAsync(
        Guid customerId,
        InternalDoctorListQuery query,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<InternalDoctorResponse>> CreateInternalDoctorAsync(
        Guid customerId,
        InternalDoctorUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<InternalDoctorResponse>> UpdateInternalDoctorAsync(
        Guid customerId,
        Guid doctorId,
        InternalDoctorUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerServiceResult<InternalDoctorResponse>> UpdateInternalDoctorStatusAsync(
        Guid customerId,
        Guid doctorId,
        InternalDoctorStatusRequest request,
        CancellationToken cancellationToken = default);
}
