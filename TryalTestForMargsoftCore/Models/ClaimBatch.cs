using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TryalTestForMargsoftCore.Models;

[Table("ClaimBatch")]
public class ClaimBatch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Name { get; set; } = string.Empty;

    public long? HospitalId { get; set; }

    public long? InsuranceCompanyId { get; set; }

    [StringLength(100)]
    public string? Division { get; set; }

    [StringLength(500)]
    public string? DenialReason { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Draft";

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset? ClosedAt { get; set; }

    [ForeignKey(nameof(HospitalId))]
    public Hospital? Hospital { get; set; }

    [ForeignKey(nameof(InsuranceCompanyId))]
    public InsuranceCompany? InsuranceCompany { get; set; }

    public ICollection<MedicalClaim> MedicalClaims { get; set; } = [];
}
