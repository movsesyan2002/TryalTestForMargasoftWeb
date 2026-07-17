using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TryalTestForMargsoftCore.Models;

[Table("InsuranceCompany")]
public class InsuranceCompany
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? PayerCode { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ClaimBatch> ClaimBatches { get; set; } = [];

    public ICollection<MedicalClaim> MedicalClaims { get; set; } = [];
}
