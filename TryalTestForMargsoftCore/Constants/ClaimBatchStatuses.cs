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

    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && All.Contains(status);
    }

    public static string ToDatabaseValue(ClaimBatchStatus status)
    {
        return status.ToString();
    }

    public static bool TryParseDatabaseValue(string? value, out ClaimBatchStatus status)
    {
        return Enum.TryParse(value, ignoreCase: false, out status) && IsValid(value);
    }
}
