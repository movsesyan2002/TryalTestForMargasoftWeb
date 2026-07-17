namespace TryalTestForMargsoftCore.Constants;

using TryalTestForMargsoftCore.Enums;

public static class ClaimDecisionStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Overridden = "Overridden";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Pending,
        Confirmed,
        Overridden
    };

    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && All.Contains(status);
    }

    public static string ToDatabaseValue(ClaimDecisionStatus status)
    {
        return status.ToString();
    }

    public static bool TryParseDatabaseValue(string? value, out ClaimDecisionStatus status)
    {
        return Enum.TryParse(value, ignoreCase: false, out status) && IsValid(value);
    }
}
