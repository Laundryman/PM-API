using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Categories;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PlanMatr_API.Controllers
{
    [Authorize]
    [Route("api/categories/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        //private ILoggerManager _logger;
        //private IRepositoryWrapper _repository;
        //private ILoggerManager _logger;
        private readonly ILogger<CategoryController> _logger;
        private readonly IMapper _mapper;
        //private readonly IReadRepository<Part> _partReadRepository;
        //private readonly IAsyncRepository<PartType> _partTypeRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;


        public CategoryController(IMapper mapper, IAsyncRepository<Category> categoryRepository, ILogger<CategoryController> logger)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpPost(Name = "Categories")]
        public async Task<IActionResult> GetCategories(CategoryFilterDto filterDto)
        {
            try
            {
                var spec = new CategorySpecification(_mapper.Map<CategoryFilter>(filterDto));
                var categories = await _categoryRepository.ListAsync(spec);

                foreach (var cat in categories)
                {
                    // Do something with each category
                    if (cat.ParentCategoryId == 0)
                    {
                        cat.ParentCategoryName = "1 PARENT CATEGORIES";
                    }
                }
                if (filterDto.GetParents)
                {
                    var mappedPCats = _mapper.Map<List<ParentCategoryDto>>(categories);
                    return Ok(mappedPCats);
                }
                var mappedCats = _mapper.Map<List<CategoryDto>>(categories);
                
                return Ok(mappedCats);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCategories action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet(Name = "CategorySelectList")]
        public async Task<IActionResult> GetCategorySelectList([FromQuery] CategoryFilterDto filterDto)
        {
            try
            {
                var spec = new CategorySpecification(_mapper.Map<CategoryFilter>(filterDto));
                var categories = await _categoryRepository.ListAsync(spec);

                var categoriesSelectList = CreateCategorySelectList(categories);
                return Ok(categoriesSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost(Name = "UpdateCategory")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryDto categoryUpdateDto)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(categoryUpdateDto.Id);
                if (category == null)
                {
                    return NotFound();
                }
                if (categoryUpdateDto.Name != null)
                {
                    category.Name = categoryUpdateDto.Name;
                }
                category.DisplayOrder = categoryUpdateDto.DisplayOrder;
                category.ParentCategoryId = categoryUpdateDto.ParentCategoryId;


                await _categoryRepository.UpdateAsync(category);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside UpdateCategory action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPost(Name = "AddCategory")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto categoryCreateDto)
        {
            try
            {
                var category = _mapper.Map<Category>(categoryCreateDto);
                var newCategory = await _categoryRepository.AddAsync(category);
                if (newCategory.ParentCategoryId != null)
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync((int)newCategory.ParentCategoryId);
                    newCategory.ParentCategory = parentCategory;
                }
                var createdCategory = _mapper.Map<CategoryDto>(newCategory);
                return Ok(createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside AddCategory action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}", Name = "DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                await _categoryRepository.DeleteAsync(category);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside DeleteCategory action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private List<CategoryDto> CreateCategorySelectList(IReadOnlyList<Category> list)
        {
            var selectList = new List<CategoryDto>();
            var categorySelect = new CategoryDto() { Name = "Select Category" };
            categorySelect.Id = 0;

            selectList.Add(categorySelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<CategoryDto>(list[i]));
            }

            return selectList;
        }

    }
}
