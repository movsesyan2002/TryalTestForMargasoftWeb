using Microsoft.AspNetCore.Mvc;
using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftShared.MedicalClaims;
using TryalTestForMargsoftCore.Constants;
using TryalTestForMargsoftCore.Enums;

namespace TryalTestForMargasoftApi.Controllers;

[ApiController]
[Route("api/medical-claims")]
public sealed class MedicalClaimsController : ControllerBase
{
    private readonly IClaimWorkflowService _claimWorkflow;

    public MedicalClaimsController(IClaimWorkflowService claimWorkflow)
    {
        _claimWorkflow = claimWorkflow;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MedicalClaimResponse>>> ListClaims(CancellationToken cancellationToken)
    {
        var claims = await _claimWorkflow.ListClaimsAsync(cancellationToken);
        return Ok(claims);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MedicalClaimResponse>> GetClaim(long id, CancellationToken cancellationToken)
    {
        var claim = await _claimWorkflow.GetClaimAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpPost]
    public async Task<ActionResult<MedicalClaimResponse>> CreateClaim(
        CreateMedicalClaimRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var claim = await _claimWorkflow.CreateClaimAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetClaim), new { id = claim.Id }, claim);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("{id:long}/recommendations")]
    public async Task<ActionResult<MedicalClaimResponse>> AnalyzeClaim(long id, CancellationToken cancellationToken)
    {
        try
        {
            var claim = await _claimWorkflow.AnalyzeClaimAsync(id, cancellationToken);
            return Ok(claim);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("recommendations/{recommendationId:long}/confirm")]
    public async Task<ActionResult<ClaimRecommendationResponse>> ConfirmRecommendation(
        long recommendationId,
        ConfirmRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var recommendation = await _claimWorkflow.ConfirmRecommendationAsync(
                recommendationId,
                request.DecidedBy,
                cancellationToken);

            return Ok(recommendation);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("recommendations/{recommendationId:long}/override")]
    public async Task<ActionResult<ClaimRecommendationResponse>> OverrideRecommendation(
        long recommendationId,
        OverrideRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseAction(request.FinalAction, out var finalAction))
        {
            return BadRequest(new { error = "Final action is invalid." });
        }

        try
        {
            var recommendation = await _claimWorkflow.OverrideRecommendationAsync(
                recommendationId,
                finalAction,
                request.OverrideReason,
                request.DecidedBy,
                cancellationToken);

            return Ok(recommendation);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static bool TryParseAction(string? value, out RecommendedAction action)
    {
        if (RecommendedActions.TryParseDatabaseValue(value, out action))
        {
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out action);
    }
}
