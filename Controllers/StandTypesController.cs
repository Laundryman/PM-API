using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Services;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PlanMatr_API.Controllers
{
    [Authorize]
    [Route("api/standTypes/[action]")]
    [ApiController]
    public class StandTypesController : ControllerBase
    {
        private readonly ILogger<StandTypesController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepository<StandType> _asyncStandTypeRepository;
        private readonly IStandTypeRepository _standTypeRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;



        public StandTypesController(IMapper mapper, IAsyncRepository<StandType> asyncStandTypeRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            ILogger<StandTypesController> logger, IStandTypeRepository standTypeRepository, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _logger = logger;
            _standTypeRepository = standTypeRepository;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _asyncStandTypeRepository = asyncStandTypeRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }


        [HttpPost]
        public async Task<IActionResult> GetStandTypes(StandTypeFilterDto filterDto)
        {
            try
            {
                var spec = new StandTypeSpecification(_mapper.Map<StandTypeFilter>(filterDto));
                var standTypes = await _asyncStandTypeRepository.ListAsync(spec);

                if (filterDto.GetParents)
                {
                    var mappedPTypes = _mapper.Map<List<ParentStandTypeDto>>(standTypes);
                    return Ok(mappedPTypes);
                }

                var mappedTypes = _mapper.Map<List<StandTypeDto>>(standTypes);
                return Ok(mappedTypes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetStandTypes action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddStandType(StandTypeUploadDto standTypeDto)
        {
            try
            {
                var standType = new StandType
                {
                    Name = standTypeDto.Name,
                    Description = standTypeDto.Description,
                    BrandId = standTypeDto.BrandId,
                    Lock = standTypeDto.Lock,
                    HidePrices = standTypeDto.HidePrices,
                    ParentStandTypeId = standTypeDto.ParentStandTypeId
                };

                StandType parentStandType = new StandType();
                if (standTypeDto.ParentStandTypeId != null)
                {
                    standType.ParentStandTypeId = standTypeDto.ParentStandTypeId;
                    parentStandType = await _standTypeRepository.GetByIdAsync((int)standTypeDto.ParentStandTypeId);
                }
                else if (standType.ParentStandTypeId != null)
                {
                    parentStandType = await _standTypeRepository.GetByIdAsync((int)standType.ParentStandTypeId);
                }
                if (standTypeDto.File != null && standTypeDto.File.Length > 0)
                {
                    // Here you would typically save the file to a storage location and update the BrandLogo property
                    // For demonstration, we'll just set a placeholder path
                    var fileType = standTypeDto.File.FileName.Split('.')[1];
                    string fileName = standType.Name.Replace(" ", "");
                    standType.StandImage = fileName + "." + fileType;
                    var blobService = new AzureBlobService(_blobServiceClient);
                    var storeName = _configuration["AzureBlob:StoreName"];
                    var brandContainerName = _configuration["AzureBlob:StandTypeContainer"];
                    if (storeName != null && brandContainerName != null)
                    {
                        var blobServiceClient = blobService.GetBlobServiceClient(storeName);
                        var containerClient = blobService.GetBlobContainerClient(blobServiceClient, brandContainerName);
                        await blobService.UploadFormFileAsync(containerClient, standTypeDto.File, standType.StandImage);
                    }
                }

                if (standTypeDto.ParentStandTypeId != null)
                    standType.ParentStandType = parentStandType;
                
                var createdStandType = await _standTypeRepository.AddAsync(standType);
                var returnData = _mapper.Map<StandTypeDto>(standType);
                returnData.ParentStandType = _mapper.Map<StandTypeDto>(parentStandType);
                return Ok(returnData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside AddStandType action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateStandType([FromForm]StandTypeUploadDto standTypeDto)
        {
            try
            {
                var standType = await _standTypeRepository.GetByIdAsync(standTypeDto.Id);
                if (standType == null)
                {
                    _logger.LogWarning($"StandType with id: {standTypeDto.Id}, hasn't been found in db.");
                    return NotFound();
                }
                standType.Name = standTypeDto.Name;
                standType.Description = standTypeDto.Description;
                StandType parentStandType = new StandType();
                if (standTypeDto.ParentStandTypeId != null)
                {
                    standType.ParentStandTypeId = standTypeDto.ParentStandTypeId;
                    parentStandType = await _standTypeRepository.GetByIdAsync((int)standTypeDto.ParentStandTypeId);
                }
                else if (standType.ParentStandTypeId != null)
                {
                    parentStandType = await _standTypeRepository.GetByIdAsync((int)standType.ParentStandTypeId);
                }

                if (standTypeDto.ParentStandTypeId != null) 
                    standType.StandImage = standTypeDto.StandImage;

                if (standTypeDto.File != null && standTypeDto.File.Length > 0)
                {
                    // Here you would typically save the file to a storage location and update the BrandLogo property
                    // For demonstration, we'll just set a placeholder path
                    var fileType = standTypeDto.File.FileName.Split('.')[1];
                    string fileName = standType.Name.Replace(" ", "");
                    standType.StandImage = fileName + "." + fileType;
                    var blobService = new AzureBlobService(_blobServiceClient);
                    var storeName = _configuration["AzureBlob:StoreName"];
                    var brandContainerName = _configuration["AzureBlob:StandTypeContainer"];
                    if (storeName != null && brandContainerName != null)
                    {
                        var blobServiceClient = blobService.GetBlobServiceClient(storeName);
                        var containerClient = blobService.GetBlobContainerClient(blobServiceClient, brandContainerName);
                        await blobService.UploadFormFileAsync(containerClient, standTypeDto.File, standType.StandImage);
                    }
                }



                await _standTypeRepository.UpdateAsync(standType);
                var returnData = _mapper.Map<StandTypeDto>(standType);
                returnData.ParentStandType = _mapper.Map<StandTypeDto>(parentStandType);
                return Ok(returnData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside UpdateStandType action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        //[HttpGet(Name = "ProductSelectList")]
        //public async Task<IActionResult> GetStandSelectList([FromQuery] ProductFilterDto filterDto)
        //{
        //    try
        //    {
        //        //if (filterDto.CountriesList != null)
        //        //{
        //        //    var allCountries = await IsAllCountries(filterDto.CountriesList, _countryRepository, _mapper);
        //        //    if (allCountries)
        //        //    {
        //        //        filterDto.CountriesList = null;
        //        //    }
        //        //}

        //        var spec = new StandSpecification(_mapper.Map<StandFilter>(filterDto));
        //        var stands = await _asyncStandRepository.ListAsync(spec);

        //        var StandSelectList = CreateSelectList(stands);
        //        return Ok(StandSelectList);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning($"Something went wrong inside GetStand action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //[HttpGet("{id}", Name = "StandById")]
        //public async Task<IActionResult> GetStandById(int id)
        //{
        //    try
        //    {
        //        var stand = await _asyncStandRepository.GetByIdAsync(id);

        //        if (stand == null)
        //        {
        //            _logger.LogWarning($"Stand with id: {id}, hasn't been found in db.");
        //            return NotFound();
        //        }
        //        else
        //        {
        //            _logger.LogInformation($"Returned stand with id: {id}");
        //            var response = _mapper.Map<FullStandDto>(stand);

        //            return Ok(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Something went wrong inside GetProductById action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //private List<StandListDto> CreateSelectList(IReadOnlyList<Stand> list)
        //{
        //    var selectList = new List<StandListDto>();
        //    var productSelect = new StandListDto("Select Product");
        //    productSelect.Id = 0;

        //    selectList.Add(productSelect);

        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        selectList.Add(_mapper.Map<StandListDto>(list[i]));
        //    }

        //    return selectList;
        //}

        //private async Task<List<Stand>> GetStandsFromCountryList(string countryList, IReadOnlyList<Stand> products)
        //{
        //    //we need to filter only products that have the at least one of the countries that are required
        //    var requiredCountryList = countryList.Split(',');var filteredStands = new List<Stand>();

        //    foreach (var product in products)
        //    {
        //        if (stand.CountriesList != null)
        //        {
        //            var productCountryList = stand.CountriesList.Split(",");

        //            foreach (var country in productCountryList)
        //            {
        //                if (requiredCountryList.Contains(country))
        //                {
        //                    filteredProducts.Add(stand);
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    return filteredProducts;

        //}
    }
}
