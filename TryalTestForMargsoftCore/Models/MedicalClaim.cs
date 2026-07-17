using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryalTestForMargsoftCore.Constants;

namespace TryalTestForMargsoftCore.Models;

[Table("MedicalClaim")]
public class MedicalClaim
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string ClaimNumber { get; set; } = string.Empty;


    public long? BatchId { get; set; }


    public long HospitalId { get; set; }

    public long InsuranceCompanyId { get; set; }

    [Required]
    [StringLength(100)]
    public string PatientIdentifier { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateOnly? PatientDateOfBirth { get; set; }

    [StringLength(100)]
    public string? PolicyNumber { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateOnly DateOfService { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? DateClaimSubmitted { get; set; }

    [Required]
    [Column(TypeName = "numeric(18, 2)")]
    public decimal AmountBilled { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? ExpectedPaymentAmount { get; set; }

    [Required]
    [Column(TypeName = "numeric(18, 2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? OutstandingBalance { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? UnderpaymentAmount { get; set; }

    [StringLength(100)]
    public string? Division { get; set; }

    [StringLength(500)]
    public string? DenialReason { get; set; }

    [StringLength(100)]
    public string? DenialCode { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? PayerResponseDate { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? LastFollowUpDate { get; set; }

    public bool DocumentationComplete { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? StatuteOfLimitationsDate { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = MedicalClaimStatuses.New;

    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = ClaimPriorities.Normal;

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [ForeignKey(nameof(BatchId))]
    public ClaimBatch? Batch { get; set; }

    [Required]
    [ForeignKey(nameof(HospitalId))]
    public Hospital Hospital { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(InsuranceCompanyId))]
    public InsuranceCompany InsuranceCompany { get; set; } = null!;

    public ICollection<ClaimRecommendation> ClaimRecommendations { get; set; } = [];

    public decimal CalculateOutstandingBalance()
    {
        return AmountBilled - AmountPaid;
    }

    public decimal? CalculateUnderpaymentAmount()
    {
        return ExpectedPaymentAmount is null
            ? null
            : Math.Max(ExpectedPaymentAmount.Value - AmountPaid, 0);
    }

    public void RecalculateFinancials()
    {
        OutstandingBalance = CalculateOutstandingBalance();
        UnderpaymentAmount = CalculateUnderpaymentAmount();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
