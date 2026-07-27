using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using PlanMatr_API.Controllers.planm;
using PlanMatr_API.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Enums;
using PMApplication.Helpers;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Services;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;
using System.Net;
using System.Text.Json;
using static PMApplication.Enums.StatusEnums;

namespace PlanMatr_API.Controllers
{
    //[Authorize]
    [Route("api/planograms/[action]")]
    public class ManagePlanogramsApiController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ILogger<EditPlanApiController> _logger;
        private readonly IBrandService _brandService;
        private readonly IPartService _partService;
        private readonly IProductService _productService;
        private readonly IPlanogramService _planogramService;
        private readonly IPlanogramRepository _planogramRepository;
        private readonly IAsyncRepositoryLong<Planogram> _planogramAsyncRepository;

        private readonly ICountryService _countryService;
        private readonly IRegionService _regionService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public ManagePlanogramsApiController(IMapper mapper, ILogger<EditPlanApiController> logger, IBrandService brandService, IPartService partService, IProductService productService, IPlanogramService planogramService, ICountryService countryService, IAuditService auditService, IConfiguration config, IWebHostEnvironment env, IRegionService regionService, IPlanogramRepository planogramRepository, IAsyncRepositoryLong<Planogram> planogramAsyncRepository)
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
            _regionService = regionService;
            _planogramRepository = planogramRepository;
            _planogramAsyncRepository = planogramAsyncRepository;
        }

        
        //[Route("api/v2/planx/get-planogram-preview/{planogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetPlanogramPreview(int planogramId)
        {
            try
            {
                var preview = await _planogramService.GetPlanogramPreview(planogramId);
                if (preview != null)
                {
                    return Ok(preview.PreviewSrc);
                }
                else
                {
                    return Ok(string.Empty);
                    
                } 


            }
            catch (Exception Ex)
            {
                //log an error

                _logger.LogError("Error getting image - " + Ex.Message + " -- stack trace is:  " + Ex.StackTrace);
                return BadRequest("Error getting image");

            }
        }

        //[Route("api/v2/planogram/rename/{planogramId}/{planoName}")]
        [HttpGet]
        public async Task<int> RenamePlanogram(int planogramId, string planoName)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            var brandId = planogram.Stand.BrandId;

            planogram.Name = planoName;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = brandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile?.DisplayName + " renamed planogram with Id " + planogramId.ToString() + " to " + planoName,
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/submit/{planogramId}")]
        [HttpGet]
        public async Task<int> SubmitPlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Submitted;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile?.DisplayName + " submitted planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/delete/{planogramId}")]
        [HttpGet]
        public async Task<int> DeletePlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Deleted;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile?.DisplayName + " deleted planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/approve/{planogramId}")]
        [HttpGet]
        public async Task<int> ApprovePlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Approved;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile?.DisplayName + " approved planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/validate/{planogramId}")]
        [HttpGet]
        public async Task<int> ValidatePlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Validated;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile?.DisplayName + " validated planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/reject/{planogramId}")]
        [HttpGet]
        public async Task<int> RejectPlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Edit;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile?.DisplayName + " rejected planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/getCommentCount/{planogramId}/{brandId}")]
        [HttpGet]
        public async Task<IActionResult> GetCommentCount(int planogramId, int brandId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();

            var countryId = userProfile.CountryId;
            try
            {
                //We're not using the country and region here: but we need to think about how we might regarding users.
                var noteFilter = new NoteFilter
                {
                    UserId = userProfile.Id,
                    BrandId = brandId,
                    CountryId = countryId,
                    PlanogramId = planogramId
                };
                var planogramNotes = await _planogramService.GetPlanogramNotes(noteFilter);
                var commentCount = planogramNotes.Count();
                return Ok(commentCount);
            }
            catch (Exception ex)
            {
                var message = "";
                if (ex.InnerException != null)
                {
                    message = new string(ex.Message +
                                                        ex.InnerException.ToString());
                }
                else
                {
                    message = new string(ex.Message
                                         + ex.StackTrace);
                }
                //message.ReasonPhrase = "Error retrieving comment count";
                //log an error
                _logger.LogError("Error retrieving comment count -- " + message);

                return BadRequest("Error retrieving comment count");
            }
            finally
            {

            }


        }

        
        //[Route("api/v2/planogram/get/jsonskulist/{planogramId}")]
        [HttpGet]

        public async Task<IActionResult> GetSkuList(int planogramId)
        {

            try
            {
                var filter = new PlanogramFilter
                {
                    Id = planogramId,
                    LoadRelatedEntities = true

                };

                var spec = new PlanogramSpecification(filter);

                var planogram = await _planogramAsyncRepository.FirstOrDefaultAsync(spec);


                var hasColumns = planogram.Stand.ColumnList.Count != 0;
                var skuList = await _planogramService.GetSkuList(planogram.Id, planogram.UserId, hasColumns);
                var currentSkuPart = 0;
                foreach (var sku in skuList)
                {
                    var skuPart = sku.PlanogramPartsId;
                    if (skuPart == currentSkuPart)
                    {
                        //set values to null to prevent over counting in spreadsheet
                        sku.Facings = null;
                        sku.Stock = null;
                        sku.TotalSKU = null;
                        sku.UnitCost = null;
                    }
                    //set current sku part if skupart has changed
                    if (currentSkuPart != skuPart)
                    {
                        currentSkuPart = skuPart ?? 0;
                    }
                }
                var exportSku = _mapper.Map<List<ExportSkuDto>>(skuList);
                //var exportSkuJson = JsonSerializer.Serialize(exportSku);

                return Ok(exportSku);

            }
            catch (Exception Ex)
            {
                var message = "";

                if (Ex.InnerException != null)
                {
                    message = new string(Ex.Message +
                                                    Ex.InnerException.Data.ToString());
                }
                else
                {
                    message = new string(Ex.Message
                                    + Ex.StackTrace);
                }
                _logger.LogError(message);

                return Ok("Error creating Sku list " + Ex.Message);
            }

        }
        [HttpPost]
        public async Task<IActionResult> SearchPlanograms([FromBody]PlanogramFilterDto filterDto)
        {
            try
            {
                var userProfile = await this.MappedUser();
                var userRegions = userProfile.RegionList.Split(",").Select(int.Parse).ToList();
                var userCountries = userProfile.CountryList.Split(",").Select(int.Parse).ToList();
                var regions = await _regionService.GetRegions(new RegionFilter { idList = userProfile.RegionList, BrandId = filterDto.BrandId ?? 0 });
                var filterCountryList = new List<int>();
                foreach (var region in regions)
                {
                    var regionCountryList = region.CountryList.Split(",").Select(int.Parse).ToList();
                    for (int i = 0; i < regionCountryList.Count; i++)
                    {
                        if (userCountries.Contains(regionCountryList[i]))
                        {
                            filterCountryList.Add(regionCountryList[i]);
                        }
                    }
                }
                filterDto.CountriesList = string.Join(",", filterCountryList);
                var planograms = await _planogramRepository.SearchPlanograms(filterDto);

                return Ok(planograms);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchProducts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deprecated - 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="countryId"></param>
        /// <param name="regionId"></param>
        /// <param name="standTypeId"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        //[Route("api/v2/planogram/get/yourplanograms/{status}/{countryId}/{regionId}/{standTypeId}/{brandId}")]
        //[HttpGet]
        //[Obsolete("This method is deprecated, please use SearchPlanograms instead.")]
        //public async Task<IActionResult> GetYourPlanograms(int status, int countryId, int regionId, int standTypeId, int brandId)
        //{
        //    IEnumerable<PlanogramInfo> planograms;

        //    try
        //    {
        //        // we can retrieve the userId from the request
        //        var userProfile = await this.MappedUser();
        //        var statusEnum = (PlanogramStatusEnum)status;
        //        string? userId = userProfile?.Id;

        //        //if (RolesHelper.IsAdministrator(int.Parse(userProfile.RoleId)))
        //        //{
        //        //    planograms = await _planogramService.GetYourPlanograms((int)statusEnum, countryId, regionId, standTypeId, brandId);
        //        //}

        //        planograms = await _planogramService.GetYourPlanograms((int)statusEnum, countryId, regionId, standTypeId, brandId);
        //        if (!RolesHelper.IsAdministrator(int.Parse(userProfile.RoleId)) && countryId == 0)
        //        {
        //            //filter planograms for allowed countries for non admin users
        //                int[] userCountries = userProfile.CountryList.Split(",").Select(int.Parse).ToArray();
        //            int[] userRegions = userProfile.RegionList.Split(",").Select(int.Parse).ToArray();
        //            var regions = await _regionService.GetRegions(new RegionFilter { idList = userProfile.RegionList, BrandId = brandId});
        //            IEnumerable<int> brandCountryList = new List<int>();
        //            foreach (var region in regions)
        //            {
        //                foreach (int cid in userCountries)
        //                {
        //                    int[] regionCountryList = region.CountryList.Split(",").Select(int.Parse).ToArray();
        //                    if (regionCountryList.Contains(cid))
        //                    {
        //                        // Do something if the region contains the country
        //                        brandCountryList = brandCountryList.Append(cid);
        //                    }
        //                }
        //            }

        //            var userPlanograms = planograms.Where(p => brandCountryList.Contains(p.CountryId ?? 0));
        //                return Ok(userPlanograms);
        //        }

        //        //else if (RolesHelper.IsValidator(userProfile.RoleIds))
        //        //{

        //        //    planograms = await _planogramService.GetYourPlanograms((int)statusEnum, userProfile.CountryId, regionId, standTypeId, brandId);
        //        //}

        //        //else if (RolesHelper.IsApprover(userProfile.RoleIds))
        //        //{
        //        //    planograms = await _planogramService.GetYourPlanograms((int)statusEnum, brandId, countryId, regionId, standTypeId);
        //        //}
        //        //else
        //        //{
        //        //    planograms = await _planogramService.GetYourPlanograms((int)(int)statusEnum, userProfile.CountryId, 0, standTypeId, brandId);
        //        //}


        //        return Ok(planograms);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error getting planograms - message = {0}", ex.Message);
        //        return StatusCode(ex.HResult, ex.Message);
        //    }

        //}


        //[Route("api/v2/planogram/get/archived/job/{isPowerUser}/{jobId}/{jobCode}/{brandId}/{countryId}/{regionId}/{standTypeId}/{isDiamUser}")]
        //[HttpGet]
        //public async Task<IEnumerable<PlanogramInfo>> GetArchivedPlanogramsByJob(int isPowerUser, int jobId, string jobCode, int brandId, int countryId, int regionId, int standTypeId, int isDiamUser)
        //{

        //    // we can retrieve the userId from the request
        //    try
        //    {
        //        // we can retrieve the userId from the request
        //        var userProfile = await this.MappedUser();

        //        //var userProfile = await this.MappedUser(_identityService);
        //        string userId = String.Empty; //userProfile.Id;
        //        var brand = await _brandService.GetBrand(brandId);
        //        var userBrands = this.MappedBrands(userProfile, _brandService);

        //        if (userBrands.Contains(brand))
        //        {
        //            var hostUrl = Request.Scheme + "://" + Request.Host + "/user_uploads/planograms/";    //.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/user_uploads/planograms/";

        //            var planograms = await _planogramService.GetArchivedPlanograms(userId, jobId, brandId, countryId, regionId, standTypeId, isDiamUser == 1, hostUrl);
        //            return planograms;

        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}


        /// <summary>
        /// Update to clone planogram will take account of IsUpdate to copy notes and update colours.
        /// </summary>
        /// <param name="planogramId"></param>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <param name="IsUpdate"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ClonePlanogram([FromBody] ClonePlanogramDto clonePlanogramDto)
        {
            try
            {
                //Planogram originalPlanogram = await _planogramRepository.GetByIdAsync(clonePlanogramDto.Id);

                //Planogram newPlanogram = new Planogram();


                var userProfile = await this.MappedUser();
                var userRegions = userProfile.RegionList.Split(",").Select(int.Parse).ToList();
                var userCountries = userProfile.CountryList.Split(",").Select(int.Parse).ToList();
                var regions = await _regionService.GetRegions(new RegionFilter { idList = userProfile.RegionList, BrandId = clonePlanogramDto.BrandId });
                var filterCountryList = new List<int>();

                var clonedPlanogramId = await _planogramService.ClonePlanogram(clonePlanogramDto.Id, clonePlanogramDto.Name, userProfile, clonePlanogramDto.IsUpdate ?? false);


                return Ok(clonedPlanogramId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside ClonePlanogram action: {ex.Message}");
                return StatusCode(500, "Internal server error");

            }
        }

        [HttpGet]
        public async Task<IActionResult> LockPlanogram(long planogramId = 0)
        {
            //var planoRepoService = dpRes.GetService<IPlanogramRepository>();
            Planogram planogram = await _planogramService.GetPlanogram(planogramId);
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;



            try
            {
                var filter = new PlanogramLockFilter
                {
                    PlanogramId = planogramId,
                    User = userProfile
                };
                var isLocked = await _planogramService.IsLocked(filter);
                if (!isLocked)
                {
                    //lock the planogram Now
                    return Ok("success");
                }
                else
                {
                    //it's already locked by someone else.
                    return Conflict("fail");
                }
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.Message);
            }

        }

        //[Route("api/v2/planx/get-plano-lock/{planogramId}/{userId}/{userName}")]
        [HttpGet]
        public async Task<IActionResult> GetPlanoLock(int planogramId)
        {
            try
            {
                var userProfile = await this.MappedUser();
                var filter = new PlanogramLockFilter
                {
                    PlanogramId = planogramId,
                    User = userProfile
                };
                var isLocked = await _planogramService.IsLocked(filter);
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                if (!isLocked)
                {
                    message = new HttpResponseMessage(HttpStatusCode.OK);
                    await _planogramService.LockPlanogram(filter);
                    return Ok("unlocked");
                }
                return Ok("locked");
            }
            catch (Exception ex)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);

                if (ex.InnerException != null)
                {
                    message.Content = new StringContent(ex.Message +
                                                        ex.InnerException.ToString());
                }
                else
                {
                    message.Content = new StringContent(ex.Message
                                                        + ex.StackTrace);
                }
                message.ReasonPhrase = "Error getting lock";
                //log an error
                _logger.LogError("Error getting lock - " + ex.Message + " -- stack trace is:  " + ex.StackTrace);

                return BadRequest("Error getting lock info");
            }
            finally
            {

            }
        }


    }
}
