using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Customers;
using LaboratorioTlahuac.Domain.Customers;
using LaboratorioTlahuac.Domain.Customers.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Customers;

public sealed class CustomerService(
    LaboratorioTlahuacDbContext dbContext,
    IClock clock,
    ICurrentUser currentUser)
    : ICustomerService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<CustomerServiceResult<PagedResponse<CustomerListItemResponse>>> ListAsync(
        CustomerListQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var page = query.Page ?? DefaultPage;
        var pageSize = query.PageSize ?? DefaultPageSize;
        var type = default(CustomerType?);

        if (page < 1)
        {
            AddError(errors, nameof(query.Page), "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            AddError(errors, nameof(query.PageSize), "PageSize must be between 1 and 100.");
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            if (!TryParseCustomerType(query.Type, out var parsedType))
            {
                AddError(errors, nameof(query.Type), "Type must be Doctor, Clinic, or Other.");
            }
            else
            {
                type = parsedType;
            }
        }

        if (errors.Count > 0)
        {
            return CustomerServiceResult.Validation<PagedResponse<CustomerListItemResponse>>(errors);
        }

        var customersQuery = dbContext.Customers.AsNoTracking();

        if (query.IsActive.HasValue)
        {
            customersQuery = customersQuery.Where(customer => customer.IsActive == query.IsActive.Value);
        }
        else
        {
            customersQuery = customersQuery.Where(customer => customer.IsActive);
        }

        if (type.HasValue)
        {
            customersQuery = customersQuery.Where(customer => customer.Type == type.Value);
        }

        var search = NormalizeOptional(query.Search);

        if (search is not null)
        {
            var pattern = $"%{search}%";
            customersQuery = customersQuery.Where(customer =>
                EF.Functions.Like(customer.DisplayName, pattern)
                || (customer.LegalName != null && EF.Functions.Like(customer.LegalName, pattern))
                || (customer.ContactName != null && EF.Functions.Like(customer.ContactName, pattern))
                || (customer.Phone != null && EF.Functions.Like(customer.Phone, pattern))
                || (customer.WhatsApp != null && EF.Functions.Like(customer.WhatsApp, pattern))
                || (customer.Email != null && EF.Functions.Like(customer.Email, pattern)));
        }

        var totalCount = await customersQuery.CountAsync(cancellationToken);
        var items = await customersQuery
            .OrderBy(customer => customer.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(customer => new CustomerListItemResponse(
                customer.Id,
                customer.Type.ToString(),
                customer.DisplayName,
                customer.ContactName,
                customer.Phone,
                customer.WhatsApp,
                customer.Email,
                customer.IsActive))
            .ToListAsync(cancellationToken);

        return CustomerServiceResult.Success(
            new PagedResponse<CustomerListItemResponse>(items, page, pageSize, totalCount));
    }

    public async Task<CustomerServiceResult<CustomerDetailResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .Include(currentCustomer => currentCustomer.InternalDoctors)
            .AsNoTracking()
            .FirstOrDefaultAsync(currentCustomer => currentCustomer.Id == id, cancellationToken);

        return customer is null
            ? CustomerServiceResult.NotFound<CustomerDetailResponse>("Customer was not found.")
            : CustomerServiceResult.Success(MapCustomerDetail(customer));
    }

    public async Task<CustomerServiceResult<CustomerDetailResponse>> CreateAsync(
        CustomerUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateCustomer(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CustomerServiceResult.Validation<CustomerDetailResponse>(input.Errors);
        }

        var now = clock.UtcNow;
        var customer = Customer.Create(
            input.Value.Type,
            input.Value.DisplayName,
            input.Value.LegalName,
            input.Value.ContactName,
            input.Value.Phone,
            input.Value.WhatsApp,
            input.Value.Email,
            input.Value.Address,
            input.Value.Notes,
            currentUser.UserId,
            now);

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerServiceResult.Success(MapCustomerDetail(customer));
    }

    public async Task<CustomerServiceResult<CustomerDetailResponse>> UpdateAsync(
        Guid id,
        CustomerUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateCustomer(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CustomerServiceResult.Validation<CustomerDetailResponse>(input.Errors);
        }

        var customer = await dbContext.Customers
            .Include(currentCustomer => currentCustomer.InternalDoctors)
            .FirstOrDefaultAsync(currentCustomer => currentCustomer.Id == id, cancellationToken);

        if (customer is null)
        {
            return CustomerServiceResult.NotFound<CustomerDetailResponse>("Customer was not found.");
        }

        var changesFromClinicToNonClinic = customer.Type == CustomerType.Clinic
            && input.Value.Type != CustomerType.Clinic;
        var hasActiveInternalDoctors = customer.InternalDoctors.Any(internalDoctor => internalDoctor.IsActive);

        if (changesFromClinicToNonClinic && hasActiveInternalDoctors)
        {
            return CustomerServiceResult.Conflict<CustomerDetailResponse>(
                "Clinic customers with active internal doctors cannot be changed to Doctor or Other.");
        }

        customer.Update(
            input.Value.Type,
            input.Value.DisplayName,
            input.Value.LegalName,
            input.Value.ContactName,
            input.Value.Phone,
            input.Value.WhatsApp,
            input.Value.Email,
            input.Value.Address,
            input.Value.Notes,
            currentUser.UserId,
            clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerServiceResult.Success(MapCustomerDetail(customer));
    }

    public async Task<CustomerServiceResult<CustomerDetailResponse>> UpdateStatusAsync(
        Guid id,
        CustomerStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .Include(currentCustomer => currentCustomer.InternalDoctors)
            .FirstOrDefaultAsync(currentCustomer => currentCustomer.Id == id, cancellationToken);

        if (customer is null)
        {
            return CustomerServiceResult.NotFound<CustomerDetailResponse>("Customer was not found.");
        }

        customer.SetStatus(request.IsActive, currentUser.UserId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerServiceResult.Success(MapCustomerDetail(customer));
    }

    public async Task<CustomerServiceResult<IReadOnlyCollection<InternalDoctorResponse>>> ListInternalDoctorsAsync(
        Guid customerId,
        InternalDoctorListQuery query,
        CancellationToken cancellationToken = default)
    {
        var clinicCheck = await EnsureClinicCustomerAsync(customerId, cancellationToken);

        if (clinicCheck.Status != CustomerServiceStatus.Success)
        {
            return MapClinicCheckFailure<IReadOnlyCollection<InternalDoctorResponse>>(clinicCheck);
        }

        var doctorsQuery = dbContext.InternalDoctors
            .AsNoTracking()
            .Where(internalDoctor => internalDoctor.CustomerId == customerId);

        if (query.IsActive.HasValue)
        {
            doctorsQuery = doctorsQuery.Where(internalDoctor => internalDoctor.IsActive == query.IsActive.Value);
        }
        else
        {
            doctorsQuery = doctorsQuery.Where(internalDoctor => internalDoctor.IsActive);
        }

        var doctors = await doctorsQuery
            .OrderByDescending(internalDoctor => internalDoctor.IsActive)
            .ThenBy(internalDoctor => internalDoctor.FullName)
            .Select(internalDoctor => new InternalDoctorResponse(
                internalDoctor.Id,
                internalDoctor.CustomerId,
                internalDoctor.FullName,
                internalDoctor.Phone,
                internalDoctor.WhatsApp,
                internalDoctor.Email,
                internalDoctor.Notes,
                internalDoctor.IsActive,
                internalDoctor.CreatedAtUtc,
                internalDoctor.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return CustomerServiceResult.Success<IReadOnlyCollection<InternalDoctorResponse>>(doctors);
    }

    public async Task<CustomerServiceResult<InternalDoctorResponse>> CreateInternalDoctorAsync(
        Guid customerId,
        InternalDoctorUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var clinicCheck = await EnsureClinicCustomerAsync(customerId, cancellationToken);

        if (clinicCheck.Status != CustomerServiceStatus.Success)
        {
            return MapClinicCheckFailure<InternalDoctorResponse>(clinicCheck);
        }

        var input = ValidateInternalDoctor(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CustomerServiceResult.Validation<InternalDoctorResponse>(input.Errors);
        }

        var doctor = InternalDoctor.Create(
            customerId,
            input.Value.FullName,
            input.Value.Phone,
            input.Value.WhatsApp,
            input.Value.Email,
            input.Value.Notes,
            currentUser.UserId,
            clock.UtcNow);

        dbContext.InternalDoctors.Add(doctor);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerServiceResult.Success(MapInternalDoctor(doctor));
    }

    public async Task<CustomerServiceResult<InternalDoctorResponse>> UpdateInternalDoctorAsync(
        Guid customerId,
        Guid doctorId,
        InternalDoctorUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var clinicCheck = await EnsureClinicCustomerAsync(customerId, cancellationToken);

        if (clinicCheck.Status != CustomerServiceStatus.Success)
        {
            return MapClinicCheckFailure<InternalDoctorResponse>(clinicCheck);
        }

        var input = ValidateInternalDoctor(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CustomerServiceResult.Validation<InternalDoctorResponse>(input.Errors);
        }

        var doctor = await dbContext.InternalDoctors
            .FirstOrDefaultAsync(
                internalDoctor => internalDoctor.Id == doctorId && internalDoctor.CustomerId == customerId,
                cancellationToken);

        if (doctor is null)
        {
            return CustomerServiceResult.NotFound<InternalDoctorResponse>("Internal doctor was not found.");
        }

        doctor.Update(
            input.Value.FullName,
            input.Value.Phone,
            input.Value.WhatsApp,
            input.Value.Email,
            input.Value.Notes,
            currentUser.UserId,
            clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerServiceResult.Success(MapInternalDoctor(doctor));
    }

    public async Task<CustomerServiceResult<InternalDoctorResponse>> UpdateInternalDoctorStatusAsync(
        Guid customerId,
        Guid doctorId,
        InternalDoctorStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var clinicCheck = await EnsureClinicCustomerAsync(customerId, cancellationToken);

        if (clinicCheck.Status != CustomerServiceStatus.Success)
        {
            return MapClinicCheckFailure<InternalDoctorResponse>(clinicCheck);
        }

        var doctor = await dbContext.InternalDoctors
            .FirstOrDefaultAsync(
                internalDoctor => internalDoctor.Id == doctorId && internalDoctor.CustomerId == customerId,
                cancellationToken);

        if (doctor is null)
        {
            return CustomerServiceResult.NotFound<InternalDoctorResponse>("Internal doctor was not found.");
        }

        doctor.SetStatus(request.IsActive, currentUser.UserId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerServiceResult.Success(MapInternalDoctor(doctor));
    }

    private async Task<CustomerServiceResult<CustomerType>> EnsureClinicCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customerType = await dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == customerId)
            .Select(customer => (CustomerType?)customer.Type)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerType is null)
        {
            return CustomerServiceResult.NotFound<CustomerType>("Customer was not found.");
        }

        if (customerType != CustomerType.Clinic)
        {
            return CustomerServiceResult.Validation<CustomerType>(new Dictionary<string, string[]>
            {
                ["customerId"] = ["Internal doctors can only be managed for Clinic customers."]
            });
        }

        return CustomerServiceResult.Success(customerType.Value);
    }

    private static CustomerServiceResult<T> MapClinicCheckFailure<T>(
        CustomerServiceResult<CustomerType> clinicCheck)
    {
        return clinicCheck.Status switch
        {
            CustomerServiceStatus.NotFound => CustomerServiceResult.NotFound<T>(clinicCheck.Message ?? "Customer was not found."),
            CustomerServiceStatus.ValidationError => CustomerServiceResult.Validation<T>(clinicCheck.Errors),
            CustomerServiceStatus.Conflict => CustomerServiceResult.Conflict<T>(clinicCheck.Message ?? "Customer rule conflict."),
            _ => throw new InvalidOperationException("Unexpected customer service status.")
        };
    }

    private static ValidationResult<CustomerInput> ValidateCustomer(CustomerUpsertRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var displayName = NormalizeOptional(request.DisplayName);
        var type = default(CustomerType);

        if (displayName is null)
        {
            AddError(errors, nameof(request.DisplayName), "DisplayName is required.");
        }
        else
        {
            ValidateMaxLength(errors, nameof(request.DisplayName), displayName, Customer.DisplayNameMaxLength);
        }

        if (!TryParseCustomerType(request.Type, out type))
        {
            AddError(errors, nameof(request.Type), "Type must be Doctor, Clinic, or Other.");
        }

        var legalName = NormalizeAndValidateMax(errors, nameof(request.LegalName), request.LegalName, Customer.LegalNameMaxLength);
        var contactName = NormalizeAndValidateMax(errors, nameof(request.ContactName), request.ContactName, Customer.ContactNameMaxLength);
        var phone = NormalizeAndValidateMax(errors, nameof(request.Phone), request.Phone, Customer.PhoneMaxLength);
        var whatsApp = NormalizeAndValidateMax(errors, nameof(request.WhatsApp), request.WhatsApp, Customer.WhatsAppMaxLength);
        var email = NormalizeAndValidateEmail(errors, nameof(request.Email), request.Email);
        var address = NormalizeAndValidateMax(errors, nameof(request.Address), request.Address, Customer.AddressMaxLength);
        var notes = NormalizeAndValidateMax(errors, nameof(request.Notes), request.Notes, Customer.NotesMaxLength);

        return errors.Count > 0 || displayName is null
            ? new ValidationResult<CustomerInput>(errors, null)
            : new ValidationResult<CustomerInput>(
                errors,
                new CustomerInput(type, displayName, legalName, contactName, phone, whatsApp, email, address, notes));
    }

    private static ValidationResult<InternalDoctorInput> ValidateInternalDoctor(InternalDoctorUpsertRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var fullName = NormalizeOptional(request.FullName);

        if (fullName is null)
        {
            AddError(errors, nameof(request.FullName), "FullName is required.");
        }
        else
        {
            ValidateMaxLength(errors, nameof(request.FullName), fullName, InternalDoctor.FullNameMaxLength);
        }

        var phone = NormalizeAndValidateMax(errors, nameof(request.Phone), request.Phone, Customer.PhoneMaxLength);
        var whatsApp = NormalizeAndValidateMax(errors, nameof(request.WhatsApp), request.WhatsApp, Customer.WhatsAppMaxLength);
        var email = NormalizeAndValidateEmail(errors, nameof(request.Email), request.Email);
        var notes = NormalizeAndValidateMax(errors, nameof(request.Notes), request.Notes, Customer.NotesMaxLength);

        return errors.Count > 0 || fullName is null
            ? new ValidationResult<InternalDoctorInput>(errors, null)
            : new ValidationResult<InternalDoctorInput>(
                errors,
                new InternalDoctorInput(fullName, phone, whatsApp, email, notes));
    }

    private static bool TryParseCustomerType(string? value, out CustomerType type)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value.Trim(), ignoreCase: true, out type)
            || !Enum.IsDefined(type))
        {
            type = default;
            return false;
        }

        return true;
    }

    private static string? NormalizeAndValidateMax(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value,
        int maxLength)
    {
        var normalized = NormalizeOptional(value);

        if (normalized is not null)
        {
            ValidateMaxLength(errors, fieldName, normalized, maxLength);
        }

        return normalized;
    }

    private static string? NormalizeAndValidateEmail(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value)
    {
        var email = NormalizeAndValidateMax(errors, fieldName, value, Customer.EmailMaxLength);

        if (email is null)
        {
            return null;
        }

        if (!email.Contains('@', StringComparison.Ordinal) || !IsValidEmail(email))
        {
            AddError(errors, fieldName, "Email must have a valid format.");
        }

        return email;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);

            return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static void ValidateMaxLength(
        IDictionary<string, string[]> errors,
        string fieldName,
        string value,
        int maxLength)
    {
        if (value.Length > maxLength)
        {
            AddError(errors, fieldName, $"{fieldName} must be {maxLength} characters or fewer.");
        }
    }

    private static void AddError(IDictionary<string, string[]> errors, string fieldName, string error)
    {
        errors[fieldName] = errors.TryGetValue(fieldName, out var existingErrors)
            ? [.. existingErrors, error]
            : [error];
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static CustomerDetailResponse MapCustomerDetail(Customer customer)
    {
        var internalDoctors = customer.Type == CustomerType.Clinic
            ? customer.InternalDoctors
                .OrderByDescending(internalDoctor => internalDoctor.IsActive)
                .ThenBy(internalDoctor => internalDoctor.FullName)
                .Select(MapInternalDoctor)
                .ToArray()
            : [];

        return new CustomerDetailResponse(
            customer.Id,
            customer.Type.ToString(),
            customer.DisplayName,
            customer.LegalName,
            customer.ContactName,
            customer.Phone,
            customer.WhatsApp,
            customer.Email,
            customer.Address,
            customer.Notes,
            customer.IsActive,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc,
            internalDoctors);
    }

    private static InternalDoctorResponse MapInternalDoctor(InternalDoctor internalDoctor)
    {
        return new InternalDoctorResponse(
            internalDoctor.Id,
            internalDoctor.CustomerId,
            internalDoctor.FullName,
            internalDoctor.Phone,
            internalDoctor.WhatsApp,
            internalDoctor.Email,
            internalDoctor.Notes,
            internalDoctor.IsActive,
            internalDoctor.CreatedAtUtc,
            internalDoctor.UpdatedAtUtc);
    }

    private sealed record CustomerInput(
        CustomerType Type,
        string DisplayName,
        string? LegalName,
        string? ContactName,
        string? Phone,
        string? WhatsApp,
        string? Email,
        string? Address,
        string? Notes);

    private sealed record InternalDoctorInput(
        string FullName,
        string? Phone,
        string? WhatsApp,
        string? Email,
        string? Notes);

    private sealed record ValidationResult<T>(
        IReadOnlyDictionary<string, string[]> Errors,
        T? Value);
}
