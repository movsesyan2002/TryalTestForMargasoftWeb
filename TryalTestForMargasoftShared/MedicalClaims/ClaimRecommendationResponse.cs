namespace TryalTestForMargasoftShared.MedicalClaims;

public sealed class ClaimRecommendationResponse
{
    public long Id { get; set; }

    public long MedicalClaimId { get; set; }

    public string RecommendedAction { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public decimal? Score { get; set; }

    public string DecisionStatus { get; set; } = string.Empty;

    public string? FinalAction { get; set; }

    public string? OverrideReason { get; set; }

    public string? DecidedBy { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }
}
