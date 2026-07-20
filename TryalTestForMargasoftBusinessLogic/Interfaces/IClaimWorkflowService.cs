using TryalTestForMargasoftShared.MedicalClaims;
using TryalTestForMargsoftCore.Enums;

namespace TryalTestForMargasoftBusinessLogic.Interfaces;

public interface IClaimWorkflowService
{
    /// <summary>
    /// Creates a medical claim, recalculates its financial values, and generates the initial recommendation.
    /// </summary>
    Task<MedicalClaimResponse> CreateClaimAsync(CreateMedicalClaimRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a medical claim by identifier with its latest recommendation, when the claim exists.
    /// </summary>
    Task<MedicalClaimResponse?> GetClaimAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all medical claims with calculated values and their latest recommendations.
    /// </summary>
    Task<IReadOnlyCollection<MedicalClaimResponse>> ListClaimsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-runs recommendation analysis for an existing medical claim.
    /// </summary>
    Task<MedicalClaimResponse> AnalyzeClaimAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms the generated recommendation as the final action.
    /// </summary>
    Task<ClaimRecommendationResponse> ConfirmRecommendationAsync(long recommendationId, string decidedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the generated recommendation with a manually selected final action.
    /// </summary>
    Task<ClaimRecommendationResponse> OverrideRecommendationAsync(
        long recommendationId,
        RecommendedAction finalAction,
        string overrideReason,
        string decidedBy,
        CancellationToken cancellationToken = default);
}
