using AutoMapper;
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

    [Route("api/part")]
    [ApiController]
    public class PartController : ControllerBase
    {
        //private readonly IAppLogger<PartController> _logger;
        private readonly IMapper _mapper;
        private readonly ILogger<PartController> _logger;
        private readonly IAsyncRepositoryLong<Part> _partRepository;
        private readonly IAsyncRepository<PartType> _partTypeRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;


        public PartController(IMapper mapper, IAsyncRepository<PartType> partTypeRepository, IAsyncRepositoryLong<Part> partRepository, IAsyncRepository<Category> categoryRepository, ILogger<PartController> logger)
        {
            _logger = logger;
            _partRepository = partRepository;
            _partTypeRepository = partTypeRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllParts([FromQuery] PartFilterDto filterDto)
        {
            try
            {
                var ptSpec = new PartTypeSpecification();
                var partTypes = await _partTypeRepository.ListAsync(ptSpec);

                var categoryFilter = new CategoryFilter();
                //categoryFilter.ParentCategory = 0;
                var catSpec = new CategorySpecification(categoryFilter);
                var categories = await _categoryRepository.ListAsync(catSpec);

                var spec = new PartSpecification(_mapper.Map<PartFilter>(filterDto));
                var parts = await _partRepository.ListAsync(spec);
                    var countFilter = filterDto;
                countFilter.IsPagingEnabled = false;
                var countSpec = new PartSpecification(_mapper.Map<PartFilter>(countFilter));
                int totalItems = await _partRepository.CountAsync(countSpec);

                _logger.LogInformation($"Returned all parts from database.");
                //var ownersResult = _mapper.Map<IEnumerable<OwnerDto>>(owners);
                //return Ok(ownersResult);

                var response = new PagedPartsListDto();
                response.Data = _mapper.Map<List<PartListDto>>(parts);
                //update partTypenames here
                response.Page = new PMApplication.Dtos.Page();
                response.Page.PageNumber = filterDto.Page;
                response.Page.TotalItems = totalItems;
                response.Page.TotalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)filterDto.PageSize);
                response.Page.Size = filterDto.PageSize;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetAllParts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}", Name = "PartById")]
        public async Task<IActionResult> GetPartById(int id)
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

        //[HttpGet("{id}/account")]
        //public async Task<IActionResult> GetPartWithDetails(int id)
        //{
        //    try
        //    {
        //        var part = await _partRepository.

        //        if (part == null)
        //        {
        //            _logger.LogError($"Part with id: {id}, hasn't been found in db.");
        //            return NotFound();
        //        }
        //        else
        //        {
        //            _logger.LogInfo($"Returned part with details for id: {id}");
        //            //var ownerResult = _mapper.Map<OwnerDto>(owner);
        //            //return Ok(ownerResult);
        //            return Ok(part);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Something went wrong inside GetPartWithDetails action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> CreatePart([FromBody] Part part)
        {
            try
            {
                if (part == null)
                {
                    _logger.LogError("Part object sent from client is null.");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }

                var newPart = await _partRepository.AddAsync(part);

                return CreatedAtRoute("PartById", new { id = part.Id }, part);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreatePart action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePart(int id, [FromBody] Part part)
        {
            try
            {
                if (part == null)
                {
                    _logger.LogError("Part object sent from client is null.");
                    return BadRequest("Part object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }

                var dbPart = await _partRepository.GetByIdAsync(id);
                if (dbPart == null)
                {
                    _logger.LogError($"Part with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                await _partRepository.UpdateAsync(dbPart);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdatePart action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            try
            {
                var part = await _partRepository.GetByIdAsync(id);
                if (part == null)
                {
                    _logger.LogError($"Part with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                //if (_partRepository.Account.AccountsByPart(id).Any())
                //{
                //    _logger.LogError($"Cannot delete part with id: {id}. It has related accounts. Delete those accounts first");
                //    return BadRequest("Cannot delete part. It has related accounts. Delete those accounts first");
                //}

                await _partRepository.DeleteAsync(part);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside DeletePart action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
