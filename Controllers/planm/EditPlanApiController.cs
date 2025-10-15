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
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PlanMatr_API.Exceptions;
using PMApplication.Entities.PlanogramAggregate;
using PlanMatr_API.Extensions;
using PMApplication.Enums;
using PMApplication.Services;
using PMApplication.Specifications.Filters;
using System.Web;
using IronPdf.Engines.Chrome;
using IronPdf.Rendering;

namespace PlanMatr_API.Controllers.planm
{
    [Route("api/countries/[action]")]
    [ApiController]
    public class EditPlanApiController : ControllerBase
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
        //private readonly ICategoryService _categoryService;
        //private readonly IAIdentityService _identityService;
        //private readonly IProductService _productService;
        private readonly IStandService _standService;

        public EditPlanApiController(IPartService partService,
                //ICategoryService categoryService,
                IStandService standService,
                IBrandService brandService,
                //ICountryService countryService,
                //IProductService productService,
                IPlanogramService planogramService,
                //IAIdentityService identityService, 
                IMapper mapper, ILogger<EditPlanApiController> logger, ICountryService countryService, IAuditService auditService, IConfiguration config, IProductService productService, IWebHostEnvironment env)
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
            _countryService = countryService;
            _auditService = auditService;
            _config = config;
            _productService = productService;
            _env = env;
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
                    _planogramService.SavePlanogram(planogram);
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

                var userProfile = await this.MappedUser();
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
                _planogramService.SavePlanogram(planogram);
                //Audit the action
                var audit = new AuditLog
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



