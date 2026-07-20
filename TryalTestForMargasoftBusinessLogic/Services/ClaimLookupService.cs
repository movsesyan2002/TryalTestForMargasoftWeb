using AutoMapper;
using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftInfrastrcture.Repositories;
using TryalTestForMargasoftShared.Lookups;

namespace TryalTestForMargasoftBusinessLogic.Services;

public sealed class ClaimLookupService : IClaimLookupService
{
    private readonly EfHospitalRepository _hospitals;
    private readonly EfInsuranceCompanyRepository _insuranceCompanies;
    private readonly IMapper _mapper;

    public ClaimLookupService(
        EfHospitalRepository hospitals,
        EfInsuranceCompanyRepository insuranceCompanies,
        IMapper mapper)
    {
        _hospitals = hospitals;
        _insuranceCompanies = insuranceCompanies;
        _mapper = mapper;
    }

    /// <summary>
    /// Lists hospitals that can be selected when creating a medical claim.
    /// </summary>
    public async Task<IReadOnlyCollection<HospitalResponse>> ListHospitalsAsync(CancellationToken cancellationToken = default)
    {
        var hospitals = await _hospitals.ListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<HospitalResponse>>(hospitals);
    }

    /// <summary>
    /// Lists insurance companies that can be selected when creating a medical claim.
    /// </summary>
    public async Task<IReadOnlyCollection<InsuranceCompanyResponse>> ListInsuranceCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _insuranceCompanies.ListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<InsuranceCompanyResponse>>(companies);
    }
}
