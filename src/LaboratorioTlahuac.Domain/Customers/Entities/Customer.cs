using System.Net.Mail;
using LaboratorioTlahuac.Domain.Customers;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;

namespace LaboratorioTlahuac.Domain.Customers.Entities;

public sealed class Customer
{
    public const int DisplayNameMaxLength = 150;
    public const int LegalNameMaxLength = 200;
    public const int ContactNameMaxLength = 150;
    public const int PhoneMaxLength = 30;
    public const int WhatsAppMaxLength = 30;
    public const int EmailMaxLength = 200;
    public const int AddressMaxLength = 500;
    public const int NotesMaxLength = 1000;

    private Customer()
    {
    }

    private Customer(
        Guid id,
        CustomerType type,
        string displayName,
        string? legalName,
        string? contactName,
        string? phone,
        string? whatsApp,
        string? email,
        string? address,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
        UpdatedAtUtc = createdAtUtc;
        UpdatedByUserId = createdByUserId;

        SetDetails(type, displayName, legalName, contactName, phone, whatsApp, email, address, notes);
    }

    public Guid Id { get; private set; }

    public CustomerType Type { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string? LegalName { get; private set; }

    public string? ContactName { get; private set; }

    public string? Phone { get; private set; }

    public string? WhatsApp { get; private set; }

    public string? Email { get; private set; }

    public string? Address { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public ICollection<InternalDoctor> InternalDoctors { get; private set; } = new List<InternalDoctor>();

    public ICollection<WorkOrder> WorkOrders { get; private set; } = new List<WorkOrder>();

    public static Customer Create(
        CustomerType type,
        string displayName,
        string? legalName,
        string? contactName,
        string? phone,
        string? whatsApp,
        string? email,
        string? address,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        return new Customer(
            Guid.NewGuid(),
            type,
            displayName,
            legalName,
            contactName,
            phone,
            whatsApp,
            email,
            address,
            notes,
            createdByUserId,
            createdAtUtc);
    }

    public void Update(
        CustomerType type,
        string displayName,
        string? legalName,
        string? contactName,
        string? phone,
        string? whatsApp,
        string? email,
        string? address,
        string? notes,
        Guid? updatedByUserId,
        DateTimeOffset updatedAtUtc)
    {
        SetDetails(type, displayName, legalName, contactName, phone, whatsApp, email, address, notes);
        Touch(updatedByUserId, updatedAtUtc);
    }

    public void SetStatus(bool isActive, Guid? updatedByUserId, DateTimeOffset updatedAtUtc)
    {
        IsActive = isActive;
        Touch(updatedByUserId, updatedAtUtc);
    }

    private void SetDetails(
        CustomerType type,
        string displayName,
        string? legalName,
        string? contactName,
        string? phone,
        string? whatsApp,
        string? email,
        string? address,
        string? notes)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "Customer type is invalid.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Type = type;
        DisplayName = TrimRequired(displayName);
        LegalName = TrimOptional(legalName);
        ContactName = TrimOptional(contactName);
        Phone = TrimOptional(phone);
        WhatsApp = TrimOptional(whatsApp);
        Email = NormalizeEmail(email);
        Address = TrimOptional(address);
        Notes = TrimOptional(notes);
    }

    private void Touch(Guid? updatedByUserId, DateTimeOffset updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string TrimRequired(string value)
    {
        return value.Trim();
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeEmail(string? email)
    {
        var trimmedEmail = TrimOptional(email);

        if (trimmedEmail is null)
        {
            return null;
        }

        _ = new MailAddress(trimmedEmail);

        return trimmedEmail;
    }
}
