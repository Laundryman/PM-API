using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.OrderAggregate;
using PMApplication.Entities.StandAggregate;
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
            CreateMap<JobFolderDto, JobFolder>();
            //.ForMember(dest => dest.Countries, opt => opt.MapFrom(src =>  src.Countries))
            CreateMap<PartFilterDto, PartFilter>();
            CreateMap<PartFilter, PartFilterDto>();
            CreateMap<Part, PartListDto>();
            CreateMap<Part, PartDto>();
            CreateMap<Planogram, PlanmPlanogramDto>();
            CreateMap<PlanmPlanogramDto, Planogram>();

            CreateMap<PlanogramShelf, PartInfoDto>();
            CreateMap<PlanogramShelf, PartInfoDto>();
            CreateMap<PartInfoDto, PlanogramPart>();
            CreateMap<PartInfoDto, PlanogramPart>();

            CreateMap<Product, ProductDto>();
            CreateMap<Product, ProductListDto>();
            CreateMap<Product, FullProductDto>();
            CreateMap<ProductFilterDto, ProductFilter>();
            CreateMap<RegionsFilterDto, RegionFilter>();
            CreateMap<Region, RegionDto>();
            CreateMap<ShadeFilterDto, ShadeFilter>();
            CreateMap<Shade, ShadeDto>();
            CreateMap<PlanmStandDto, Stand>();
            CreateMap<Stand, PlanmStandDto>();
            CreateMap<PlanmStandColumnDto, StandColumn>();
            CreateMap<PlanmStandRowDto, StandRow>();
            CreateMap<StandRow, PlanmStandRowDto>();
            CreateMap<StandColumn,PlanmStandColumnDto>();
            CreateMap<StandRow, PlanmStandRowDto>();
            CreateMap<Sku, ExportSkuDto>();
            CreateMap<Order, OrderDto>();
        }
    }
}