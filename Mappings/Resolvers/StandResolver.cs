using AutoMapper;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.StandAggregate;

namespace PlanMatr_API.Mappings.Resolvers
{
    public class StandCountResolver : IValueResolver<StandType, StandTypeDto, int>
    {
        public int Resolve(StandType source, StandTypeDto destination, int destMember, ResolutionContext context)
        {
            if (source.Stands != null)
            {
                // Map the PartStatusId to a status string or enum as needed
                
                return source.Stands.Count;
            }
            else
            {
                return 0;
            }
        }
    }

    public class StandLayoutResolver : IValueResolver<Stand, PlanmStandDto, byte?>
    {
        public byte? Resolve(Stand source, PlanmStandDto destination, byte? destMember, ResolutionContext context)
        {
            if (source.LayoutStyle == 0)
            {
                // Map the PartStatusId to a status string or enum as needed

                return 1;
            }
            else
            {
                return (byte)source.LayoutStyle;
            }
        }
    }

    public class StandUpdateLayoutResolver : IValueResolver<StandUpdateDto, Stand, byte>
    {
        public byte Resolve(StandUpdateDto source, Stand destination, byte destMember, ResolutionContext context)
        {
            if (source.LayoutStyle == 0)
            {
                // Map the PartStatusId to a status string or enum as needed

                return 1;
            }
            else
            {
                return (byte)source.LayoutStyle;
            }
        }
    }

    public class StandRowResolver : IValueResolver<Stand, PlanmStandDto, IEnumerable<PlanmStandRowDto>>
    {
        public IEnumerable<PlanmStandRowDto> Resolve(Stand source, PlanmStandDto destination, IEnumerable<PlanmStandRowDto> destMember, ResolutionContext context)
        {
            var planmList = new List<PlanmStandRowDto>();

            if (source.EqualRows && source.Rows > 0)
            {
                for (int i = 0; i < source.Rows; i++)
                {
                    var newRow = new PlanmStandRowDto
                    {
                        Height = (int)source.DefaultRowHeight,
                        Id = i,
                        Position = i,
                        StandId = source.Id
                    };
                    planmList.Add(newRow);
                }

            }
            else
            {
                foreach (var row in source.RowList)
                {
                    var newRow = new PlanmStandRowDto
                    {
                        Height = row.Height,
                        Id = row.Id,
                        Position = row.Position,
                        StandId = row.StandId
                    };
                    planmList.Add(newRow);
                }
            }
            return planmList;
        }
    }
}
