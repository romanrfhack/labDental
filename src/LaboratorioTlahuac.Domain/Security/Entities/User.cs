using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Domain.Security.Entities;

public sealed class User
{
    private User()
    {
    }

    private User(
        Guid id,
        string email,
        string fullName,
        string passwordHash,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        SetEmail(email);
        SetFullName(fullName);
        SetPasswordHash(passwordHash);
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public int AccessFailedCount { get; private set; }

    public DateTimeOffset? LockoutEndUtc { get; private set; }

    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    public static User Create(
        string email,
        string fullName,
        string passwordHash,
        DateTimeOffset createdAtUtc)
    {
        return new User(Guid.NewGuid(), email, fullName, passwordHash, createdAtUtc);
    }

    public bool IsLockedOut(DateTimeOffset utcNow)
    {
        return LockoutEndUtc is not null && LockoutEndUtc > utcNow;
    }

    public void RecordSuccessfulLogin(DateTimeOffset utcNow)
    {
        LastLoginAtUtc = utcNow;
        AccessFailedCount = 0;
        LockoutEndUtc = null;
        UpdatedAtUtc = utcNow;
    }

    public void RecordFailedLogin(
        DateTimeOffset utcNow,
        int maxFailedAccessAttempts,
        TimeSpan lockoutDuration)
    {
        AccessFailedCount += 1;

        if (maxFailedAccessAttempts > 0 && AccessFailedCount >= maxFailedAccessAttempts)
        {
            LockoutEndUtc = utcNow.Add(lockoutDuration);
            AccessFailedCount = 0;
        }

        UpdatedAtUtc = utcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
    }

    public void Rename(string fullName, DateTimeOffset utcNow)
    {
        SetFullName(fullName);
        UpdatedAtUtc = utcNow;
    }

    public void Activate(DateTimeOffset utcNow)
    {
        IsActive = true;
        UpdatedAtUtc = utcNow;
    }

    public void Deactivate(DateTimeOffset utcNow)
    {
        IsActive = false;
        UpdatedAtUtc = utcNow;
    }

    public void LockUntil(DateTimeOffset lockoutEndUtc, DateTimeOffset utcNow)
    {
        if (lockoutEndUtc <= utcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(lockoutEndUtc), "Lockout end must be in the future.");
        }

        LockoutEndUtc = lockoutEndUtc;
        UpdatedAtUtc = utcNow;
    }

    public void ClearLockout(DateTimeOffset utcNow)
    {
        AccessFailedCount = 0;
        LockoutEndUtc = null;
        UpdatedAtUtc = utcNow;
    }

    private void SetEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Email = email.Trim();
        NormalizedEmail = SecurityTextNormalizer.NormalizeEmail(Email);
    }

    private void SetFullName(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        FullName = fullName.Trim();
    }
}
