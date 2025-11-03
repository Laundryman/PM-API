using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;

namespace PlanMatr_API.Mappings
{
    public class PlanogramPartProfile : Profile
    {


        public PlanogramPartProfile()
        {
            CreateMap<PlanogramPart, PlanmPartInfo>()
                .ForMember(pm => pm.Position, opt => opt.MapFrom<PartPositionResolver>())
                .ForMember(pm => pm.PlanogramPartId, opt => opt.MapFrom(src => src.Id))
                .ForMember(pm => pm.PlanogramShelfId, opt => opt.MapFrom(src => src.PlanogramShelfId))
                .ForMember(pm => pm.PlanogramId, opt => opt.MapFrom(src => src.PlanogramId))
                .ForMember(pm => pm.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(pm => pm.Name, opt => opt.MapFrom(src => src.Part.Name))
                .ForMember(pm => pm.PartType, opt => opt.MapFrom(src => src.Part.PartType.Name))
                .ForMember(pm => pm.PartTypeId, opt => opt.MapFrom(src => src.Part.PartTypeId))

                .ForMember(pm => pm.Facings, opt => opt.MapFrom(src => src.Part.Facings))
                .ForMember(pm => pm.PartNumber, opt => opt.MapFrom(src => src.Part.PartNumber))
                .ForMember(pm => pm.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(pm => pm.Label, opt => opt.MapFrom(src => src.Label))

                //.ForMember(pm => pm.Notes, opt => opt.MapFrom(string.Empty))
                .ForMember(pm => pm.ManufacturingProcess, opt => opt.MapFrom(src => src.Part.ManufacturingProcess))
                .ForMember(pm => pm.Presentation, opt => opt.MapFrom(src => src.Part.Presentation))
                .ForMember(pm => pm.TestingType, opt => opt.MapFrom(src => src.Part.TestingType))
                .ForMember(pm => pm.Stock, opt => opt.MapFrom(src => src.Part.Stock))
                .ForMember(pm => pm.Height, opt => opt.MapFrom(src => src.Part.Height))
                .ForMember(pm => pm.Width, opt => opt.MapFrom(src => src.Part.Width))
                .ForMember(pm => pm.UnitCost, opt => opt.MapFrom(src => src.Part.UnitCost))
                .ForMember(pm => pm.Description, opt => opt.MapFrom(src => src.Part.Description))
                .ForMember(pm => pm.PackShotImageSrc, opt => opt.MapFrom(src => src.Part.PackShotImageSrc))
                .ForMember(pm => pm.Render2dImage, opt => opt.MapFrom(src => src.Part.Render2dImage))
                .ForMember(pm => pm.LaunchPrice, opt => opt.MapFrom(src => src.Part.LaunchPrice))
                .ForMember(pm => pm.LaunchDate, opt => opt.MapFrom(src => src.Part.LaunchDate))
                .ForMember(pm => pm.CassetteBio, opt => opt.MapFrom(src => src.Part.CassetteBio))
                .ForMember(pm => pm.DmiReco, opt => opt.MapFrom(src => src.Part.DmiReco))
                .ForMember(pm => pm.SvgLineGraphic, opt => opt.MapFrom(src => src.Part.SvgLineGraphic))
                .ForMember(pm => pm.CategoryId, opt => opt.MapFrom(src => src.Part.CategoryId))
                .ForMember(pm => pm.Category, opt => opt.MapFrom(src => src.Part.Category.Name))
                .ForMember(pm => pm.ParentCategoryId, opt => opt.MapFrom(src => src.Part.ParentCategoryId))
                .ForMember(pm => pm.Discontinued, opt => opt.MapFrom(src => src.Part.Discontinued))
                .ForMember(pm => pm.StatusId, opt => opt.MapFrom(src => src.PartStatusId))
                .ForMember(pm => pm.PartStatusId, opt => opt.MapFrom(src => src.PartStatusId))
                .ForMember(pm => pm.Status, opt => opt.MapFrom<PartStatusEnumResolver>())
                .ForMember(pm => pm.CountryList,
                    opt => opt.MapFrom(src => src.Part.CountryList))
                .ForMember(pm => pm.ScratchPadId, opt => opt.MapFrom(src => src.ScratchPadId))
                .ForMember(pm => pm.PlanogramPartPlanogramPartsId,
                    opt => opt.MapFrom(src => src.PlanogramPartPlanogramPartsId))
                .ForMember(pm => pm.facingProducts, opt => opt.MapFrom(src => src.PlanogramPartFacings))
                .ForMember(pm => pm.products, opt => opt.MapFrom(src => src.Part.Products))
                .ForMember(pm => pm.ProductList,
                    opt => opt.MapFrom(src => src.Part.ProductList))


                .ForMember(pm => pm.ShelfTypeId, opt => opt.Ignore())
                //.ForMember(pm => pm.facingProducts, opt => opt.Ignore())
                //.ForMember(pm => pm.products, opt => opt.Ignore())
                .ForMember(pm => pm.ClusterId, opt => opt.Ignore())
                //.ForMember(pm => pm.PlanogramPartPlanogramPartsId, opt => opt.Ignore())
                .ForMember(pm => pm.ClusterShelfId, opt => opt.Ignore())
                .ForMember(pm => pm.PlanogramColumnId, opt => opt.Ignore())
                .ForMember(pm => pm.PlanxShelfId, opt => opt.Ignore())
                //.ForMember(pm => pm.PartStatusId, opt => opt.Ignore())
                .ForMember(pm => pm.PlanxPartAllowed, opt => opt.Ignore())
                .ForMember(pm => pm.NonMarket, opt => opt.Ignore());
        }
    }

}
