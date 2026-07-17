using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TryalTestForMargsoftCore.Models;

[Table("ClaimRecommendation")]
public class ClaimRecommendation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long MedicalClaimId { get; set; }

    [Required]
    [StringLength(150)]
    public string RecommendedAction { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "text")]
    public string Explanation { get; set; } = string.Empty;

    [Column(TypeName = "numeric(5, 2)")]
    public decimal? Score { get; set; }

    [Required]
    [StringLength(20)]
    public string DecisionStatus { get; set; } = "Pending";

    [StringLength(150)]
    public string? FinalAction { get; set; }

    [StringLength(1000)]
    public string? OverrideReason { get; set; }

    [StringLength(150)]
    public string? DecidedBy { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset? DecidedAt { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [ForeignKey(nameof(MedicalClaimId))]
    public MedicalClaim MedicalClaim { get; set; } = null!;
}
