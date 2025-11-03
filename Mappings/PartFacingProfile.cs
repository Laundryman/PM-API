using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;

namespace PlanMatr_API.Mappings
{
    public class PartFacingProfile : Profile
    {


        public PartFacingProfile()
        {
            CreateMap<PlanogramPartFacing, PlanmPartFacing>()
                .ForMember(p => p.FacingNo, opt => opt.MapFrom(f => f.Position))
                .ForMember(p => p.PlanogramFacingId, opt => opt.MapFrom(f => f.Id))
                .ForMember(p => p.ProductId, opt => opt.MapFrom(f => f.ProductId))
                .ForMember(p => p.PartId, opt => opt.MapFrom(f => f.PlanogramPart.PartId))
                .ForMember(p => p.ShadeId, opt => opt.MapFrom(f => f.ShadeId))
                .ForMember(p => p.FacingStatus, opt => opt.MapFrom(f => f.FacingStatusId));

        }
    }
}
