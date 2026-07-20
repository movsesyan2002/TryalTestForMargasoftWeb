using Microsoft.EntityFrameworkCore;
using TryalTestForMargasoftInfrastrcture.Data;
using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargasoftInfrastrcture.Repositories;

public sealed class EfMedicalClaimRepository
{
    private readonly TryalTestDbContext _dbContext;

    public EfMedicalClaimRepository(TryalTestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a medical claim and saves the change immediately.
    /// </summary>
    public async Task<MedicalClaim> AddAsync(MedicalClaim claim, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(claim);

        _dbContext.MedicalClaims.Add(claim);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return claim;
    }

    /// <summary>
    /// Finds a medical claim by identifier, or returns null when it does not exist.
    /// </summary>
    public Task<MedicalClaim?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.MedicalClaims
            .FirstOrDefaultAsync(claim => claim.Id == id, cancellationToken);
    }

    /// <summary>
    /// Lists all medical claims currently stored in the database.
    /// </summary>
    public async Task<IReadOnlyCollection<MedicalClaim>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.MedicalClaims
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing medical claim and saves the change immediately.
    /// </summary>
    public async Task<MedicalClaim> UpdateAsync(MedicalClaim claim, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(claim);

        var exists = await _dbContext.MedicalClaims
            .AnyAsync(existing => existing.Id == claim.Id, cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException($"Medical claim {claim.Id} was not found.");
        }

        _dbContext.MedicalClaims.Update(claim);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return claim;
    }
}
