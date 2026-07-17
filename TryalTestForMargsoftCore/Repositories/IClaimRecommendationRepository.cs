using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargsoftCore.Repositories;

public interface IClaimRecommendationRepository
{
    Task<ClaimRecommendation> AddAsync(ClaimRecommendation recommendation, CancellationToken cancellationToken = default);

    Task<ClaimRecommendation?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<ClaimRecommendation?> GetLatestForClaimAsync(long medicalClaimId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClaimRecommendation>> ListByClaimAsync(long medicalClaimId, CancellationToken cancellationToken = default);

    Task<ClaimRecommendation> UpdateAsync(ClaimRecommendation recommendation, CancellationToken cancellationToken = default);
}
