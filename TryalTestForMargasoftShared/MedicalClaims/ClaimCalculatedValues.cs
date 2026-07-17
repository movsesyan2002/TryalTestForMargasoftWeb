namespace TryalTestForMargasoftShared.MedicalClaims;

public sealed class ClaimCalculatedValues
{
    public decimal OutstandingBalance { get; set; }

    public decimal? UnderpaymentAmount { get; set; }

    public int ClaimAgeDays { get; set; }

    public int? PatientAgeYears { get; set; }

    public int? DaysUntilDeadline { get; set; }

    public string Urgency { get; set; } = string.Empty;
}
