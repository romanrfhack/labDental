namespace LaboratorioTlahuac.Infrastructure.Dashboard;

internal static class DashboardTimeZoneResolver
{
    private static readonly Dictionary<string, string[]> CompatibleTimeZoneIds =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["America/Mexico_City"] = ["Central Standard Time (Mexico)"],
            ["Central Standard Time (Mexico)"] = ["America/Mexico_City"]
        };

    public static TimeZoneInfo Resolve(string? timeZoneId)
    {
        var configuredId = string.IsNullOrWhiteSpace(timeZoneId)
            ? DashboardOptions.DefaultBusinessTimeZone
            : timeZoneId.Trim();

        foreach (var candidateId in GetCandidateIds(configuredId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidateId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException(
            $"Dashboard business time zone '{configuredId}' is not available on this operating system.");
    }

    private static IEnumerable<string> GetCandidateIds(string configuredId)
    {
        yield return configuredId;

        if (!CompatibleTimeZoneIds.TryGetValue(configuredId, out var candidateIds))
        {
            yield break;
        }

        foreach (var candidateId in candidateIds)
        {
            yield return candidateId;
        }
    }
}
