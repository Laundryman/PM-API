using PlanMatr_API.CurrentUser;
using System.Security.Claims;


namespace PlanMatr_API.Extensions
{
        public static class CurrentUserExtensions
    {
        /// <summary>
        /// Obsolete use controller extension MappedUser instead
        ///        public static class CurrentUserExtensions
        /// </summary>
        public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app)
            {
            //https://fiseni.com/posts/current-user-aspnetcore/
            //https://jasonwatmore.com/post/2023/01/20/net-7-create-a-base-controller-in-net
            app.Use(async (context, next) =>
                {
                    var user = context.User;
                    var currentUser = context.RequestServices.GetRequiredService<ICurrentUserInitializer>();

                    var countryId = user.Claims.Where(c => c.Type == "extension_diamCountryId")
                        .Select(c => c.Value).FirstOrDefault();

                    currentUser.Id ??= user.FindFirstValue(ClaimTypes.NameIdentifier);
                    currentUser.BrandIds ??= user.Claims.Where(c => c.Type == "extension_brands").Select(c => c.Value).FirstOrDefault();
                    if (countryId != null && currentUser.DiamCountryId == null)
                    {
                        currentUser.DiamCountryId = int.Parse(countryId);
                    }

                    await next();
                });

                return app;
            }
        }
}
