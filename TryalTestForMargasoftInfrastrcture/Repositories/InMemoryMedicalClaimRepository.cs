using TryalTestForMargsoftCore.Models;
using TryalTestForMargsoftCore.Repositories;

namespace TryalTestForMargasoftInfrastrcture.Repositories;

public sealed class InMemoryMedicalClaimRepository : IMedicalClaimRepository
{
    private readonly Lock _gate = new();
    private readonly List<MedicalClaim> _claims = [];
    private long _nextId = 1;

    public Task<MedicalClaim> AddAsync(MedicalClaim claim, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(claim);

        lock (_gate)
        {
            claim.Id = _nextId++;
            _claims.Add(claim);
            return Task.FromResult(claim);
        }
    }

    public Task<MedicalClaim?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_claims.FirstOrDefault(claim => claim.Id == id));
        }
    }

    public Task<IReadOnlyCollection<MedicalClaim>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<MedicalClaim>>(_claims.ToList());
        }
    }

    public Task<MedicalClaim> UpdateAsync(MedicalClaim claim, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(claim);

        lock (_gate)
        {
            var index = _claims.FindIndex(existing => existing.Id == claim.Id);
            if (index < 0)
            {
                throw new InvalidOperationException($"Medical claim {claim.Id} was not found.");
            }

            _claims[index] = claim;
            return Task.FromResult(claim);
        }
    }
}
