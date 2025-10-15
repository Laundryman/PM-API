﻿using Microsoft.AspNetCore.Mvc;
using PlanMatr_API.Extensions;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Interfaces.ServiceInterfaces;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using PlanMatr_API.Controllers.planm;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Helpers;
using PMApplication.Services;
using PMApplication.Specifications.Filters;
using static PMApplication.Enums.StatusEnums;

namespace PlanMatr_API.Controllers
{
    public class ManagePlanogramsApiController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ILogger<EditPlanApiController> _logger;
        private readonly IBrandService _brandService;
        private readonly IPartService _partService;
        private readonly IProductService _productService;
        private readonly IPlanogramService _planogramService;
        private readonly ICountryService _countryService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public ManagePlanogramsApiController(IMapper mapper, ILogger<EditPlanApiController> logger, IBrandService brandService, IPartService partService, IProductService productService, IPlanogramService planogramService, ICountryService countryService, IAuditService auditService, IConfiguration config, IWebHostEnvironment env)
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
        }

        [Authorize]
        [Route("api/v2/planogram/lock/{planogramId}")]
        [HttpGet]

        public async Task<IActionResult> LockPlanogram(long planogramId = 0)
        {
            //var planoRepoService = dpRes.GetService<IPlanogramRepository>();
            Planogram planogram = await _planogramService.GetPlanogram(planogramId);
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string userId = userProfile.Id;



            try
            {
                var isLocked = _planogramService.IsLocked(planogramId, userProfile);
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


        [Authorize]
        [Route("api/v2/planogram/rename/{planogramId}/{planoName}")]
        [HttpGet]
        public async Task<int> RenamePlanogram(int planogramId, string planoName)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string userId = userProfile.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            var brandId = planogram.Stand.BrandId;

            planogram.Name = planoName;
            _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = brandId,
                Roles = userProfile.RoleIds,
                UserName = userProfile.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile.DisplayName + " renamed planogram with Id " + planogramId.ToString() + " to " + planoName,
                PlanoId = planogramId
            };
            _auditService.AuditEvent(audit);

            return planogramId;

        }

        [Authorize]
        [Route("api/v2/planogram/getCommentCount/{planogramId}/{brandId}")]
        [HttpGet]
        public async Task<IActionResult> GetCommentCount(int planogramId, int brandId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();

            var countryId = userProfile.DiamCountryId;
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

        [Authorize]
        [Route("api/v2/planogram/get/jsonskulist/{planogramId}")]
        [HttpGet]

        public async Task<IActionResult> GetJsonSkuList(int planogramId)
        {

            try
            {
                //SystemLog.DebugFormat("GetJsonSkuList with planogramId " + planogramId.ToString());
                Planogram planogram = await _planogramService.GetPlanogram(planogramId);

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


        [Authorize]
        [Route("api/v2/planogram/get/yourplanograms/{status}/{countryId}/{regionId}/{standTypeId}/{brandId}")]
        [HttpGet]
        public async Task<IEnumerable<PlanogramInfo>> GetYourPlanograms(int status, int countryId, int regionId, int standTypeId, int brandId)
        {
            IEnumerable<PlanogramInfo> planograms;

            try
            {
                // we can retrieve the userId from the request
                var userProfile = await this.MappedUser();
                var statusEnum = (PlanogramStatusEnum)status;
                string userId = userProfile.Id;

                if (RolesHelper.IsAdministrator(userProfile.RoleIds))
                {
                    planograms = await _planogramService.GetYourPlanograms((int)statusEnum, countryId, regionId, standTypeId, brandId);
                }

                else if (RolesHelper.IsValidator(userProfile.RoleIds))
                {

                    planograms = await _planogramService.GetYourPlanograms((int)statusEnum, userProfile.DiamCountryId, regionId, standTypeId, brandId);
                }

                else if (RolesHelper.IsApprover(userProfile.RoleIds))
                {
                    planograms = await _planogramService.GetYourPlanograms((int)statusEnum, brandId, countryId, regionId, standTypeId);
                }
                else
                {
                    planograms = await _planogramService.GetYourPlanograms((int)(int)statusEnum, userProfile.DiamCountryId, 0, standTypeId, brandId);
                }


                return planograms;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting planograms - message = {0}", ex.Message);
                throw;
            }

        }


    }
}
