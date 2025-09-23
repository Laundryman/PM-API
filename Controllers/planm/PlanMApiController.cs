using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using System.Net;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PlanMatr_API.Exceptions;
using PMApplication.Entities.PlanogramAggregate;
using PlanMatr_API.Extensions;
using PMApplication.Enums;
using PMApplication.Services;
using PMApplication.Specifications.Filters;

namespace PlanMatr_API.Controllers.planm
{
    [Route("api/countries/[action]")]
    [ApiController]
    public class PlanMApiController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<PartController> _logger;
        private readonly IAIdentityService _identityService;
        private readonly IBrandService _brandService;
        private readonly IPartService _partService;
        private readonly IProductService _productService;
        private readonly IPlanogramService _planogramService;
        private readonly ICountryService _countryService;
        private readonly ILmAuditService _auditService;
        private readonly IConfiguration _config;
        //private readonly ICategoryService _categoryService;
        //private readonly IAIdentityService _identityService;
        //private readonly IProductService _productService;
        private readonly IStandService _standService;

        public PlanMApiController(IPartService partService,
                //ICategoryService categoryService,
                IStandService standService,
                IBrandService brandService,
                //ICountryService countryService,
                //IProductService productService,
                IPlanogramService planogramService,
                //IAIdentityService identityService, 
                IMapper mapper, ILogger<PartController> logger, IAIdentityService identityService, ICountryService countryService, ILmAuditService auditService, IConfiguration config, IProductService productService)
            //IPlanogramVersionService versionService)
        {
            this._partService = partService;
            this._standService = standService;
            //this._categoryService = categoryService;
            //this._productService = productService;
            //this._countryService = countryService;
            this._brandService = brandService;
            this._planogramService = planogramService;
            //this._identityService = identityService;
            _mapper = mapper;
            _logger = logger;
            _identityService = identityService;
            _countryService = countryService;
            _auditService = auditService;
            _config = config;
            _productService = productService;
            //this._versionService = versionService;
        }


