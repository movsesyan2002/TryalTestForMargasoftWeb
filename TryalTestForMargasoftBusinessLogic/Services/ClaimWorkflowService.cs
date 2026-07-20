using AutoMapper;
using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftInfrastrcture.Repositories;
using TryalTestForMargasoftShared.MedicalClaims;
using TryalTestForMargsoftCore.Constants;
using TryalTestForMargsoftCore.Enums;
using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargasoftBusinessLogic.Services;

public sealed class ClaimWorkflowService : IClaimWorkflowService
{
    private readonly EfMedicalClaimRepository _medicalClaims;
    private readonly EfClaimRecommendationRepository _recommendations;
    private readonly EfHospitalRepository _hospitals;
    private readonly EfInsuranceCompanyRepository _insuranceCompanies;
    private readonly IMapper _mapper;

    public ClaimWorkflowService(
        EfMedicalClaimRepository medicalClaims,
        EfClaimRecommendationRepository recommendations,
        EfHospitalRepository hospitals,
        EfInsuranceCompanyRepository insuranceCompanies,
        IMapper mapper)
    {
        _medicalClaims = medicalClaims;
        _recommendations = recommendations;
        _hospitals = hospitals;
        _insuranceCompanies = insuranceCompanies;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a medical claim, recalculates its financial values, and generates the initial recommendation.
    /// </summary>
    public async Task<MedicalClaimResponse> CreateClaimAsync(CreateMedicalClaimRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCreateRequest(request);
        await ValidateLookupReferencesAsync(request, cancellationToken);

        var claim = new MedicalClaim
        {
            BatchId = request.BatchId,
            ClaimNumber = request.ClaimNumber.Trim(),
            HospitalId = request.HospitalId,
            InsuranceCompanyId = request.InsuranceCompanyId,
            PatientIdentifier = request.PatientIdentifier.Trim(),
            PatientDateOfBirth = request.PatientDateOfBirth,
            PolicyNumber = TrimToNull(request.PolicyNumber),
            DateOfService = request.DateOfService,
            DateClaimSubmitted = request.DateClaimSubmitted,
            AmountBilled = request.AmountBilled,
            ExpectedPaymentAmount = request.ExpectedPaymentAmount,
            AmountPaid = request.AmountPaid,
            Division = TrimToNull(request.Division),
            DenialReason = TrimToNull(request.DenialReason),
            DenialCode = TrimToNull(request.DenialCode),
            PayerResponseDate = request.PayerResponseDate,
            LastFollowUpDate = request.LastFollowUpDate,
            DocumentationComplete = request.DocumentationComplete,
            StatuteOfLimitationsDate = request.StatuteOfLimitationsDate,
            Status = string.IsNullOrWhiteSpace(request.Status) ? MedicalClaimStatuses.New : request.Status.Trim()
        };

        claim.RecalculateFinancials();
        claim.Priority = DeterminePriority(claim, CalculateValues(claim));

        await _medicalClaims.AddAsync(claim, cancellationToken);

        return await AnalyzeExistingClaimAsync(claim, cancellationToken);
    }

    /// <summary>
    /// Gets a medical claim by identifier with its latest recommendation, when the claim exists.
    /// </summary>
    public async Task<MedicalClaimResponse?> GetClaimAsync(long id, CancellationToken cancellationToken = default)
    {
        var claim = await _medicalClaims.GetByIdAsync(id, cancellationToken);
        if (claim is null)
        {
            return null;
        }

        var latestRecommendation = await _recommendations.GetLatestForClaimAsync(id, cancellationToken);
        return MapClaim(claim, CalculateValues(claim), latestRecommendation);
    }

    /// <summary>
    /// Lists all medical claims with calculated values and their latest recommendations.
    /// </summary>
    public async Task<IReadOnlyCollection<MedicalClaimResponse>> ListClaimsAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _medicalClaims.ListAsync(cancellationToken);
        var responses = new List<MedicalClaimResponse>(claims.Count);

        foreach (var claim in claims.OrderByDescending(claim => claim.CreatedAt))
        {
            var latestRecommendation = await _recommendations.GetLatestForClaimAsync(claim.Id, cancellationToken);
            responses.Add(MapClaim(claim, CalculateValues(claim), latestRecommendation));
        }

        return responses;
    }

    /// <summary>
    /// Re-runs recommendation analysis for an existing medical claim.
    /// </summary>
    public async Task<MedicalClaimResponse> AnalyzeClaimAsync(long id, CancellationToken cancellationToken = default)
    {
        var claim = await _medicalClaims.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Medical claim {id} was not found.");

        return await AnalyzeExistingClaimAsync(claim, cancellationToken);
    }

