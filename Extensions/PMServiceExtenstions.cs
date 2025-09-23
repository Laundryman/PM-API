using PlanMatr_API.Controllers;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMInfrastructure.Repositories;
using PMApplication.Services;

namespace PlanMatr_API.Extensions
{
    public static class PMServiceExtenstions
    {
        public static IServiceCollection AddPMServices(this IServiceCollection services)
        {
        // Register your application services here
        //Example: services.AddScoped<IYourService, YourServiceImplementation>();
            services.AddScoped<IPartRepository, PartRepository>();
            services.AddScoped<IPartService, PartService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<IPlanogramService, PlanogramService>();
            //services.AddScoped<ICategoryService, CategoryService>();
            return services;
        }
    }
}
