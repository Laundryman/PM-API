using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlanMatr_API.Controllers
{

    [Route("api/part/[action]")]
    [Authorize]
    [ApiController]
    public class PartController : ControllerBase
    {
        //private readonly IAppLogger<PartController> _logger;
        private readonly IMapper _mapper;
        private readonly ILogger<PartController> _logger;
        private readonly IAsyncRepositoryLong<Part> _partRepository;
        private readonly IAsyncRepository<PartType> _partTypeRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;


        public PartController(IMapper mapper, IAsyncRepository<PartType> partTypeRepository,
            IAsyncRepositoryLong<Part> partRepository, IAsyncRepository<Category> categoryRepository,
            ILogger<PartController> logger)
        {
            _logger = logger;
            _partRepository = partRepository;
            _partTypeRepository = partTypeRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> SearchParts([FromQuery] PartFilterDto filterDto)
        {
            try
            {

                var spec = new PartSpecification(_mapper.Map<PartFilter>(filterDto));
                var parts = await _partRepository.ListAsync(spec);


                _logger.LogInformation($"Returned all parts from database.");
                var response = _mapper.Map<List<PartListDto>>(parts);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchParts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPart(int id)
        {
            try
            {
                var part = await _partRepository.GetByIdAsync(id);

                if (part == null)
                {
                    _logger.LogWarning($"Part with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned part with id: {id}");
                    var response = _mapper.Map<PartDto>(part);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetPartById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
