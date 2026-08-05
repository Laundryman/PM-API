using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;

namespace PlanMatr_API.Mappings
{
    public class PlanogramProfile : Profile
    {


        public PlanogramProfile()
        {
            CreateMap<Planogram, PlanmPlanogramDto>()
                .ForMember(p => p.StatusName, opt => opt.MapFrom<PlanogramStatusEnumResolver>());
        }
    }
}