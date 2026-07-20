using TryalTestForMargasoftShared.Lookups;

namespace TryalTestForMargasoftBusinessLogic.Interfaces;

public interface IClaimLookupService
{
    /// <summary>
    /// Lists hospitals that can be selected when creating a medical claim.
    /// </summary>
    Task<IReadOnlyCollection<HospitalResponse>> ListHospitalsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists insurance companies that can be selected when creating a medical claim.
    /// </summary>
    Task<IReadOnlyCollection<InsuranceCompanyResponse>> ListInsuranceCompaniesAsync(CancellationToken cancellationToken = default);
}
