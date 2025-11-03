using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Services;
using PMApplication.Specifications.Filters;
using PMApplication.Extensions;
using PMApplication.Entities.ClusterAggregate;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities;
using static Microsoft.Graph.CoreConstants;
using PlanMatr_API.Extensions;
namespace PlanMatr_API.Controllers.planm
{
    public class CreatePlanApiController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<PartController> _logger;
        private readonly IBrandService _brandService;
        private readonly IClusterService _clusterService;
        private readonly IStandService _standService;
        private readonly IPartService _partService;
        private readonly IProductService _productService;
        private readonly IPlanogramService _planogramService;
        private readonly ICountryService _countryService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public CreatePlanApiController(IMapper mapper, ILogger<PartController> logger, IBrandService brandService, IPartService partService, IProductService productService, IPlanogramService planogramService, ICountryService countryService, IAuditService auditService, IConfiguration config, IWebHostEnvironment env, IStandService standService, IClusterService clusterService)
        {
            _mapper = mapper;
            _logger = logger;
            _brandService = brandService;
            _partService = partService;
            _productService = productService;
            _planogramService = planogramService;
            _countryService = countryService;
            _auditService = auditService;
            _config = config;
            _env = env;
            _standService = standService;
            _clusterService = clusterService;
        }

        #region API V2

        /// <summary>
        /// Gets Stands that are published and have clusters using the new branded standtypes
        /// </summary>
        /// <param name="brandId">The Id of the brand</param>
        /// <param name="countryID">The Id of the country</param>
        /// <param name="standTypeId">The Id of the standType</param>
        /// <returns></returns>

        [Route("api/v2/stand/getBrandedWithClusters/{brandId}/{countryCode}/{standTypeId}")]
        [HttpGet]
        public async Task<IActionResult> GetBrandStandsWithClustersSecure(int brandId, int countryCode, int standTypeId)
        {
            var country = await _countryService.GetCountry(countryCode);
            //var stands = _standService.GetBrandStandsWithClustersForCountry(brandId, country.CountryId, standTypeId).ToSelectListItems(-1);
            var standFilter = new StandFilter();

            standFilter.BrandId = brandId;
            standFilter.CountryId = country.Id;
            standFilter.StandTypeId = standTypeId;

            var standList = await _standService.GetStands(standFilter);
            if (countryCode != 0)
            {
                standList = standList.Where(s => s.Countries.Any()).ToList();
            }

            var stands = standList;
            return Ok(stands.ToSelectListItems(-1));

        }

        [Route("api/v2/clusters/get/{brandId}/{standId}")]
        [HttpGet]
        public async Task<IEnumerable<PlanmClusterDto>> GetClusters(int brandId, int standId)
        {
            var clusterFilter = new ClusterFilter();
            clusterFilter.StandId = standId;
            clusterFilter.BrandId = brandId;
            var clusters = await _clusterService.GetClusters(clusterFilter);
            List<PlanmClusterDto> clusterList = new List<PlanmClusterDto>();
            foreach (Cluster cluster in clusters)
            {
                PlanmClusterDto pm = new PlanmClusterDto(); //plano's and clusters are very similar.

                _mapper.Map(cluster, pm);
                pm.Id = cluster.Id;
                pm.standName = cluster.Stand.Name;
                pm.standType = cluster.Stand.StandType.Name;
                pm.standWidth = cluster.Stand.Width;
                pm.standHeight = cluster.Stand.Height;
                clusterList.Add(pm);
            }

            return clusterList;

        }

        //[Authorize]
        [Route("api/v2/planogram/create/{clusterId}/{planoName}/{brandId}")]
        [HttpGet]
        public async Task<IActionResult> CreatePlanogram(long clusterId, string planoName, int brandId)
        {
            try
            {
                var userProfile = await this.MappedUser();

                string? userId = userProfile.Id;

                var filter = new ClusterFilter
                {
                    Id = clusterId,
                };

                var planogramId = await _planogramService.CreatePlanogramFromCluster(filter, planoName, userProfile, brandId);


                var planogram = await _planogramService.GetPlanogram(planogramId);
                if (planogram.ScratchPad == null)
                {
                    //we need to create a new scratchpad
                    ScratchPad sPad = new ScratchPad();
                    sPad.DateCreated = DateTime.Now;
                    sPad.DateUpdated = DateTime.Now;
                    _planogramService.CreateScratchPad(sPad);
                    planogram.ScratchPad = sPad;
                    _planogramService.SavePlanogram(planogram);
                }

                //Audit the action
                var audit = new AuditLog
                {
                    UserId = userId,
                    Date = DateTime.Now,
                    BrandId = brandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.CreatePlano,
                    Message = userProfile.DisplayName + " created planogram " + planogram.Name,
                    PlanoId = planogramId
                };
                _auditService.AuditEvent(audit);

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
