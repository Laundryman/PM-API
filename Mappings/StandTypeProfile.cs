using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.StandAggregate;

namespace PlanMatr_API.Mappings
{
    public class StandTypeProfile : Profile
    {


        public StandTypeProfile()
        {
            CreateMap<StandType, StandTypeDto>()
                .ForMember(p => p.StandCount, opt => opt.MapFrom<StandCountResolver>());


        }
    }
}
