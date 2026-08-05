using AutoMapper;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Enums;
namespace PlanMatr_API.Mappings.Resolvers
{
    public class PlanogramStatusEnumResolver : IValueResolver<Planogram, PlanmPlanogramDto, String>
    {
        public string Resolve(Planogram source, PlanmPlanogramDto destination, string destMember, ResolutionContext context)
        {
            {
                // Map the PartStatusId to a status string or enum as needed
                var statusEnum = (StatusEnums.PlanogramStatusEnum)source.StatusId;
                return statusEnum.ToString();
            }
        }
    }
}