    /// <summary>
    /// Confirms the generated recommendation as the final action.
    /// </summary>
    public async Task<ClaimRecommendationResponse> ConfirmRecommendationAsync(
        long recommendationId,
        string decidedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(decidedBy))
        {
            throw new ArgumentException("Decided by is required.", nameof(decidedBy));
        }

        var recommendation = await _recommendations.GetByIdAsync(recommendationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim recommendation {recommendationId} was not found.");

        recommendation.Confirm(decidedBy.Trim(), DateTimeOffset.UtcNow);
        await _recommendations.UpdateAsync(recommendation, cancellationToken);

        return MapRecommendation(recommendation);
    }

    /// <summary>
    /// Replaces the generated recommendation with a manually selected final action.
    /// </summary>
    public async Task<ClaimRecommendationResponse> OverrideRecommendationAsync(
        long recommendationId,
        RecommendedAction finalAction,
        string overrideReason,
        string decidedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(decidedBy))
        {
            throw new ArgumentException("Decided by is required.", nameof(decidedBy));
        }

        var recommendation = await _recommendations.GetByIdAsync(recommendationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim recommendation {recommendationId} was not found.");

        recommendation.Override(finalAction, overrideReason, decidedBy.Trim(), DateTimeOffset.UtcNow);
        await _recommendations.UpdateAsync(recommendation, cancellationToken);

        return MapRecommendation(recommendation);
    }

    /// <summary>
    /// Updates claim calculations and stores a fresh recommendation for an already persisted claim.
    /// </summary>
    private async Task<MedicalClaimResponse> AnalyzeExistingClaimAsync(MedicalClaim claim, CancellationToken cancellationToken)
    {
        claim.RecalculateFinancials();

        var calculatedValues = CalculateValues(claim);
        claim.Priority = DeterminePriority(claim, calculatedValues);

        if (claim.Status is MedicalClaimStatuses.New or MedicalClaimStatuses.InReview)
        {
            claim.Status = MedicalClaimStatuses.Recommended;
        }

        await _medicalClaims.UpdateAsync(claim, cancellationToken);

        var recommendationResult = RecommendAction(claim, calculatedValues);
        var recommendation = new ClaimRecommendation
        {
            MedicalClaimId = claim.Id,
            MedicalClaim = claim,
            Explanation = recommendationResult.Explanation,
            Score = recommendationResult.Score,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        recommendation.SetRecommendedAction(recommendationResult.Action);
        recommendation.Explanation = recommendationResult.Explanation;
        recommendation.Score = recommendationResult.Score;

        await _recommendations.AddAsync(recommendation, cancellationToken);
        claim.ClaimRecommendations.Add(recommendation);

        return MapClaim(claim, calculatedValues, recommendation);
    }

    /// <summary>
    /// Calculates derived financial, aging, patient, and legal-deadline values for recommendation rules.
    /// </summary>
    private static ClaimCalculatedValues CalculateValues(MedicalClaim claim)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageStart = claim.DateClaimSubmitted ?? claim.DateOfService;
        var claimAgeDays = Math.Max(today.DayNumber - ageStart.DayNumber, 0);
        int? daysUntilDeadline = claim.StatuteOfLimitationsDate is null
            ? null
            : claim.StatuteOfLimitationsDate.Value.DayNumber - today.DayNumber;

        return new ClaimCalculatedValues
        {
            OutstandingBalance = claim.CalculateOutstandingBalance(),
            UnderpaymentAmount = claim.CalculateUnderpaymentAmount(),
            ClaimAgeDays = claimAgeDays,
            PatientAgeYears = CalculatePatientAge(claim.PatientDateOfBirth, today),
            DaysUntilDeadline = daysUntilDeadline,
            Urgency = DetermineUrgency(claimAgeDays, daysUntilDeadline)
        };
    }

