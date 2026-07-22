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

    /// <summary>
    /// Returns medical claims matching optional search, filter, and pagination query parameters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedMedicalClaimResponse>> ListClaims(
        [FromQuery] MedicalClaimSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var claims = await _claimWorkflow.ListClaimsAsync(request, cancellationToken);
            return Ok(claims);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    /// <summary>
    /// Returns one medical claim by identifier, or 404 when it does not exist.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<MedicalClaimResponse>> GetClaim(long id, CancellationToken cancellationToken)
    {
        var claim = await _claimWorkflow.GetClaimAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }

    /// <summary>
    /// Creates a medical claim and returns the generated recommendation.
    /// </summary>
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

    /// <summary>
    /// Updates an existing medical claim and returns the refreshed claim.
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<MedicalClaimResponse>> UpdateClaim(
        long id,
        UpdateMedicalClaimRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var claim = await _claimWorkflow.UpdateClaimAsync(id, request, cancellationToken);
            return Ok(claim);
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

    /// <summary>
    /// Generates a new recommendation for an existing claim.
    /// </summary>
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

    /// <summary>
    /// Confirms the generated recommendation as the final action.
    /// </summary>
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

    /// <summary>
    /// Overrides the generated recommendation with a manually selected final action.
    /// </summary>
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

    /// <summary>
    /// Parses either the database label or enum name for a recommended action.
    /// </summary>
    private static bool TryParseAction(string? value, out RecommendedAction action)
    {
        if (RecommendedActions.TryParseDatabaseValue(value, out action))
        {
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out action);
    }
}
