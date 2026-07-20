using Microsoft.EntityFrameworkCore;
using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargasoftInfrastrcture.Data;

public sealed class TryalTestDbContext : DbContext
{
    public TryalTestDbContext(DbContextOptions<TryalTestDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClaimBatch> ClaimBatches { get; set; } = default!;

    public DbSet<ClaimRecommendation> ClaimRecommendations { get; set; } = default!;

    public DbSet<Hospital> Hospitals { get; set; } = default!;

    public DbSet<InsuranceCompany> InsuranceCompanies { get; set; } = default!;

    public DbSet<MedicalClaim> MedicalClaims { get; set; } = default!;

    /// <summary>
    /// Configures claim workflow relationships, delete behavior, and lookup indexes.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MedicalClaim>()
            .HasIndex(claim => claim.ClaimNumber)
            .IsUnique();

        modelBuilder.Entity<MedicalClaim>()
            .HasOne(claim => claim.Batch)
            .WithMany(batch => batch.MedicalClaims)
            .HasForeignKey(claim => claim.BatchId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MedicalClaim>()
            .HasOne(claim => claim.Hospital)
            .WithMany(hospital => hospital.MedicalClaims)
            .HasForeignKey(claim => claim.HospitalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MedicalClaim>()
            .HasOne(claim => claim.InsuranceCompany)
            .WithMany(company => company.MedicalClaims)
            .HasForeignKey(claim => claim.InsuranceCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClaimRecommendation>()
            .HasOne(recommendation => recommendation.MedicalClaim)
            .WithMany(claim => claim.ClaimRecommendations)
            .HasForeignKey(recommendation => recommendation.MedicalClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClaimRecommendation>()
            .HasIndex(recommendation => new { recommendation.MedicalClaimId, recommendation.GeneratedAt });

        modelBuilder.Entity<ClaimBatch>()
            .HasOne(batch => batch.Hospital)
            .WithMany(hospital => hospital.ClaimBatches)
            .HasForeignKey(batch => batch.HospitalId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ClaimBatch>()
            .HasOne(batch => batch.InsuranceCompany)
            .WithMany(company => company.ClaimBatches)
            .HasForeignKey(batch => batch.InsuranceCompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
