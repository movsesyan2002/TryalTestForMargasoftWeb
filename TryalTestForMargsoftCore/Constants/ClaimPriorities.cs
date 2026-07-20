namespace TryalTestForMargsoftCore.Constants;

using TryalTestForMargsoftCore.Enums;

public static class ClaimPriorities
{
    public const string Low = "Low";
    public const string Normal = "Normal";
    public const string High = "High";
    public const string Urgent = "Urgent";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Low,
        Normal,
        High,
        Urgent
    };

    /// <summary>
    /// Returns whether the supplied database value is a supported claim priority.
    /// </summary>
    public static bool IsValid(string? priority)
    {
        return !string.IsNullOrWhiteSpace(priority) && All.Contains(priority);
    }

    /// <summary>
    /// Converts a claim priority enum value to the database label.
    /// </summary>
    public static string ToDatabaseValue(ClaimPriority priority)
    {
        return priority.ToString();
    }

    /// <summary>
    /// Parses a database label into the matching claim priority enum value.
    /// </summary>
    public static bool TryParseDatabaseValue(string? value, out ClaimPriority priority)
    {
        return Enum.TryParse(value, ignoreCase: false, out priority) && IsValid(value);
    }
}
