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
    /// Lists medical claims matching the supplied filters and page bounds.
    /// </summary>
    public async Task<(IReadOnlyCollection<MedicalClaim> Claims, int TotalCount)> SearchAsync(
        string? search,
        string? status,
        string? priority,
        long? hospitalId,
        long? insuranceCompanyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MedicalClaims.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{EscapeLikePattern(search.Trim())}%";

            query = query.Where(claim =>
                EF.Functions.ILike(claim.ClaimNumber, searchPattern)
                || EF.Functions.ILike(claim.PatientIdentifier, searchPattern)
                || claim.PolicyNumber != null && EF.Functions.ILike(claim.PolicyNumber, searchPattern)
                || claim.Division != null && EF.Functions.ILike(claim.Division, searchPattern)
                || claim.DenialCode != null && EF.Functions.ILike(claim.DenialCode, searchPattern)
                || claim.DenialReason != null && EF.Functions.ILike(claim.DenialReason, searchPattern)
                || EF.Functions.ILike(claim.Hospital.Name, searchPattern)
                || EF.Functions.ILike(claim.InsuranceCompany.Name, searchPattern));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(claim => claim.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            query = query.Where(claim => claim.Priority == priority);
        }

        if (hospitalId is not null)
        {
            query = query.Where(claim => claim.HospitalId == hospitalId);
        }

        if (insuranceCompanyId is not null)
        {
            query = query.Where(claim => claim.InsuranceCompanyId == insuranceCompanyId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var claims = await query
            .OrderByDescending(claim => claim.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (claims, totalCount);
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

    /// <summary>
    /// Escapes PostgreSQL LIKE wildcards so search input is treated as text.
    /// </summary>
    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }
}
