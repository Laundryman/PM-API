using PlanMatr_API.Controllers;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMInfrastructure.Repositories;
using PMApplication.Services;
using PMInfrastructure.Data;

namespace PlanMatr_API.Extensions
{
    public static class PMServiceExtenstions
    {
        public static IServiceCollection AddPMServices(this IServiceCollection services)
        {
            // Register your application services here
            //Example: services.AddScoped<IYourService, YourServiceImplementation>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddScoped<IClusterService, ClusterService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IJobService, JobService>();
            services.AddTransient<IJobFolderService, JobFolderService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderWindowService, OrderWindowService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IPartService, PartService>();
            services.AddScoped<IPlanogramService, PlanogramService>();
            services.AddTransient<IPartService, PartService>();
            services.AddTransient<IPlanogramService, PlanogramService>();
            services.AddScoped<IRegionService, RegionService>();
            services.AddScoped<IStandService, StandService>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<PlanMatrContext>();
            services.AddTransient<IAuditRepository, AuditRepository>();
            services.AddTransient<IBrandRepository, BrandRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IClusterRepository, ClusterRepository>();
            //services.AddTransient<IClusterShelfRepository, ClusterShelfRepository>();
            //services.AddTransient<IClusterPartRepository, ClusterPartRepository>();
            services.AddTransient<ICountryRepository, CountryRepository>();
            //services.AddTransient<IEmailRepository, EmailRepository>();
            //services.AddTransient<IHeroProductRepository, HeroProductRepository>();
            services.AddTransient<IJobRepository, JobRepository>();
            services.AddTransient<IJobFolderRepository, JobFolderRepository>();
            services.AddTransient<IOrderPlanogramRepository, OrderPlanogramRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IOrderItemRepository, OrderItemRepository>();
            services.AddTransient<IOrderWindowRepository, OrderWindowRepository>();
            services.AddTransient<IPartRepository, PartRepository>();
            services.AddTransient<IPartTypeRepository, PartTypeRepository>();
            services.AddTransient<IPlanogramRepository, PlanogramRepository>();
            services.AddTransient<IPlanogramLockRepository, PlanogramLockRepository>();
            services.AddTransient<IPlanogramNoteRepository, PlanogramNoteRepository>();
            services.AddTransient<IPlanogramPartRepository, PlanogramPartRepository>();
            //services.AddTransient<IPlanogramPartFacingRepository, PlanogramPartFacingRepository>();
            services.AddTransient<IPlanogramShelfRepository, PlanogramShelfRepository>();
            //services.AddTransient<IPlanogramStatusRepository, PlanogramStatusRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IRegionRepository, RegionRepository>();
            services.AddTransient<IStandRepository, StandRepository>();
            //services.AddTransient<IStandColumnRepository, StandColumnRepository>();
            //services.AddTransient<IStandColumnUprightRepository, StandColumnUprightRepository>();
            //services.AddTransient<IStandRowRepository, StandRowRepository>();
            services.AddTransient<IStandTypeRepository, StandTypeRepository>();
            services.AddTransient<IShadeRepository, ShadeRepository>();
            services.AddTransient<IScratchPadRepository, ScratchPadRepository>();
            services.AddTransient<IStandService, StandService>();

            return services;

        }
    }
}
