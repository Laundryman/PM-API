using AutoMapper;
using CoreSystem2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Graph.Models;
using PlanMatr_API.Controllers.planm;
using PlanMatr_API.Extensions;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Specifications.Filters;
using PMApplication.Dtos.Notes;
using System.Security.Claims;
using System.Text.Json;

namespace PlanMatr_API.Controllers
{
    [Authorize]
    [Route("api/planogramNotes/[action]")]
    public class PlanogramNotesController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ILogger<PlanogramNotesController> _logger;
        private readonly IBrandService _brandService;
        private readonly IPlanogramService _planogramService;
        private readonly IPlanogramRepository _planogramRepository;
        private readonly IAsyncRepositoryLong<Planogram> _planogramAsyncRepository;

        private readonly ICountryService _countryService;
        private readonly IRegionService _regionService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public PlanogramNotesController(IMapper mapper, ILogger<PlanogramNotesController> logger, IBrandService brandService, IPlanogramService planogramService, IPlanogramRepository planogramRepository, IAsyncRepositoryLong<Planogram> planogramAsyncRepository, ICountryService countryService, IRegionService regionService, IAuditService auditService, IConfiguration config, IWebHostEnvironment env)
        {
            _mapper = mapper;
            _logger = logger;
            _brandService = brandService;
            _planogramService = planogramService;
            _planogramRepository = planogramRepository;
            _planogramAsyncRepository = planogramAsyncRepository;
            _countryService = countryService;
            _regionService = regionService;
            _auditService = auditService;
            _config = config;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes(int planogramId)
        {
            // we can retrieve the userId from the request
            try {
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            //var planogram = await _planogramService.GetPlanogram(planogramId);

                //we will create a custom model
                var planogramNote= new PlanogramNote();

                //We're not using the country and region here: but we need to think about how we might regarding users.
                var filter = new NoteFilter
                {
                    //UserId = userInfo.id,
                    //BrandId = planogramNotesModel.BrandId,
                    //CountryId = country.id,
                    PlanogramId = planogramId
                };
                var notes = await _planogramService.GetPlanogramNotes(filter);

                //var serializerSettings = new JsonSerializerOptions
                //{

                //    //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                //    //MaxDepth = 2
                //    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve

                //};

                //var jsonContent = JsonSerializer.Serialize(notes, serializerSettings);
                var response = _mapper.Map<IReadOnlyList<PlanogramNoteDto>>(notes);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving planogram notes for PlanogramId {PlanogramId}", planogramId);
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNote([FromBody] NewNoteDto note)
        {
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;

            try {

                try
                {
                    var newPNote = new PlanogramNote();
                    newPNote.Note = note.Note;
                    newPNote.PlanogramId = note.PlanogramId;
                    newPNote.NoteDate = DateTime.Now;
                    newPNote.UserId = userProfile?.Id;
                    newPNote.Username = userProfile?.UserName;
                    newPNote.NoteTitle = userProfile?.UserName +
                                         String.Format("{0:d/M/yyyy HH:mm:ss}", newPNote.NoteDate);
                    await _planogramService.CreatePlanogramNote(newPNote);
                    return Ok("Saved");
                }
                catch (Exception ex)
                {
                    return BadRequest("Failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding planogram note for PlanogramId {PlanogramId}", note.PlanogramId);
                return BadRequest("Failed");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ReplyNote([FromBody] NewNoteDto note)
        {
            try
            {
                var userProfile = await this.MappedUser();
                string? userId = userProfile?.Id;

                var planogramId = note.PlanogramId;
                var noteId = note.ReplyNoteId;
                var inReplyTo = await _planogramService.GetNote(noteId);
                var newPNote = new PlanogramNote();
                newPNote.Note = note.Note;
                newPNote.PlanogramId = note.PlanogramId;
                newPNote.NoteDate = DateTime.Now;
                newPNote.NoteInReplyTo = noteId;
                //newPNote.InReplyTo = inReplyTo;
                newPNote.UserId = userProfile?.Id;
                newPNote.Username = userProfile?.UserName;
                newPNote.NoteTitle = userProfile?.UserName +
                                     String.Format("{0:d/M/yyyy HH:mm:ss}", newPNote.NoteDate);
                await _planogramService.CreatePlanogramNote(newPNote);
                return Ok("Saved");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to planogram note for PlanogramId {PlanogramId}", note.PlanogramId);
                return BadRequest("Failed");
            }
        }


    }
}
