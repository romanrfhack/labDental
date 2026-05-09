namespace LaboratorioTlahuac.Application.Customers;

public sealed record CustomerListQuery(
    string? Search,
    string? Type,
    bool? IsActive,
    int? Page,
    int? PageSize);

public sealed record CustomerUpsertRequest(
    string? Type,
    string? DisplayName,
    string? LegalName,
    string? ContactName,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Address,
    string? Notes);

public sealed record CustomerStatusRequest(bool IsActive);

public sealed record InternalDoctorListQuery(bool? IsActive);

public sealed record InternalDoctorUpsertRequest(
    string? FullName,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Notes);

public sealed record InternalDoctorStatusRequest(bool IsActive);

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CustomerListItemResponse(
    Guid Id,
    string Type,
    string DisplayName,
    string? ContactName,
    string? Phone,
    string? WhatsApp,
    string? Email,
    bool IsActive);

public sealed record CustomerDetailResponse(
    Guid Id,
    string Type,
    string DisplayName,
    string? LegalName,
    string? ContactName,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<InternalDoctorResponse> InternalDoctors);

public sealed record InternalDoctorResponse(
    Guid Id,
    Guid CustomerId,
    string FullName,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
