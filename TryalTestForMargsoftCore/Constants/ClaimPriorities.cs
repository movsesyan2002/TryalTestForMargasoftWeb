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

    public static bool IsValid(string? priority)
    {
        return !string.IsNullOrWhiteSpace(priority) && All.Contains(priority);
    }

    public static string ToDatabaseValue(ClaimPriority priority)
    {
        return priority.ToString();
    }

    public static bool TryParseDatabaseValue(string? value, out ClaimPriority priority)
    {
        return Enum.TryParse(value, ignoreCase: false, out priority) && IsValid(value);
    }
}
