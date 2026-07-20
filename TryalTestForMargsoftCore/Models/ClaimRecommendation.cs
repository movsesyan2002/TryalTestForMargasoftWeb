using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryalTestForMargsoftCore.Constants;
using TryalTestForMargsoftCore.Enums;

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

    [Required]
    [StringLength(20)]
    public string DecisionStatus { get; set; } = ClaimDecisionStatuses.Pending;

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

    /// <summary>
    /// Stores the generated recommendation and resets any prior decision fields.
    /// </summary>
    public void SetRecommendedAction(RecommendedAction action)
    {
        RecommendedAction = RecommendedActions.ToDatabaseValue(action);
        DecisionStatus = ClaimDecisionStatuses.Pending;
        FinalAction = null;
        OverrideReason = null;
        DecidedBy = null;
        DecidedAt = null;
    }

    /// <summary>
    /// Accepts the generated recommendation as the final decision.
    /// </summary>
    public void Confirm(string decidedBy, DateTimeOffset decidedAt)
    {
        if (!RecommendedActions.IsValid(RecommendedAction))
        {
            throw new InvalidOperationException("Cannot confirm a recommendation with an invalid recommended action.");
        }

        DecisionStatus = ClaimDecisionStatuses.Confirmed;
        FinalAction = RecommendedAction;
        OverrideReason = null;
        DecidedBy = decidedBy;
        DecidedAt = decidedAt;
    }

    /// <summary>
    /// Records a manual final action in place of the generated recommendation.
    /// </summary>
    public void Override(RecommendedAction finalAction, string reason, string decidedBy, DateTimeOffset decidedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Override reason is required.", nameof(reason));
        }

        DecisionStatus = ClaimDecisionStatuses.Overridden;
        FinalAction = RecommendedActions.ToDatabaseValue(finalAction);
        OverrideReason = reason.Trim();
        DecidedBy = decidedBy;
        DecidedAt = decidedAt;
    }
}
