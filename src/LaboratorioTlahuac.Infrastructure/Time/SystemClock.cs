using LaboratorioTlahuac.Application.Abstractions.Time;

namespace LaboratorioTlahuac.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
