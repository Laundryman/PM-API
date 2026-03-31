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

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(options =>
//        {
//            builder.Configuration.Bind("AzureAdB2C", options);

//            options.TokenValidationParameters.NameClaimType = "name";
//        },
//        options => { builder.Configuration.Bind("AzureAdB2C", options); });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureEntraId"));
// End of the Microsoft Identity platform block    
builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey =
        "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODA2MTA1NjAwIiwiaWF0IjoiMTc3NDYyMzY1NyIsImFjY291bnRfaWQiOiIwMTlkMmZjZWMyZGM3YTQ3OGRjMzA5MzJjYWJmNzRlYSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa21xeDAzMXBwNWRrazFjdm1ncHY5cngxIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.vDmCEAP0Rf063Pd_NyoDtGHvhrWAZ7UVnKtCMI0nPqw8qADxInJ4S_iNDiMBZ6jKda3SWdiBJrJ-UkNUztNmtO5uuGvkMOJ2JGkT-foyP8yCnKysV6OzGnGlt3gNf4e27Vh0bXetgvoxzC7EJoUHfPE63PUbi33UMuMrjhtPVi9kQG9Gg7SBrg09yHBum99sPY8TkHSq5e5Hstb_dVxzv9vdQERW-wY6nTHNR_EK2hT_VO_x1HiQEKfeNnkU9G_33_FzWMPMcktGCMQMB9CLOmku4oVStMz_Pt3zjBTalvQvmX5pZxxz5ka9zkVpizSlnNA9t0X5Elh4GAOHiJ7FNA";
    cfg.AddMaps(typeof(Program).Assembly);
});

//builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped(typeof(IAsyncRepositoryLong<>), typeof(EfRepositoryLong<>));
builder.Services.AddPMServices();
builder.Services.AddRepositories();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", builder =>
    {
        builder.WithOrigins("https://localhost:44321", "https://brand.demo.planmatr.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Demo"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowClientApp");
app.UseAuthentication();
app.UseAuthorization();
//app.UseRouting();

app.MapControllers();

app.Run();