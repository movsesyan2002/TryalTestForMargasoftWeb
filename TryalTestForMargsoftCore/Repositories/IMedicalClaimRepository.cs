using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargsoftCore.Repositories;

public interface IMedicalClaimRepository
{
    Task<MedicalClaim> AddAsync(MedicalClaim claim, CancellationToken cancellationToken = default);

    Task<MedicalClaim?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MedicalClaim>> ListAsync(CancellationToken cancellationToken = default);

    Task<MedicalClaim> UpdateAsync(MedicalClaim claim, CancellationToken cancellationToken = default);
}
