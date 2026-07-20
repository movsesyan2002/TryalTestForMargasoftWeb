using Microsoft.EntityFrameworkCore;
using TryalTestForMargasoftInfrastrcture.Data;
using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargasoftInfrastrcture.Repositories;

public sealed class EfHospitalRepository
{
    private readonly TryalTestDbContext _dbContext;

    public EfHospitalRepository(TryalTestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Finds a hospital by identifier, or returns null when it does not exist.
    /// </summary>
    public Task<Hospital?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Hospitals
            .FirstOrDefaultAsync(hospital => hospital.Id == id, cancellationToken);
    }

    /// <summary>
    /// Lists all hospitals ordered by name for selection controls.
    /// </summary>
    public async Task<IReadOnlyCollection<Hospital>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hospitals
            .OrderBy(hospital => hospital.Name)
            .ToListAsync(cancellationToken);
    }
}
