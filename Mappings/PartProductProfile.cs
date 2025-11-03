using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;

namespace PlanMatr_API.Mappings
{
    public class PartProductProfile : Profile
    {


        public PartProductProfile()
        {
            CreateMap<PartProduct, ProductDto>()
                .ForMember(p => p.Id, opt => opt.MapFrom(s => s.Product.Id))
                .ForMember(p => p.BrandId, opt => opt.MapFrom(s => s.Product.BrandId))
                .ForMember(p => p.CategoryId, opt => opt.MapFrom(s => s.Product.CategoryId))
                .ForMember(p => p.Name, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(p => p.ShortDescription, opt => opt.MapFrom(s => s.Product.ShortDescription))
                .ForMember(p => p.FullDescription, opt => opt.MapFrom(s => s.Product.FullDescription))
                .ForMember(p => p.DateCreated, opt => opt.MapFrom(s => s.Product.DateCreated))
                .ForMember(p => p.DateUpdated, opt => opt.MapFrom(s => s.Product.DateUpdated))
                .ForMember(p => p.DateAvailable, opt => opt.MapFrom(s => s.Product.DateAvailable))
                .ForMember(p => p.Published, opt => opt.MapFrom(s => s.Product.Published))
                .ForMember(p => p.ProductImage, opt => opt.MapFrom(s => s.Product.ProductImage))
                .ForMember(p => p.Discontinued, opt => opt.MapFrom(s => s.Product.Discontinued))
                .ForMember(p => p.Hero, opt => opt.MapFrom(s => s.Product.Hero))
                .ForMember(p => p.CountryList, opt => opt.MapFrom(s => s.Product.CountryList))
                .ForMember(p => p.ParentCategoryId, opt => opt.MapFrom(s => s.Product.ParentCategoryId))
                .ForMember(p => p.Shades, opt => opt.MapFrom(s => s.Product.Shades));

        }
    }
}
