using PMApplication.Entities.JobsAggregate;
using PMApplication.Helpers;
using PMApplication.Interfaces.ServiceInterfaces;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using PlanMatr_API.Extensions;
using PMApplication.Dtos;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using AutoMapper;
using PlanMatr_API.Controllers.planm;

namespace PlanMatr_API.Controllers
{
    public class JobsApiController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IBrandService _brandService;
        private readonly IRegionService _regionService;
        private readonly IJobService _jobService;
        private readonly IJobFolderService _jobFolderService;

        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EditPlanApiController> _logger;




        public JobsApiController(ICountryService countryService, IBrandService brandService, IRegionService regionService, IJobService jobService, IJobFolderService jobFolderService, ICountryRepository countryRepository, IMapper mapper, ILogger<EditPlanApiController> logger)
        {
            _countryService = countryService;
            _brandService = brandService;
            _regionService = regionService;
            _jobService = jobService;
            _jobFolderService = jobFolderService;
            _countryRepository = countryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        #region Jobs

        [Route("api/v2/jobNumbersForFolder/get/{jobFolderId}")]
        [HttpGet]

        public async Task<IActionResult> GetJobNumbersForFolder(int jobFolderId)
        {
            try
            {
                var jobs = await _jobService.GetJobFolderJobs(jobFolderId);
                return Ok(jobs);
            }
            catch (Exception Ex)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);

                _logger.LogError("Error getting job numbers");

                return BadRequest(Ex.Message);
            }

        }


        #endregion

        #region JobFolders

        [Route("api/v2/jobFolders/get/{brandId}")]
        [HttpGet]

        public async Task<IActionResult> GetJobFolders(int brandId)
        {
            try
            {
                var filter = new JobFolderFilter
                {
                    BrandId = brandId,
                    HasJobs = true
                };
                var jobFolders = await _jobFolderService.GetJobFolders(filter);
                    var jobFolderDto = jobFolders.Select(j => _mapper.Map<JobFolderDto>(j));
                //return Json(products, JsonRequestBehavior.AllowGet);
                return Ok(jobFolderDto);
            }
            catch (Exception Ex)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);

                // Get stack trace for the exception with source file information
                //var st = new StackTrace(ex, true);
                // Get the top stack frame
                //var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                //var line = frame.GetFileLineNumber();

                if (Ex.InnerException != null)
                {
                    message.Content = new StringContent(Ex.Message +
                                                        Ex.InnerException.ToString());
                }
                else
                {
                    message.Content = new StringContent(Ex.Message
                                                        + Ex.StackTrace);
                }
                message.ReasonPhrase = "Error get JobFolders";
                _logger.LogError("Error getting jobFolders -- " + message);

                return BadRequest("Error getting job folders");
            }


        }



        [Route("api/v2/jobFolders/get/{brandId}/{countryId}/{regionId}")]
        [HttpGet]

        public async Task<IActionResult> GetJobFolders(int brandId, int countryId, int regionId)
        {
            var userProfile = await this.MappedUser();
            var getCountrySpec = new GetCountrySpec(userProfile.DiamCountryId);
            var countries = await _countryRepository.ListAsync(getCountrySpec);
            var userCountry = countries.FirstOrDefault();
            var userDefaultRegion = userCountry.Regions.First(r => r.BrandId == brandId);
            var userRoleIds = userProfile.RoleIds;
            IReadOnlyList<JobFolderInfo> jobFolders;

            int userId = userProfile.DiamUserId;
            try
            {

                if (RolesHelper.IsAdminUser(userRoleIds))
                {
                    var filter = new JobFolderFilter
                    {
                        BrandId = brandId,
                        CountryId = countryId,
                        RegionId = regionId
                    };
                    jobFolders = await _jobFolderService.GetJobFolderInfos(filter);

                }
                else if (RolesHelper.IsClientValidator(userRoleIds))
                {
                    var filter = new JobFolderFilter
                    {
                        BrandId = brandId,
                        CountryId = countryId,
                        RegionId = userDefaultRegion.Id
                    };
                    jobFolders = await _jobFolderService.GetJobFolderInfos(filter);
                }
                else
                {
                    var filter = new JobFolderFilter
                    {
                        BrandId = brandId,
                        CountryId = userCountry.Id,
                        RegionId = userDefaultRegion.Id
                    };
                    jobFolders = await _jobFolderService.GetJobFolderInfos(filter);

                }
                return Ok(jobFolders);
            }
            catch (Exception Ex)
            {
                //log an error

                _logger.LogError("Error getting job folders -- " + Ex.Message);

                return BadRequest("Error getting job folders");
            }
        }


        #endregion

    }
}
