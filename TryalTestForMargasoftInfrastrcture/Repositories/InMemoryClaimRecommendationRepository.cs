using TryalTestForMargsoftCore.Models;
using TryalTestForMargsoftCore.Repositories;

namespace TryalTestForMargasoftInfrastrcture.Repositories;

public sealed class InMemoryClaimRecommendationRepository : IClaimRecommendationRepository
{
    private readonly Lock _gate = new();
    private readonly List<ClaimRecommendation> _recommendations = [];
    private long _nextId = 1;

    public Task<ClaimRecommendation> AddAsync(ClaimRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        lock (_gate)
        {
            recommendation.Id = _nextId++;
            _recommendations.Add(recommendation);
            return Task.FromResult(recommendation);
        }
    }

    public Task<ClaimRecommendation?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_recommendations.FirstOrDefault(recommendation => recommendation.Id == id));
        }
    }

    public Task<ClaimRecommendation?> GetLatestForClaimAsync(long medicalClaimId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var recommendation = _recommendations
                .Where(item => item.MedicalClaimId == medicalClaimId)
                .OrderByDescending(item => item.GeneratedAt)
                .ThenByDescending(item => item.Id)
                .FirstOrDefault();

            return Task.FromResult(recommendation);
        }
    }

    public Task<IReadOnlyCollection<ClaimRecommendation>> ListByClaimAsync(long medicalClaimId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var recommendations = _recommendations
                .Where(recommendation => recommendation.MedicalClaimId == medicalClaimId)
                .OrderByDescending(recommendation => recommendation.GeneratedAt)
                .ThenByDescending(recommendation => recommendation.Id)
                .ToList();

            return Task.FromResult<IReadOnlyCollection<ClaimRecommendation>>(recommendations);
        }
    }

    public Task<ClaimRecommendation> UpdateAsync(ClaimRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        lock (_gate)
        {
            var index = _recommendations.FindIndex(existing => existing.Id == recommendation.Id);
            if (index < 0)
            {
                throw new InvalidOperationException($"Claim recommendation {recommendation.Id} was not found.");
            }

            _recommendations[index] = recommendation;
            return Task.FromResult(recommendation);
        }
    }
}
