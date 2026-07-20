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

    /// <summary>
    /// Returns whether the supplied database value is a supported recommendation decision status.
    /// </summary>
    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && All.Contains(status);
    }

    /// <summary>
    /// Converts a recommendation decision status enum value to the database label.
    /// </summary>
    public static string ToDatabaseValue(ClaimDecisionStatus status)
    {
        return status.ToString();
    }

    /// <summary>
    /// Parses a database label into the matching recommendation decision status enum value.
    /// </summary>
    public static bool TryParseDatabaseValue(string? value, out ClaimDecisionStatus status)
    {
        return Enum.TryParse(value, ignoreCase: false, out status) && IsValid(value);
    }
}
