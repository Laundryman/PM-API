using AutoMapper;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.OrderAggregate;
using PMApplication.Specifications.Filters;

namespace PlanMatr_API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BrandFilterDto, BrandFilter>();
            CreateMap<Brand, BrandDto>();

            CreateMap<CategoryFilterDto, CategoryFilter>();
            CreateMap<Category, CategoryDto>();
            CreateMap<CountriesFilterDto, CountryFilter>();
            CreateMap<Country, CountryDto>();
            //.ForMember(dest => dest.Countries, opt => opt.MapFrom(src =>  src.Countries))
            CreateMap<PartFilterDto, PartFilter>();
            CreateMap<PartFilter, PartFilterDto>();
            CreateMap<Part, PartListDto>();
            CreateMap<Part, PartDto>();
            CreateMap<Planogram, PlanogramDto>();
            CreateMap<Product, ProductDto>();
            CreateMap<Product, ProductListDto>();
            CreateMap<Product, FullProductDto>();
            CreateMap<ProductFilterDto, ProductFilter>();
            CreateMap<RegionsFilterDto, RegionFilter>();
            CreateMap<Region, RegionDto>();
            CreateMap<ShadeFilterDto, ShadeFilter>();
            CreateMap<Shade, ShadeDto>();
            CreateMap<Sku, ExportSkuDto>();
            CreateMap<Order, OrderDto>();
        }
    }
}