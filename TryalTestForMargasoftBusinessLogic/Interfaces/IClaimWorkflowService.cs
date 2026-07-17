using TryalTestForMargasoftShared.MedicalClaims;
using TryalTestForMargsoftCore.Enums;

namespace TryalTestForMargasoftBusinessLogic.Interfaces;

public interface IClaimWorkflowService
{
    Task<MedicalClaimResponse> CreateClaimAsync(CreateMedicalClaimRequest request, CancellationToken cancellationToken = default);

    Task<MedicalClaimResponse?> GetClaimAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MedicalClaimResponse>> ListClaimsAsync(CancellationToken cancellationToken = default);

    Task<MedicalClaimResponse> AnalyzeClaimAsync(long id, CancellationToken cancellationToken = default);

    Task<ClaimRecommendationResponse> ConfirmRecommendationAsync(long recommendationId, string decidedBy, CancellationToken cancellationToken = default);

    Task<ClaimRecommendationResponse> OverrideRecommendationAsync(
        long recommendationId,
        RecommendedAction finalAction,
        string overrideReason,
        string decidedBy,
        CancellationToken cancellationToken = default);
}
