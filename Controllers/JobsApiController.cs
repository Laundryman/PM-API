using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMatr_API.Controllers.planm;
using PlanMatr_API.Extensions;
using PMApplication.Dtos;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Helpers;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;
using System.Net;

namespace PlanMatr_API.Controllers
{
    [Authorize]
    [Route("api/jobs/[action]")]
    [ApiController]
    public class JobsApiController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IBrandService _brandService;
        private readonly IRegionService _regionService;
        private readonly IJobService _jobService;
        private readonly IJobFolderService _jobFolderService;
        private readonly IJobFolderRepository _jobFolderRepository;

        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EditPlanApiController> _logger;




        public JobsApiController(ICountryService countryService, IBrandService brandService, IRegionService regionService, IJobService jobService, IJobFolderService jobFolderService, IJobFolderRepository jobFolderRepository, ICountryRepository countryRepository, IMapper mapper, ILogger<EditPlanApiController> logger)
        {
            _countryService = countryService;
            _brandService = brandService;
            _regionService = regionService;
            _jobService = jobService;
            _jobFolderService = jobFolderService;
            _jobFolderRepository = jobFolderRepository;
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

        [HttpPost(Name = "JobFolders")]
        public async Task<IActionResult> SearchJobFolders(JobFolderFilter filterDto)
        {
            try
            {
                var spec = new JobFolderSpecification(filterDto);
                var jobFolders = await _jobFolderRepository.ListAsync(spec);

                var jfResponse = _mapper.Map<List<JobFolderDto>>(jobFolders);
                return Ok(jfResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchJobFolders action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        //[Route("api/v2/jobFolders/get/{brandId}/{countryId}/{regionId}")]
        //[HttpGet]

        //public async Task<IActionResult> GetJobFolders(int brandId, int countryId, int regionId)
        //{
        //    var userProfile = await this.MappedUser();
        //    var getCountrySpec = new GetCountrySpec(userProfile.CountryId);
        //    var countries = await _countryRepository.ListAsync(getCountrySpec);
        //    var userCountry = countries.FirstOrDefault();
        //    if (userCountry != null)
        //    {
        //        var userDefaultRegion = userCountry.Regions.First(r => r.BrandId == brandId);
        //    }

        //    //var userRoleIds = userProfile.RoleId;
        //    IReadOnlyList<JobFolderInfo> jobFolders;

        //    //int userId = userProfile.DiamUserId;
        //    try
        //    {

        //        //if (RolesHelper.IsAdminUser(int.Parse(userProfile.RoleId)))
        //        //{
        //            var filter = new JobFolderFilter
        //            {
        //                BrandId = brandId,
        //                CountryId = countryId,
        //                RegionId = regionId
        //            };
        //            jobFolders = await _jobFolderService.GetJobFolderInfos(filter);

        //        //}
        //        //else if (RolesHelper.IsValidator(userProfile.Permissions))
        //        //{
        //        //    var filter = new JobFolderFilter
        //        //    {
        //        //        BrandId = brandId,
        //        //        CountryId = countryId,
        //        //        RegionId = regionId
        //        //    };
        //        //    jobFolders = await _jobFolderService.GetJobFolderInfos(filter);
        //        //}
        //        //else
        //        //{
        //        //    var filter = new JobFolderFilter
        //        //    {
        //        //        BrandId = brandId,
        //        //        CountryId = countryId,
        //        //        RegionId = regionId
        //        //    };
        //        //    jobFolders = await _jobFolderService.GetJobFolderInfos(filter);

        //        //}
        //        return Ok(jobFolders);
        //    }
        //    catch (Exception Ex)
        //    {
        //        //log an error

        //        _logger.LogError("Error getting job folders -- " + Ex.Message);

        //        return BadRequest("Error getting job folders");
        //    }
        //}


        #endregion

    }
}
