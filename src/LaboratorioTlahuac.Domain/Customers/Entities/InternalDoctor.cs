using System.Net.Mail;

namespace LaboratorioTlahuac.Domain.Customers.Entities;

public sealed class InternalDoctor
{
    public const int FullNameMaxLength = 150;

    private InternalDoctor()
    {
    }

    private InternalDoctor(
        Guid id,
        Guid customerId,
        string fullName,
        string? phone,
        string? whatsApp,
        string? email,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
        UpdatedAtUtc = createdAtUtc;
        UpdatedByUserId = createdByUserId;

        SetDetails(fullName, phone, whatsApp, email, notes);
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string? WhatsApp { get; private set; }

    public string? Email { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public Customer? Customer { get; private set; }

    public static InternalDoctor Create(
        Guid customerId,
        string fullName,
        string? phone,
        string? whatsApp,
        string? email,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        return new InternalDoctor(
            Guid.NewGuid(),
            customerId,
            fullName,
            phone,
            whatsApp,
            email,
            notes,
            createdByUserId,
            createdAtUtc);
    }

    public void Update(
        string fullName,
        string? phone,
        string? whatsApp,
        string? email,
        string? notes,
        Guid? updatedByUserId,
        DateTimeOffset updatedAtUtc)
    {
        SetDetails(fullName, phone, whatsApp, email, notes);
        Touch(updatedByUserId, updatedAtUtc);
    }

    public void SetStatus(bool isActive, Guid? updatedByUserId, DateTimeOffset updatedAtUtc)
    {
        IsActive = isActive;
        Touch(updatedByUserId, updatedAtUtc);
    }

    private void SetDetails(
        string fullName,
        string? phone,
        string? whatsApp,
        string? email,
        string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        FullName = fullName.Trim();
        Phone = TrimOptional(phone);
        WhatsApp = TrimOptional(whatsApp);
        Email = NormalizeEmail(email);
        Notes = TrimOptional(notes);
    }

    private void Touch(Guid? updatedByUserId, DateTimeOffset updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
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
