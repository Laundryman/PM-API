using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlanMatr_API.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities;
using PMApplication.Entities.ClusterAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Extensions;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Services;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using static Microsoft.Graph.CoreConstants;
namespace PlanMatr_API.Controllers.planm
{
    [Authorize]
    [Route("api/planograms/create/[action]")]
    [ApiController]
    public class CreatePlanApiController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePlanApiController> _logger;
        private readonly IPlanogramService _planogramService;
        private readonly IAuditService _auditService;
        private readonly IAsyncRepository<StandType> _asyncStandTypeRepository;
        private readonly IAsyncRepository<Stand> _asyncStandRepository;
        private readonly IClusterRepository _clusterRepository;


        public CreatePlanApiController(IMapper mapper, ILogger<CreatePlanApiController> logger, IPlanogramService planogramService, IAuditService auditService, IAsyncRepository<StandType> asyncStandTypeRepository, IAsyncRepository<Stand> asyncStandRepository, IClusterRepository clusterRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _planogramService = planogramService;
            _auditService = auditService;
            _asyncStandTypeRepository = asyncStandTypeRepository;
            _asyncStandRepository = asyncStandRepository;
            _clusterRepository = clusterRepository;
        }

        #region API

        [HttpPost]
        public async Task<IActionResult> GetStands(StandFilterDto filterDto)
        {
            try
            {
                var spec = new StandSpecification(_mapper.Map<StandFilter>(filterDto));
                var stands = await _asyncStandRepository.ListAsync(spec);

                var mappedStands = _mapper.Map<List<StandDto>>(stands);
                return Ok(mappedStands);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetStands action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }   

        [HttpPost]
        public async Task<IActionResult> GetStandTypes(StandTypeFilterDto filterDto)
        {
            try
            {
                var spec = new StandTypeSpecification(_mapper.Map<StandTypeFilter>(filterDto));
                var standTypes = await _asyncStandTypeRepository.ListAsync(spec);

                if (filterDto.GetParents)
                {
                    var mappedPTypes = _mapper.Map<List<ParentStandTypeDto>>(standTypes);
                    return Ok(mappedPTypes);
                }

                var mappedTypes = _mapper.Map<List<StandTypeDto>>(standTypes);
                return Ok(mappedTypes.Where(st => st.StandCount > 0));
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetStandTypes action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetLayouts(LayoutFilterDto filterDto)
        {
            //need to update the cluster table/cluster nomenclature to change the name to layout 
            try
            {
                var spec = new ClusterSpecification(_mapper.Map<ClusterFilter>(filterDto));
                var clusters = await _clusterRepository.ListAsync(spec);

                var mappedTypes = _mapper.Map<List<PlanmClusterDto>>(clusters);
                return Ok(mappedTypes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetLayouts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        //[Route("api/v2/planogram/create/{clusterId}/{planoName}/{brandId}/{countryId}")]
        [HttpPost]
        public async Task<IActionResult> CreatePlanogram(CreatePlanogramDto newPlanogramDetails)
        {
            try
            {
                var userProfile = await this.MappedUser();

                if (newPlanogramDetails.BrandId == 0 || newPlanogramDetails.ClusterId == 0 ||
                    newPlanogramDetails.CountryId == 0 || newPlanogramDetails.RegionId == 0
                    || newPlanogramDetails.StandId == 0 || newPlanogramDetails.StandTypeId == 0)
                {
                    throw new Exception("Planogram information incomplete");
                }
                    

                string? userId = userProfile.Id;

                var filter = new ClusterFilter
                {
                    Id = newPlanogramDetails.ClusterId,
                };

                var planogramId = await _planogramService.CreatePlanogramFromCluster(filter, newPlanogramDetails, userProfile);


                var planogram = await _planogramService.GetPlanogram(planogramId);
                if (planogram.ScratchPad == null)
                {
                    //we need to create a new scratchpad
                    ScratchPad sPad = new ScratchPad();
                    sPad.DateCreated = DateTime.Now;
                    sPad.DateUpdated = DateTime.Now;
                    await _planogramService.CreateScratchPad(sPad);
                    planogram.ScratchPad = sPad;
                    await _planogramService.SavePlanogram(planogram);
                }

                //Audit the action
                var audit = new AuditLog
                {
                    UserId = userId,
                    Date = DateTime.Now,
                    BrandId = newPlanogramDetails.BrandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.CreatePlano,
                    Message = userProfile.DisplayName + " created planogram " + planogram.Name,
                    PlanoId = planogramId
                };
                await _auditService.AuditEvent(audit);

                return Ok(planogramId);
            }
            catch (Exception ex)
            {
                return BadRequest("Could not create Planogram");
            }

        }

        #endregion
    }
}