        [Route("api/v2/planx/get-menu/{planogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetMenu(int planogramId)
        {
            var planogram = await _planogramService.GetPlanogram(planogramId);
            var standTypeId = planogram.Stand.StandTypeId;
            var brandId = planogram.Stand.BrandId;
            var countryId = planogram.CountryId ?? 0;

            var menu = new MenuDto();
            try
            {


                var menuParts = await _partService.GetPlanxMenu(brandId, countryId, standTypeId);

                return Ok(menuParts);
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

                message.ReasonPhrase = "Error creating menu";
                //log an error
                _logger.LogError("Error getting menu for planogram - " + planogram.Id + " error message:  " +
                                 ex.Message);

                return StatusCode(500, "Internal server error getting menu");
            }
            finally
            {

            }

}


        [Route("api/v2/planx/get-planogram/{PlanogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetPlanogram(int PlanogramId)
        {
            try
            {
                var planogram = await _planogramService.GetPlanogram(PlanogramId);
                if (planogram.ScratchPad == null)
                {
                    //we need to create a new scratchpad
                    ScratchPad sPad = new ScratchPad();
                    sPad.DateCreated = DateTime.Now;
                    sPad.DateUpdated = DateTime.Now;
                    _planogramService.CreateScratchPad(sPad);
                    planogram.ScratchPad = sPad;
                    _planogramService.SavePlanogram();
                }
                //var planogramView = (PlanogramDTO)planogram;
                var planogramView = _mapper.Map<PlanogramDto>(planogram);
                return Ok(planogramView);
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
                message.ReasonPhrase = "Error getting planogram";
                //log an error
                _logger.LogError("Error getting planogram for planogramId " + PlanogramId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);

                return StatusCode(500, "Internal server error getting planogram");
            }
        }


        [Route("api/v2/planx/get-planogram-scratchpad/{planogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetPlanogramScratchPad(int planogramId)
        {
            try
            {
                var plano = await _planogramService.GetPlanogram(planogramId);


                var planoCountryId = plano.CountryId;



                var planoParts = _mapper.Map<List<PartInfoDto>>(plano.ScratchPad.PlanogramParts);
                var planoShelves = _mapper.Map<List<PartInfoDto>>(plano.ScratchPad.PlanogramShelves);
                var allScratch = planoParts.Concat(planoShelves);

                var scratchParts = allScratch.ToList();
                foreach (PartInfoDto prt in scratchParts)
                {
                    if (!prt.CountryIds.Contains((int)planoCountryId))
                    {
                        prt.NonMarket = true;
                    }
                }

                return Ok(scratchParts);
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
                message.ReasonPhrase = "Error retrieving scratchpad";
                //log an error

                _logger.LogError("Error getting scratchpad for planogramId " + planogramId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error getting scratchpad");
            }

        }

        [Route("api/v2/planx/get-stand/{standId}")]
        [HttpGet]
        public async Task<IActionResult> GetStand(int standId)
        {
            //var stand = new PlanXStandViewModel();
            try
            {
                var stand = await _standService.GetStand(standId, true);
                var brand = await _brandService.GetBrand(stand.BrandId);
                var standView = _mapper.Map<PlanmStandDto>(stand);

                if (brand.ShelfLock)
                {
                    standView.ShelfLock = true;
                }

                var standType = await _standService.GetStandType(standView.StandTypeId);
                standView.StandTypeName = standType.Name;
                standView.ParentStandTypeName = standType.ParentStandType.Name;
                return Ok(standView);
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
                message.ReasonPhrase = "Error creating menu";
                //log an error

                _logger.LogError("Error getting stand for standId " + standId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error getting stand");
            }

        }

        [Route("api/v2/planx/save-planogram")]
        [HttpPost]
        public async Task<IActionResult> SavePlanogram(PlanmPlanogramInfo planogramData)
        {

            try
            {
                //get the planogramID
                var planogramId = planogramData.PlanogramId;
                var planogram = await _planogramService.GetPlanogram(planogramId);
                var planogramParts = planogram.PlanogramParts.ToList();

                var userProfile = await this.MappedUser(_identityService);
                //var userProfile = new UserViewModel();
                //userProfile.Id = planogramData.UserId;
                //userProfile.UserName = planogramData.UserName;
                //userProfile.Roles = planogramData.UserRoles;
                //userProfile.DiamCountryId = planogramData.CountryId;

                var currPlano = await _planogramService.GetPlanogram(planogramId);

                var planoIsLocked = IsLocked(planogramId, userProfile);

                if (planoIsLocked)
                {
                    HttpResponseMessage messg = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    messg.Content = new StringContent("This planogram is currently locked");
                    messg.ReasonPhrase = "User not authorised";
                    //log an error

                    return BadRequest(messg);
                }



                var planoCountryId = planogramData.CountryId;
                var planoCountry = _countryService.GetCountry(planoCountryId);
                //var userBrands = this.MappedBrands(userProfile, _brandService);
                //TODO should we always be making plano country = to use country whatever happens?
                if (currPlano.StatusId == (int)StatusEnums.PlanogramStatusEnum.Archived)
                {
                    if (currPlano.CountryId != null)
                    {
                        planoCountry = _countryService.GetCountry(currPlano.CountryId ?? 0);
                    }
                }

                var brand = _brandService.GetBrand(currPlano.Stand.BrandId);

                //////////////////////////////////////////////////////////////////
                //Finish user checks
                //////////////////////////////////////////////////////////////////

                planogram.DateUpdated = DateTime.Now;
                planogram.LastUpdatedBy = userProfile.Id;
                planogram.LubName = userProfile.GivenName + " " + userProfile.Surname;
                planogram.Name = planogramData.PlanogramName;
                planogram.CurrentVersion += 1;

                if (planogram.StatusId != (int)StatusEnums.PlanogramStatusEnum.Approved &&
                    planogram.StatusId != (int)StatusEnums.PlanogramStatusEnum.Archived &&
                    planogram.StatusId != (int)StatusEnums.PlanogramStatusEnum.Validated &&
                    planogram.StatusId != (int)StatusEnums.PlanogramStatusEnum.Deleted &&
                    planogram.StatusId != (int)StatusEnums.PlanogramStatusEnum.Submitted)
                {
                    planogram.StatusId = (int)StatusEnums.PlanogramStatusEnum.Edit;
                    PlanogramStatus status =
                        _planogramService.GetPlanogramStatus((int)StatusEnums.PlanogramStatusEnum.Edit);
                    planogram.Status = status;
                }
                _planogramService.SavePlanogram();
                //Audit the action
                var audit = new LMAuditLog
                {
                    UserId = userProfile.Id,
                    Date = DateTime.Now,
                    BrandId = planogram.Stand.BrandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.EditPlano,
                    Message = userProfile.DisplayName + " edited planogram with Id " + planogramId.ToString(),
                    PlanoId = (int)planogramId
                };

                _auditService.AuditEvent(audit);


                //Handle Deletions now
                if (planogramData.DeletedInfo.partInfos != null)
                {
                    DeletePlanogramParts(planogramData.DeletedInfo.partInfos.ToList());
                }

                if (planogramData.DeletedInfo.shelfInfos != null)
                {
                    DeletePlanogramShelves(planogramData.DeletedInfo.shelfInfos.ToList());
                }

                //Handle the scratchpad now
                UpdateScratchPad(planogramData.PlanogramId, planogramData.ScratchPadInfo);


                //Handle the planogram now
                var shelves = planogramData.PlanogramInfo.shelfInfos;
                if (shelves != null)
                {
                    foreach (var shelf in shelves)
                    {
                        var planogramShelf = await SaveShelf(shelf);
                        if (planogramShelf != null)
                        {
                            if (shelf.Parts != null)
                            {
                                foreach (var part in shelf.Parts)
                                {
                                    if (part.PlanogramShelfId == 0)
                                    {
                                        part.PlanogramShelfId = planogramShelf.Id;
                                    }
                                }
                            }

                            SaveCassettes(planogramShelf.PlanogramId, shelf.Parts.ToList());
                        }
                    }
                }

                //Now handle any parts not associated with shelves
                SaveCassettes(planogramData.PlanogramId, planogramData.cassetteInfo.ToList());

                return Ok();
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

                message.ReasonPhrase = "Error saving cassettes";
                //log an error
                _logger.LogError("Error saving planogram " + planogramData.PlanogramId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Error saving planogram");
            }

            finally
            {

            }
        }


        #region PlanogramFunctions



        /// <summary>
        /// Checks a planogram isn't locked
        /// </summary>
        /// <param name="planogramId">The id of the planogram to check.</param>
        /// <returns>true or false.</returns>
        private bool IsLocked(int planogramId, CurrentUser user)
        {
            return _planogramService.IsLocked(planogramId, user);
        }

        private async Task SaveCassettes(long planogramId, List<PlanmPartInfo> cassettes, ScratchPad scratchPad = null)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////
            //HANDLE CASSETTES
            //now handle the cassettes available products

            //bool debugSave = ConfigurationManager.AppSettings["debugSave"] == "True" ? true : false;
            bool debugSave = _config["AppSettings:DebugSave"] == "True" ? true : false;
            bool throwError = false;
            var cassetteCounter = 0;
            foreach (var cassette in cassettes)
            {

                if (cassettes.Count > 3)
                {

                    if (cassette.PlanogramPartId != 0 && debugSave && cassetteCounter > 3)
                        throwError = true;
                }

                cassetteCounter++;
                switch (cassette.PartTypeId)
                {
                    case (int)PartTypeEnum.Cassette:
                        await PlanogramCassetteUpdate(cassette, planogramId, throwError);
                        break;
                    case (int)PartTypeEnum.Glorifier:
                        await PlanogramCassetteUpdate(cassette, planogramId, throwError);
                        break;
                    case (int)PartTypeEnum.RedFrame:
                        await PlanogramCassetteUpdate(cassette, planogramId, throwError);
                        break;
                    case (int)PartTypeEnum.Blanking:
                        await PlanogramCassetteUpdate(cassette, planogramId, throwError);
                        break;
                    case (int)PartTypeEnum.FasciaPlate:
                        await PlanogramCassetteUpdate(cassette, planogramId, throwError);
                        break;
                    case (int)PartTypeEnum.Accessory:
                        await PlanogramCassetteUpdate(cassette, planogramId, throwError);
                        break;
                }

            }
        }

        private async Task PlanogramCassetteUpdate(PlanmPartInfo planoPart, long planogramId, bool throwError) //, planogramId, planogramToSave, newShelf)
        {

            try
            {
                Part part = null;

                int planogramPartId = planoPart.PlanogramPartId;
                int partId = planoPart.PartId;
                var planogram = await _planogramService.GetPlanogram(planogramId);

                if (planogram.ScratchPad == null)
                {
                    //we need to create a new scratchpad
                    ScratchPad sPad = new ScratchPad();
                    sPad.DateCreated = DateTime.Now;
                    sPad.DateUpdated = DateTime.Now;
                    _planogramService.CreateScratchPad(sPad);
                    planogram.ScratchPad = sPad;
                    _planogramService.SavePlanogram();
                }

                var scratchPadId = planogram.ScratchPadId;
                // part status
                int planogramPartStatusId = planoPart.StatusId ?? 0;


                if (partId != 0)
                {
                    part = _partService.GetPart(partId);
                }
                else
                {
                    if (!CassetteHasDuplicate(planoPart, planogram))
                    {
                        part = _partService.GetPart(planoPart.PartNumber);
                    }
                    else
                    {
                        throw new DuplicatePartException();
                    }
                }

                if (part != null)
                {

                    PlanogramPart newPart = new PlanogramPart();
                    if (planogramPartId != 0)
                    {
                        newPart = _planogramService.GetPlanogramPart(planogramPartId);
                        _planogramService.SavePlanogramPart();
                    }

                    if (newPart == null)
                    {
                        newPart = new PlanogramPart();
                        planogramPartId = 0;
                    }
                    else
                    {
                        newPart.ScratchPadId = null; //need this to fix issue with restoring from scratchpad
                    }

                    newPart.PlanogramId = planogramId;
                    newPart.PlanogramShelfId = planoPart.PlanogramShelfId == 0 ? null : planoPart.PlanogramShelfId;
                    //this bit basically sets whether the part is in the scratch pad or not.
                    newPart.ScratchPadId = planoPart.ScratchPadId == 0 ? null : planoPart.ScratchPadId;
                    newPart.PositionX = planoPart.Position.x;
                    newPart.PositionY = planoPart.Position.y;
                    newPart.Notes = planoPart.Notes;
                    newPart.Label = planoPart.Label;
                    newPart.Part = part;

                    newPart.PartStatusId = planogramPartStatusId;



                    if (planogramPartId != 0)
                    {
                        if (throwError)
                            throw new Exception("Debug Save Error Thrown");

                        newPart.DateUpdated = DateTime.Now;
                        _planogramService.SavePlanogramPart();
                    }
                    else
                    {
                        if (throwError)
                            throw new Exception("Debug Save Error Thrown");

                        newPart.DateCreated = DateTime.Now;
                        _planogramService.CreatePlanogramPart(newPart);
                    } //ERROR Part_CatPartId not exist



                    //now handle the products (selected products and facings and stock)

                    ////////////////////////////////////////////////////////////////////////////////
                    //  CASSETTE ITEM LOOP
                    ///////////////////////////////////////////////////////////////////////////////

                    var selectedFacings = planoPart.facingProducts;
                    if (selectedFacings != null)
                    {
                        //Assume that the nodes are in the correct order for the facing position, counting 1 to n from the left.
                        int FacingPosition = 1;
                        foreach (var facingItem in selectedFacings)
                        {
                            if (newPart.PlanogramPartFacings != null && newPart.PlanogramPartFacings.Count > 0)
                            {
                                var currentFacing = newPart.PlanogramPartFacings
                                    .Where(pf => pf.Position == FacingPosition).FirstOrDefault();
                                if (currentFacing != null)
                                {
                                    if (facingItem.ProductId != 0)
                                    {
                                        FacingPosition = await InsertFacing(facingItem, planogramId, part.Stock, newPart,
                                            FacingPosition);
                                    }
                                    else
                                    {
                                        //we need to remove this productFacing info
                                        //we need to delete the item in this position
                                        _planogramService.DeletePlanogramPartFacing(currentFacing.Id);
                                        FacingPosition++;
                                    }

                                }
                                else
                                {
                                    if (facingItem.ProductId != 0)
                                    {
                                        FacingPosition = await InsertFacing(facingItem, planogramId, part.Stock, newPart,
                                            FacingPosition);
                                    }
                                    else
                                    {
                                        FacingPosition++;
                                    }
                                    //must be a new facing
                                }
                            }
                            else
                            {
                                if (facingItem.ProductId != 0)
                                {
                                    FacingPosition = await InsertFacing(facingItem, planogramId, part.Stock, newPart,
                                        FacingPosition);
                                }
                                else
                                {
                                    FacingPosition++;
                                }
                            }
                        } //end of cassetteProduct loop               

                    } //end of if selectedProducts is null


                } //end if part is null

            }
            catch (DuplicatePartException ex)
            {
                string message = String.Format("Duplicate found with cassette id {0} with partId {1}, partName {2}", planoPart.PartId, planoPart.PartId, planoPart.Name);
                Exception newException = new Exception(message, ex);
                string exceptionString = newException.ToString(); // full stack trace
                _logger.LogError(message);

                return;

            }
            catch (Exception ex)
            {
                string message = String.Format("An error occurred updating the cassette id {0} with partId {1}", planoPart.PartId, planoPart.PartId);
                Exception newException = new Exception(message, ex);
                string exceptionString = newException.ToString(); // full stack trace
                _logger.LogError(message);
                throw newException;
            }

        }

        private async Task<int> InsertFacing(PlanmPartFacing cassetteProductFacing, long planogramId, int stockCount,
    PlanogramPart newPart, int facingPosition)
        {

            //if (cassetteProductFacing.ProductId == 0)
            //{
            //    facingPosition++;
            //    return facingPosition;
            //}
            var partFacingId = cassetteProductFacing.Id;
            //////////////////////////////////////////////////////////////////////////////
            // facing status ////
            //////////////////////////////////////////////////////////////////////////////
            int planogramFacingStatusId = cassetteProductFacing.FacingStatus ?? 0;

            //////////////////////////////////////////////////////////////////////////////
            // END of facing status ////
            /////////////////////////////////////////////////////////////////////////////

            //we make the assumption that there is a one to one relationship between facing - product - shade
            PlanogramPartFacing currentFacing = new PlanogramPartFacing();
            if (partFacingId == 0)
            {
                currentFacing.PlanogramId = planogramId;
                currentFacing.PlanogramPart = newPart;
                //currentFacing.Id = 0;
            }
            else
            {
                currentFacing = _planogramService.GetPlanogramPartFacing(partFacingId);
            }

            if (currentFacing != null)
            {
                currentFacing.Position = facingPosition;
                currentFacing.ProductId = cassetteProductFacing.ProductId;
                currentFacing.FacingStatusId = planogramFacingStatusId;
                if (cassetteProductFacing.ShadeId != null)
                    currentFacing.Shade = await _productService.GetShade(cassetteProductFacing.ShadeId ?? 0);
                currentFacing.StockCount = stockCount;

                var filter = new PlanogramPartFilter();
                filter.PartId = newPart.Part.Id;
                var partFactices = await _planogramService.GetPlanogramParts(filter);

            }

            //REMOVE ANY EXISTING ITEM IN THE POSITION - FACINGS
            if (newPart.PlanogramPartFacings != null)
            {
                if (newPart.PlanogramPartFacings.Any(pf => pf.Position == facingPosition))
                {
                    //we need to delete the item in this position
                    PlanogramPartFacing pfToDelete = newPart.PlanogramPartFacings.FirstOrDefault(pf => pf.Position == facingPosition);
                    _planogramService.DeletePlanogramPartFacing(pfToDelete.Id);
                }
                _planogramService.CreatePlanogramPartFacing(currentFacing);
            }
            else
            {
                if (partFacingId == 0)
                {
                    _planogramService.CreatePlanogramPartFacing(currentFacing);
                }
                else
                {
                    _planogramService.SavePlanogramPartFacing();
                }
            }

            facingPosition++;
            return facingPosition;
        }

        private async Task<PlanogramShelf> SaveShelf(PlanmShelfInfo shelf, ScratchPad scratchPad = null)
        {

            try
            {
                var newShelf = new PlanogramShelf();
                long? shelfId = shelf.Id; //the planogramShelfId
                var planogram = await _planogramService.GetPlanogram(shelf.PlanogramId);

                if (shelfId.Value != 0)
                {
                    newShelf = _planogramService.GetPlanogramShelf((int)shelfId);
                }
                else
                {
                    {
                        if (ShelfHasDuplicate(shelf, planogram))
                        {
                            throw new DuplicatePartException();
                        }
                    }
                }


                if (newShelf == null)
                {
                    newShelf = new PlanogramShelf();
                    shelfId = 0;
                }



                newShelf.PlanogramId = shelf.PlanogramId;
                if (scratchPad != null)
                {
                    //then we are dealing with the scratchpad
                    newShelf.ScratchPadId = scratchPad.Id;
                }
                else
                {
                    newShelf.ScratchPadId = null;
                }

                newShelf.ShelfTypeId = shelf.ShelfTypeId;
                newShelf.Height = (short)shelf.Height;
                newShelf.Width = (short)shelf.Width;
                newShelf.PositionX = shelf.Position.x;
                newShelf.PositionY = shelf.Position.y;
                newShelf.Part = _partService.GetPart(shelf.PartId);
                newShelf.PartStatusId = shelf.StatusId ?? 0;
                var label = shelf.Label;
                if (label != null)
                    newShelf.Label = label;

                if (shelfId != null)
                {
                    if (shelfId != 0)
                    {
                        _planogramService.SavePlanogramShelf();
                    }
                    else //shelfId == 0
                    {
                        _planogramService.CreatePlanogramShelf(newShelf);
                    }

                }
                else //no shelfId attribute
                {
                    _planogramService.CreatePlanogramShelf(newShelf);
                }

                return newShelf;
            }
            catch (DuplicatePartException ex)
            {
                string message = String.Format("Duplicate found with shelf id {0} with partId {1}, and with label {2}", shelf.PlanxShelfId, shelf.PartId, shelf.Label);
                DuplicatePartException newException = new DuplicatePartException(message, ex);
                _logger.LogError(message);

                return null;

            }
            catch (Exception ex)
            {
                string message = String.Format("An error occurred updating the shelf id {0} with partId {1}", shelf.PlanxShelfId, shelf.PartId);
                Exception newException = new Exception(message, ex);
                string exceptionString = newException.ToString(); // full stack trace
                _logger.LogError(message);
                throw newException;
            }
        }
        private void DeletePlanogramParts(List<PlanmPartInfo> parts)
        {
            foreach (var delItem in parts)
            {
                var ppart = _planogramService.GetPlanogramPart(delItem.PlanogramPartId);
                if (ppart != null) //part hasn't already been deleted
                {
                    List<PlanogramPartFacing> idsToDelete = new List<PlanogramPartFacing>();
                    foreach (PlanogramPartFacing facing in ppart.PlanogramPartFacings)
                    {
                        idsToDelete.Add(facing);
                    }
                    foreach (var facing in idsToDelete)
                    {
                        _planogramService.DeletePlanogramPartFacing(facing.Id);
                    }
                    _planogramService.DeletePlanogramPart(delItem.PlanogramPartId);
                }
            }
        }

        private void DeletePlanogramShelves(List<PlanmShelfInfo> shelves)
        {
            foreach (var delItem in shelves)
            {
                if (delItem.Id != 0)
                {
                    _planogramService.DeletePlanogramShelf(delItem.Id);
                }
            }
        }

        private async Task UpdateScratchPad(int planogramId, PlanmShelfInfoList scratchPad)
        {
            //Handle the planogram now
            var shelves = scratchPad.shelfInfos;
            var parts = scratchPad.partInfos;
            int? sPadId = 0;
            if (parts.Any() || shelves.Any())
            {
                if (parts.Any())
                {
                    sPadId = parts.FirstOrDefault().ScratchPadId;
                }
                else
                {
                    if (shelves.Any())
                    {
                        sPadId = shelves.FirstOrDefault().ScratchPadId;
                    }
                }

                int scratchPadId = sPadId == null ? 0 : (int)sPadId;
                var spad = _planogramService.GetScratchPad(scratchPadId);

                if (shelves != null)
                {
                    foreach (var shelf in shelves)
                    {
                        var planogramShelf = await SaveShelf(shelf, spad);
                        if (planogramShelf != null)
                        {
                            if (shelf.Parts != null)
                            {
                                foreach (var part in shelf.Parts)
                                {
                                    if (part.PlanogramShelfId == 0)
                                    {
                                        part.PlanogramShelfId = planogramShelf.Id;
                                    }
                                }

                                SaveCassettes(planogramShelf.PlanogramId, shelf.Parts.ToList());
                            }
                        }
                    }
                }

                //save parts
                if (parts != null)
                {
                    SaveCassettes(planogramId, parts.ToList());
                }
            }
        }

        private bool ShelfHasDuplicate(PlanmShelfInfo shelf, Planogram planogram)
        {
            PlanogramShelf? foundShelf = new PlanogramShelf();
            //check if this has been duplicated
            foundShelf = planogram.PlanogramShelves.FirstOrDefault(sf =>
                sf.PositionX == shelf.Position.x && (sf.PositionY == shelf.Position.y) && sf.Part.Id == shelf.PartId);
            if (foundShelf != null)
            {
                _logger.LogError("Duplicate Found - planogramId = " + planogram.Id + " --- partId = " + shelf.PartId);

            }
            return foundShelf != null;
        }

        private bool CassetteHasDuplicate(PlanmPartInfo part, Planogram planogram)
        {
            PlanogramPart? foundPart = new PlanogramPart();
            //check if this has been duplicated
            foundPart = planogram.PlanogramParts.FirstOrDefault(pp =>
                pp.PositionX == part.Position.x && (pp.PositionY == part.Position.y) && pp.Part.Id == part.PartId);
            if (foundPart != null)
            {
                _logger.LogError("Duplicate Found - planogramId = " + planogram.Id + " --- partId = " + part.PartId);

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion PlanogramFunctions


    }


}
