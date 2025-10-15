using Microsoft.AspNetCore.Mvc;
using PMApplication.Entities;
using PMApplication.Interfaces.ServiceInterfaces;
using System.Security.Claims;
using PMApplication.Dtos;

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
            //var me = _identityService.GetMe(Globals.B2cExtensionAppId);
            var identity = ((System.Security.Claims.ClaimsPrincipal)currentUser);
            var user = new CurrentUser()
            {
                BrandIds = identity.Claims.Where(c => c.Type == "extension_brands").Select(c => c.Value)
                    .FirstOrDefault(),
                DiamCountryId = int.Parse(identity.Claims.Where(c => c.Type == "extension_diamCountryId")
                    .Select(c => c.Value).FirstOrDefault()),
                DisplayName = identity.Claims.Where(c => c.Type == "name").Select(c => c.Value).FirstOrDefault(),
                UserName = identity.Claims.Where(c => c.Type == "name").Select(c => c.Value).FirstOrDefault(),
                Email = identity.Claims.Where(c => c.Type == "extension_userEmailAddress").Select(c => c.Value)
                    .FirstOrDefault(),
                Id = identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).FirstOrDefault(),
                Surname = identity.Claims.Where(c => c.Type == ClaimTypes.Surname).Select(c => c.Value).FirstOrDefault(),
                GivenName = identity.Claims.Where(c => c.Type == ClaimTypes.GivenName).Select(c => c.Value).FirstOrDefault(),
                RoleIds = identity.Claims.Where(c => c.Type == "extension_diamRoles").Select(c => c.Value)
                    .FirstOrDefault(),
            };
            //var userOID = identity.Claims.FirstOrDefault((x => x.Type == ClaimTypes.NameIdentifier)).Value;// 'http://schemas.microsoft.com/identity/claims/objectidentifier']
            //var user = await identityService.GetUser(userOID, Globals.B2cExtensionAppId);
            //var mapper = MapperConfig.InitializeAutomapper();
            //var mappedUser = mapper.Map<User, UserViewModel>(user);
            return Task.FromResult(user);
        }
    }


}
