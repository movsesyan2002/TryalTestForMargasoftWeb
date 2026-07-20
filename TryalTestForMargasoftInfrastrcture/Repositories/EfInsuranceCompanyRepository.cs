using Microsoft.EntityFrameworkCore;
using TryalTestForMargasoftInfrastrcture.Data;
using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargasoftInfrastrcture.Repositories;

public sealed class EfInsuranceCompanyRepository
{
    private readonly TryalTestDbContext _dbContext;

    public EfInsuranceCompanyRepository(TryalTestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Finds an insurance company by identifier, or returns null when it does not exist.
    /// </summary>
    public Task<InsuranceCompany?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.InsuranceCompanies
            .FirstOrDefaultAsync(company => company.Id == id, cancellationToken);
    }

    /// <summary>
    /// Lists all insurance companies ordered by name for selection controls.
    /// </summary>
    public async Task<IReadOnlyCollection<InsuranceCompany>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.InsuranceCompanies
            .OrderBy(company => company.Name)
            .ToListAsync(cancellationToken);
    }
}
