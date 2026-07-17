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

    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && All.Contains(status);
    }

    public static string ToDatabaseValue(MedicalClaimStatus status)
    {
        return status.ToString();
    }

    public static bool TryParseDatabaseValue(string? value, out MedicalClaimStatus status)
    {
        return Enum.TryParse(value, ignoreCase: false, out status) && IsValid(value);
    }
}