        [Route("api/v2/planx/save-planogram-jpeg-image")]
        [HttpPost]
        public async Task<IActionResult> SavePlanogramJPEG(PlanmImageDto planoJpeg)
        {
            try
            {
                Planogram planogram = await _planogramService.GetPlanogram((int)planoJpeg.PlanogramId);
                var userProfile = await this.MappedUser();

                planogram.PlanogramPreviewSrc = planoJpeg.Image;
                _planogramService.SavePlanogram(planogram);
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                var audit = new AuditLog
                {
                    UserId = userProfile.Id,
                    Date = DateTime.Now,
                    BrandId = planogram.Stand.BrandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.EditPlano,
                    Message = userProfile.DisplayName + " edited planogram with Id " + planogram.Id,
                    PlanoId = planogram.Id
                };

                _auditService.AuditEvent(audit);

                return Ok();

            }
            catch (Exception ex)
            {
                //log an error
                _logger.LogError("Error saving planogram jpeg image " + planoJpeg.PlanogramId +
                                 "---- error message - " +
                                 ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error saving jpeg");

            }
        }




        [Route("api/v2/planx/save-planogram-svg-image")]
        [HttpPut]
        public async Task<IActionResult> SavePlanogramSVG(PlanmImageDto planoSvg)
        {
            if (planoSvg != null)
            {
                Planogram planogram = await _planogramService.GetPlanogram((int)planoSvg.PlanogramId);



                bool isDevServer = _config["AppSettings:isDevServer"] == "True" ? true : false;

                try
                {
                    return Ok();
                }
                catch (Exception ex)
                {
                    //log an error
                    _logger.LogError("Error saving planogram svg image " + planoSvg.PlanogramId +
                                     "---- error message - " +
                                     ex.Message + " --- " + ex.StackTrace);
                    return StatusCode(500, "Internal server error saving snapshot");

                }
            }
            else
            {

                _logger.LogError("No Planogram Id Supplied");
                return StatusCode(500, "Internal server error saving snapshot");

            }
        }


        [Route("api/v2/planx/get-planogram-pdf")]
        [HttpPost]
        public async Task<IActionResult> PlanogramToPdf(PlanmImageDto planoSvg)
        {
            //var content = Request.Content.ReadAsStringAsync();

            if (planoSvg != null)
            {



                Planogram planogram = await _planogramService.GetPlanogram((int)planoSvg.PlanogramId);
                // we can retrieve the userId from the request
                var skuCount = 0;
                var shelfCount = 0;
                foreach (var part in planogram.PlanogramParts)
                {
                    if (part.Part.PartTypeId == 4 || part.Part.PartTypeId == 10)
                    {
                        shelfCount += 1;
                    }
                    else
                    {
                        skuCount += (part.Part.Facings * part.Part.Stock);
                    }
                }

                foreach (var shelf in planogram.PlanogramShelves)
                {
                    shelfCount += 1;
                }

                var pageHtmlTop =
                    "<html><head><link rel=\"stylesheet\" href=\"https://use.typekit.net/oov2wcw.css\"><style>html { font-family: century-gothic, sans-serif; font-weight: 400; font-style: normal; } </style></head><body >";

                var pageHtmlBottom = "</body></html>";


                var headerHtml = "<div class=\"header-section\" style=\"width:100%;height:80px;font-size: 16px; \">" +
                "<div class=\"row title-row\" style=\"width:100%\">" +
                  //"<div class=\"user-name\" style=\"width:40%;float:left;\">" + planoSvg.UserName + "</div>" +
                  "<div class=\"plano-name\" style=\"text-align:center;\"><div>" + planogram.Name + "</div><div>" + planogram.Stand.standType.Name + " | SKU " + skuCount + " | SHELVES " + shelfCount + "</div> </div>" +
                "</div>" +
                "<div class=\"row name-row\" style=\"width:100%;display:flex;grid-auto-column:50%;\">" +
                  "<div class=\"view-name\" style=\"width:50%;text-align:left;\"><strong>Planogram</strong> View</div>" +
                  "<div class=\"diam-logo\" style=\"width:50%; text-align:right;\"><img src = \"" + _config["AppSettings:BaseImageDomain"] + "/Content/images/DIAM_pdf_logo.png\" style=\"height:40px;\" /></div>" +
                "</div>" +
              "</div>";


                try
                {

                    // Render any HTML fragment or document to HTML
                    //var Renderer = new IronPdf.HtmlToPdf();
                    ChromePdfRenderer Renderer = new ChromePdfRenderer();
                    Renderer.RenderingOptions.Timeout = 200;
                    Renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() { MaxHeight = 45, Spacing = 25, HtmlFragment = headerHtml, FontSize = 16, LoadStylesAndCSSFromMainHtmlDocument = true };
                    //Renderer.RenderingOptions.PaperSize = PdfPrintOptions.PdfPaperSize.A4;
                    Renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

                    Renderer.RenderingOptions.MarginTop = 15;
                    Renderer.RenderingOptions.MarginBottom = 5;
                    Renderer.RenderingOptions.MarginLeft = 5;
                    Renderer.RenderingOptions.MarginRight = 5;
                    //Renderer.RenderingOptions.FirstPageNumber = 1;
                    var imageHtml = "<div style=\"width:90%; margin:auto\"><image src=\"" +
                                    HttpUtility.UrlDecode(planoSvg.Image) + "\" style=\"max-height:100%;max-width: 100%;\"></div>";

                    Renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.FixedPixelWidth;
                    //Renderer.RenderingOptions.PaperFit.UseFitToPageRendering();
                    //Renderer.RenderingOptions.PaperFit.UseChromeDefaultRendering();

                    if (planogram.Stand.Width > planogram.Stand.Height)
                    {
                        //Renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.FixedPixelWidth;
                        Renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
                        //Renderer.RenderingOptions.PaperFit.UseFitToPageRendering(550);
                        imageHtml = "<div style=\"width:95%;margin:auto\"><image src=\"" +
                                    HttpUtility.UrlDecode(planoSvg.Image) + "\" style=\"max-height:100%;max-width: 100%;\"></div>";

                    }




                    var htmlPage = pageHtmlTop + imageHtml + pageHtmlBottom;
                    var PDF = Renderer.RenderHtmlAsPdf(htmlPage);
                    if (PDF.PageCount > 2)
                    {
                        PDF.Pages.Remove(PDF.Pages.First());
                    }
                    string PdfFileLocation = "~/planogram/pdf/";

                    var OutputPath = Path.Combine(_env.WebRootPath + "\n" + _env.ContentRootPath, planogram.Name + ".pdf");


                    var stream = PDF.Stream.ToArray();

                    var response = File(stream, "application/pdf");


                    return response;
                    // This neat trick opens our PDF file so we can see the result in our default PDF viewer
                    //System.Diagnostics.Process.Start(OutputPath);
                }
                catch (Exception Ex)
                {

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);

                    _logger.LogError("Error generating PDF");
                    if (Ex.InnerException != null)
                    {
                        message.Content = new StringContent(Ex.Message +
                                                      Ex.InnerException.ToString());

                        _logger.LogError(Ex.Message + Ex.InnerException.ToString());
                    }
                    else
                    {
                        message.Content = new StringContent(Ex.Message
                                      + Ex.StackTrace);

                        _logger.LogError(Ex.Message + Ex.StackTrace);
                    }
                    //message.ReasonPhrase = "Error creating pdf";
                    //log an error

                    return StatusCode(500, "Error generating pdf");

                }
            }
            else
            {
                return StatusCode(500, "Error generating pdf");
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
                    _planogramService.SavePlanogram(planogram);
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
                var spad = await _planogramService.GetScratchPad(scratchPadId);

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