    /// <summary>
    /// Chooses the recovery action from claim balance, documentation, denial, age, and deadline signals.
    /// </summary>
    private static RecommendationResult RecommendAction(MedicalClaim claim, ClaimCalculatedValues values)
    {
        var balanceAtIssue = Math.Max(values.OutstandingBalance, values.UnderpaymentAmount ?? 0);
        var hasDenialOrDispute = !string.IsNullOrWhiteSpace(claim.DenialCode)
            || !string.IsNullOrWhiteSpace(claim.DenialReason);
        var daysSinceFollowUp = claim.LastFollowUpDate is null
            ? (int?)null
            : DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - claim.LastFollowUpDate.Value.DayNumber;

        if (claim.Status is MedicalClaimStatuses.Closed or MedicalClaimStatuses.Recovered)
        {
            return BuildRecommendation(
                RecommendedAction.NoAction,
                "The claim is already closed or recovered, so no recovery action is needed.",
                0);
        }

        if (balanceAtIssue <= 0)
        {
            return BuildRecommendation(
                RecommendedAction.NoAction,
                "The calculated outstanding balance and underpayment amount are zero, so there is no current recovery amount to pursue.",
                5);
        }

        if (!claim.DocumentationComplete)
        {
            return BuildRecommendation(
                RecommendedAction.RequestMoreInformation,
                "The claim has money at issue, but the documentation is incomplete. Complete records are needed before legal recovery can be evaluated.",
                CalculateScore(values, balanceAtIssue, hasDenialOrDispute));
        }

        if (values.DaysUntilDeadline is <= 15)
        {
            return BuildRecommendation(
                RecommendedAction.EscalateForAttorneyReview,
                "The legal deadline is within 15 days, so attorney review should happen before the recovery window is missed.",
                100);
        }

        if (hasDenialOrDispute && balanceAtIssue >= 25000 && values.ClaimAgeDays >= 90)
        {
            return BuildRecommendation(
                RecommendedAction.EscalateForAttorneyReview,
                "The claim has denial or dispute information, a high balance, and has aged at least 90 days. That combination needs attorney review.",
                CalculateScore(values, balanceAtIssue, hasDenialOrDispute));
        }

        if (balanceAtIssue >= 25000 && values.ClaimAgeDays >= 60)
        {
            return BuildRecommendation(
                RecommendedAction.PrepareDemandLetter,
                "The balance is high and the claim has been pending for at least 60 days, so a formal demand letter is appropriate.",
                CalculateScore(values, balanceAtIssue, hasDenialOrDispute));
        }

        if (hasDenialOrDispute)
        {
            return BuildRecommendation(
                RecommendedAction.ReviewDenial,
                "The payer supplied denial or dispute information. Review the denial basis before deciding whether to demand payment or escalate.",
                CalculateScore(values, balanceAtIssue, hasDenialOrDispute));
        }

        if (values.ClaimAgeDays >= 45 || daysSinceFollowUp >= 30)
        {
            return BuildRecommendation(
                RecommendedAction.FollowUpWithPayerProvider,
                "The claim still has a balance and has aged enough to justify active follow-up with the payer or provider.",
                CalculateScore(values, balanceAtIssue, hasDenialOrDispute));
        }

        return BuildRecommendation(
            RecommendedAction.Monitor,
            "The claim has an open balance, but it is still relatively recent and has no denial or urgent deadline signals.",
            CalculateScore(values, balanceAtIssue, hasDenialOrDispute));
    }

    /// <summary>
    /// Assigns claim priority from legal deadline, balance, denial, and aging signals.
    /// </summary>
    private static string DeterminePriority(MedicalClaim claim, ClaimCalculatedValues values)
    {
        var balanceAtIssue = Math.Max(values.OutstandingBalance, values.UnderpaymentAmount ?? 0);
        var hasDenialOrDispute = !string.IsNullOrWhiteSpace(claim.DenialCode)
            || !string.IsNullOrWhiteSpace(claim.DenialReason);

        if (values.DaysUntilDeadline is <= 15 || balanceAtIssue >= 25000 && values.ClaimAgeDays >= 90)
        {
            return ClaimPriorities.Urgent;
        }

        if (values.DaysUntilDeadline is <= 30 || balanceAtIssue >= 10000 || hasDenialOrDispute && values.ClaimAgeDays >= 30)
        {
            return ClaimPriorities.High;
        }

        return balanceAtIssue > 0 ? ClaimPriorities.Normal : ClaimPriorities.Low;
    }

    /// <summary>
    /// Converts claim age and deadline proximity into a user-facing urgency label.
    /// </summary>
    private static string DetermineUrgency(int claimAgeDays, int? daysUntilDeadline)
    {
        if (daysUntilDeadline is <= 15)
        {
            return "Immediate";
        }

        if (daysUntilDeadline is <= 30 || claimAgeDays >= 180)
        {
            return "High";
        }

        if (daysUntilDeadline is <= 90 || claimAgeDays >= 90)
        {
            return "Moderate";
        }

        return "Normal";
    }

