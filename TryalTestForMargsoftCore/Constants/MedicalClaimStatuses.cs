namespace TryalTestForMargsoftCore.Constants;

using TryalTestForMargsoftCore.Enums;

public static class MedicalClaimStatuses
{
    public const string New = "New";
    public const string InReview = "InReview";
    public const string Recommended = "Recommended";
    public const string InProgress = "InProgress";
    public const string Recovered = "Recovered";
    public const string Closed = "Closed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        New,
        InReview,
        Recommended,
        InProgress,
        Recovered,
        Closed
    };

    /// <summary>
    /// Returns whether the supplied database value is a supported medical claim status.
    /// </summary>
    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && All.Contains(status);
    }

    /// <summary>
    /// Converts a medical claim status enum value to the database label.
    /// </summary>
    public static string ToDatabaseValue(MedicalClaimStatus status)
    {
        return status.ToString();
    }

    /// <summary>
    /// Parses a database label into the matching medical claim status enum value.
    /// </summary>
    public static bool TryParseDatabaseValue(string? value, out MedicalClaimStatus status)
    {
        return Enum.TryParse(value, ignoreCase: false, out status) && IsValid(value);
    }
}
