namespace TryalTestForMargsoftCore.Constants;

using TryalTestForMargsoftCore.Enums;

public static class ClaimBatchStatuses
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Closed = "Closed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Draft,
        Active,
        Closed
    };

    /// <summary>
    /// Returns whether the supplied database value is a supported claim batch status.
    /// </summary>
    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && All.Contains(status);
    }

    /// <summary>
    /// Converts a claim batch status enum value to the database label.
    /// </summary>
    public static string ToDatabaseValue(ClaimBatchStatus status)
    {
        return status.ToString();
    }

    /// <summary>
    /// Parses a database label into the matching claim batch status enum value.
    /// </summary>
    public static bool TryParseDatabaseValue(string? value, out ClaimBatchStatus status)
    {
        return Enum.TryParse(value, ignoreCase: false, out status) && IsValid(value);
    }
}
