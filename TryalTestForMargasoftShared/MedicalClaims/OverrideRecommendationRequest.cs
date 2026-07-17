namespace TryalTestForMargasoftShared.MedicalClaims;

public sealed class OverrideRecommendationRequest
{
    public string FinalAction { get; set; } = string.Empty;

    public string OverrideReason { get; set; } = string.Empty;

    public string DecidedBy { get; set; } = string.Empty;
}
