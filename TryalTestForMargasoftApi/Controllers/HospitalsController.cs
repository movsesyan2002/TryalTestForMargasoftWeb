using Microsoft.AspNetCore.Mvc;
using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftShared.Lookups;

namespace TryalTestForMargasoftApi.Controllers;

[ApiController]
[Route("api/hospitals")]
public sealed class HospitalsController : ControllerBase
{
    private readonly IClaimLookupService _claimLookups;

    public HospitalsController(IClaimLookupService claimLookups)
    {
        _claimLookups = claimLookups;
    }

    /// <summary>
    /// Returns hospitals that can be selected when creating a medical claim.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<HospitalResponse>>> ListHospitals(CancellationToken cancellationToken)
    {
        var hospitals = await _claimLookups.ListHospitalsAsync(cancellationToken);
        return Ok(hospitals);
    }
}
