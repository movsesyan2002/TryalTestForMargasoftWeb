namespace TryalTestForMargasoftShared.MedicalClaims;

public sealed class MedicalClaimSearchRequest
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public long? HospitalId { get; set; }

    public long? InsuranceCompanyId { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;
}
