using Microsoft.EntityFrameworkCore;
using PlanMatr_API.Extensions;
using PMApplication.Interfaces;
using PMInfrastructure.Data;
using PMInfrastructure.Repositories;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAdB2C", options);

            options.TokenValidationParameters.NameClaimType = "name";
        },
        options => { builder.Configuration.Bind("AzureAdB2C", options); });
// End of the Microsoft Identity platform block    

// Add services to the container.
builder.Services.AddDbContext<PlanMatrContext>(options =>
    options.UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped(typeof(IAsyncRepositoryLong<>), typeof(EfRepositoryLong<>));
builder.Services.AddPMServices();

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.Run();