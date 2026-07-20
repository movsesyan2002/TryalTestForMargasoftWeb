using AutoMapper;
using TryalTestForMargasoftShared.Lookups;
using TryalTestForMargasoftShared.MedicalClaims;
using TryalTestForMargsoftCore.Models;

namespace TryalTestForMargasoftBusinessLogic.Mappings;

public sealed class MedicalClaimMappingProfile : Profile
{
    /// <summary>
    /// Configures API response mappings for medical claims and claim recommendations.
    /// </summary>
    public MedicalClaimMappingProfile()
    {
        CreateMap<Hospital, HospitalResponse>();
        CreateMap<InsuranceCompany, InsuranceCompanyResponse>();
        CreateMap<ClaimRecommendation, ClaimRecommendationResponse>();

        CreateMap<MedicalClaim, MedicalClaimResponse>()
            .ForMember(response => response.CalculatedValues, options => options.Ignore())
            .ForMember(response => response.LatestRecommendation, options => options.Ignore())
            .ForMember(
                response => response.OutstandingBalance,
                options => options.MapFrom(claim => claim.OutstandingBalance ?? claim.CalculateOutstandingBalance()))
            .ForMember(
                response => response.UnderpaymentAmount,
                options => options.MapFrom(claim => claim.UnderpaymentAmount ?? claim.CalculateUnderpaymentAmount()));
    }
}
