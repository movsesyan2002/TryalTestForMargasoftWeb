namespace TryalTestForMargasoftShared.MedicalClaims;

public sealed class CreateMedicalClaimRequest
{
    public string ClaimNumber { get; set; } = string.Empty;

    public long? BatchId { get; set; }

    public long HospitalId { get; set; }

    public long InsuranceCompanyId { get; set; }

    public string PatientIdentifier { get; set; } = string.Empty;

    public DateOnly? PatientDateOfBirth { get; set; }

    public string? PolicyNumber { get; set; }

    public DateOnly DateOfService { get; set; }

    public DateOnly? DateClaimSubmitted { get; set; }

    public decimal AmountBilled { get; set; }

    public decimal? ExpectedPaymentAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public string? Division { get; set; }

    public string? DenialReason { get; set; }

    public string? DenialCode { get; set; }

    public DateOnly? PayerResponseDate { get; set; }

    public DateOnly? LastFollowUpDate { get; set; }

    public bool DocumentationComplete { get; set; }

    public DateOnly? StatuteOfLimitationsDate { get; set; }

    public string? Status { get; set; }
}
