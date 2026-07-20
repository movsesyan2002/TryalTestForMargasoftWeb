using Microsoft.AspNetCore.Mvc;
using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftShared.Lookups;

namespace TryalTestForMargasoftApi.Controllers;

[ApiController]
[Route("api/insurance-companies")]
public sealed class InsuranceCompaniesController : ControllerBase
{
    private readonly IClaimLookupService _claimLookups;

    public InsuranceCompaniesController(IClaimLookupService claimLookups)
    {
        _claimLookups = claimLookups;
    }

    /// <summary>
    /// Returns insurance companies that can be selected when creating a medical claim.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InsuranceCompanyResponse>>> ListInsuranceCompanies(CancellationToken cancellationToken)
    {
        var companies = await _claimLookups.ListInsuranceCompaniesAsync(cancellationToken);
        return Ok(companies);
    }
}
