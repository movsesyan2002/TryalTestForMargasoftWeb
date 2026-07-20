using Microsoft.EntityFrameworkCore;
using TryalTestForMargasoftInfrastrcture.Data;
using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargasoftInfrastrcture.Repositories;

public sealed class EfClaimRecommendationRepository
{
    private readonly TryalTestDbContext _dbContext;

    public EfClaimRecommendationRepository(TryalTestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a claim recommendation and saves the change immediately.
    /// </summary>
    public async Task<ClaimRecommendation> AddAsync(
        ClaimRecommendation recommendation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        _dbContext.ClaimRecommendations.Add(recommendation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return recommendation;
    }

    /// <summary>
    /// Finds a claim recommendation by identifier, or returns null when it does not exist.
    /// </summary>
    public Task<ClaimRecommendation?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ClaimRecommendations
            .FirstOrDefaultAsync(recommendation => recommendation.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets the newest recommendation for a medical claim by generated time and identifier.
    /// </summary>
    public Task<ClaimRecommendation?> GetLatestForClaimAsync(
        long medicalClaimId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ClaimRecommendations
            .Where(recommendation => recommendation.MedicalClaimId == medicalClaimId)
            .OrderByDescending(recommendation => recommendation.GeneratedAt)
            .ThenByDescending(recommendation => recommendation.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Lists recommendations for a medical claim from newest to oldest.
    /// </summary>
    public async Task<IReadOnlyCollection<ClaimRecommendation>> ListByClaimAsync(
        long medicalClaimId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimRecommendations
            .Where(recommendation => recommendation.MedicalClaimId == medicalClaimId)
            .OrderByDescending(recommendation => recommendation.GeneratedAt)
            .ThenByDescending(recommendation => recommendation.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing claim recommendation and saves the change immediately.
    /// </summary>
    public async Task<ClaimRecommendation> UpdateAsync(
        ClaimRecommendation recommendation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        var exists = await _dbContext.ClaimRecommendations
            .AnyAsync(existing => existing.Id == recommendation.Id, cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException($"Claim recommendation {recommendation.Id} was not found.");
        }

        _dbContext.ClaimRecommendations.Update(recommendation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return recommendation;
    }
}