    /// <summary>
    /// Calculates the patient age in whole years on the supplied date.
    /// </summary>
    private static int? CalculatePatientAge(DateOnly? dateOfBirth, DateOnly today)
    {
        if (dateOfBirth is null)
        {
            return null;
        }

        var age = today.Year - dateOfBirth.Value.Year;
        if (today < dateOfBirth.Value.AddYears(age))
        {
            age--;
        }

        return Math.Max(age, 0);
    }

    /// <summary>
    /// Produces a capped recommendation score from claim value, age, denial status, and deadline proximity.
    /// </summary>
    private static decimal CalculateScore(ClaimCalculatedValues values, decimal balanceAtIssue, bool hasDenialOrDispute)
    {
        var score = 20m;

        score += Math.Min(balanceAtIssue / 1000m, 30m);
        score += Math.Min(values.ClaimAgeDays / 3m, 25m);

        if (hasDenialOrDispute)
        {
            score += 15m;
        }

        if (values.DaysUntilDeadline is <= 30)
        {
            score += 20m;
        }
        else if (values.DaysUntilDeadline is <= 90)
        {
            score += 10m;
        }

        return Math.Min(decimal.Round(score, 2), 100m);
    }

    /// <summary>
    /// Builds a recommendation result with a rounded score capped at 100.
    /// </summary>
    private static RecommendationResult BuildRecommendation(RecommendedAction action, string explanation, decimal score)
    {
        return new RecommendationResult(action, explanation, Math.Min(decimal.Round(score, 2), 100m));
    }

    /// <summary>
    /// Validates the minimum required fields and value ranges for creating a claim.
    /// </summary>
    private static void ValidateCreateRequest(CreateMedicalClaimRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClaimNumber))
        {
            throw new ArgumentException("Claim number is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.PatientIdentifier))
        {
            throw new ArgumentException("Patient identifier is required.", nameof(request));
        }

        if (request.HospitalId <= 0)
        {
            throw new ArgumentException("Hospital is required.", nameof(request));
        }

        if (request.InsuranceCompanyId <= 0)
        {
            throw new ArgumentException("Insurance company is required.", nameof(request));
        }

        if (request.DateOfService == default)
        {
            throw new ArgumentException("Date of service is required.", nameof(request));
        }

        if (request.DateClaimSubmitted < request.DateOfService)
        {
            throw new ArgumentException("Claim submitted date cannot be before the date of service.", nameof(request));
        }

        if (request.AmountBilled < 0 || request.AmountPaid < 0 || request.ExpectedPaymentAmount < 0)
        {
            throw new ArgumentException("Claim amounts cannot be negative.", nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && !MedicalClaimStatuses.IsValid(request.Status.Trim()))
        {
            throw new ArgumentException("Claim status is invalid.", nameof(request));
        }
    }

    /// <summary>
    /// Verifies claim lookup identifiers point to existing hospital and insurance company records.
    /// </summary>
    private async Task ValidateLookupReferencesAsync(CreateMedicalClaimRequest request, CancellationToken cancellationToken)
    {
        var hospital = await _hospitals.GetByIdAsync(request.HospitalId, cancellationToken);
        if (hospital is null)
        {
            throw new ArgumentException("Hospital was not found.", nameof(request));
        }

        var insuranceCompany = await _insuranceCompanies.GetByIdAsync(request.InsuranceCompanyId, cancellationToken);
        if (insuranceCompany is null)
        {
            throw new ArgumentException("Insurance company was not found.", nameof(request));
        }
    }

    /// <summary>
    /// Maps a claim entity to its API response and injects calculated values and the latest recommendation.
    /// </summary>
    private MedicalClaimResponse MapClaim(
        MedicalClaim claim,
        ClaimCalculatedValues calculatedValues,
        ClaimRecommendation? latestRecommendation)
    {
        var response = _mapper.Map<MedicalClaimResponse>(claim);

        response.OutstandingBalance = calculatedValues.OutstandingBalance;
        response.UnderpaymentAmount = calculatedValues.UnderpaymentAmount;
        response.CalculatedValues = calculatedValues;
        response.LatestRecommendation = latestRecommendation is null
            ? null
            : MapRecommendation(latestRecommendation);

        return response;
    }

    /// <summary>
    /// Maps a recommendation entity to its API response.
    /// </summary>
    private ClaimRecommendationResponse MapRecommendation(ClaimRecommendation recommendation)
    {
        return _mapper.Map<ClaimRecommendationResponse>(recommendation);
    }

    /// <summary>
    /// Normalizes optional text by trimming meaningful values and storing blanks as null.
    /// </summary>
    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record RecommendationResult(RecommendedAction Action, string Explanation, decimal Score);
}
