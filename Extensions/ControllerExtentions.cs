using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Entities;
using PMApplication.Interfaces.ServiceInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PlanMatr_API.Extensions
{
    public static class ControllerExtentions
    {
        public static async Task<Brand> CurrentBrand(this Controller controller, IBrandService brandService)
        {
            var brandSelection = controller.Request.Cookies["diamBrandCookie"];

            if (brandSelection == null)
            {
                controller.Response.Redirect("~/");
                return null;
            }
            else
            {
                return await brandService.GetBrand(int.Parse(brandSelection));
            }
        }
        public static Task<CurrentUser> MappedUser(this ControllerBase controller)
        {
            // we can retrieve the userId from the request
            var currentUser = controller.User;
            var idToken = controller.Request.Headers["x-user-info"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValues = tokenHandler.ReadJwtToken(idToken);

            //var me = _identityService.GetMe(Globals.B2cExtensionAppId);
            var identity = ((System.Security.Claims.ClaimsPrincipal)currentUser);
            var user = new CurrentUser();
            user.BrandIds = currentUser.Claims.Where(c => c.Type == "Brands").Select(c => c.Value).FirstOrDefault();
                user.DiamCountryId = int.Parse(tokenValues.Claims.Where(c => c.Type == "DiamCountryId")
                    .Select(c => c.Value)
                    .FirstOrDefault() ?? string.Empty);
            user.DisplayName = tokenValues.Claims.Where(c => c.Type == "name").Select(c => c.Value).FirstOrDefault();
            user.UserName = tokenValues.Claims.Where(c => c.Type == "name").Select(c => c.Value).FirstOrDefault();
            user.Email = tokenValues.Claims.Where(c => c.Type == "UserEmailAddress").Select(c => c.Value).FirstOrDefault();
            user.Id = tokenValues.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).FirstOrDefault();
            user.Surname = tokenValues.Claims.Where(c => c.Type == "surname").Select(c => c.Value).FirstOrDefault();
            user.GivenName = tokenValues.Claims.Where(c => c.Type == "givenname").Select(c => c.Value).FirstOrDefault();
            user.RoleIds = tokenValues.Claims.Where(c => c.Type == "DiamRoles").Select(c => c.Value).FirstOrDefault();
            //var userOID = identity.Claims.FirstOrDefault((x => x.Type == ClaimTypes.NameIdentifier)).Value;// 'http://schemas.microsoft.com/identity/claims/objectidentifier']
            //var user = await identityService.GetUser(userOID, Globals.B2cExtensionAppId);
            //var mapper = MapperConfig.InitializeAutomapper();
            //var mappedUser = mapper.Map<User, UserViewModel>(user);
            return Task.FromResult(user);
        }

        public static List<Brand> MappedBrands(this ControllerBase controller, CurrentUser user, IBrandService brandService)
        {
            var brandIds = user.BrandIds.Split(',');
            var brands = new List<Brand>();
            for (int i = 0; i < brandIds.Length; i++)
            {
                var brand = brandService.GetBrand(int.Parse(brandIds[i])).Result;
                brands.Add(brand);
            }
            return brands;
        }
    }


}
