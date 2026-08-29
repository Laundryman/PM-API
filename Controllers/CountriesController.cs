using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMatr_API.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PlanMatr_API.Controllers
{
    [Authorize]
    [Route("api/countries/[action]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        //private ILoggerManager _logger;
        //private IRepositoryWrapper _repository;
        //private ILoggerManager _logger;
        private readonly ILogger<CountriesController> _logger;
        private readonly IMapper _mapper;
        //private readonly IReadRepository<Part> _partReadRepository;
        private readonly IAsyncRepository<Region> _regionRepository;
        private readonly IAsyncRepository<Country> _countryRepository;


        public CountriesController(IMapper mapper, IAsyncRepository<Country> countryRepository, IAsyncRepository<Region> regionRepository, ILogger<CountriesController> logger)
        {
            _logger = logger;
            _countryRepository = countryRepository;
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        #region Countries

        [HttpGet(Name = "Countries")]
        public async Task<IActionResult> GetAllCountries(int? regionId, string? searchText = "")
        {
            try
            {
                var userProfile = await this.MappedUser();
                var userCountries = userProfile.CountryList.Split(",").Select(int.Parse).ToList();

                var filter = new CountryFilter();
                filter.RegionId = regionId;
                var spec = new CountrySpecification(filter);
                var countries = await _countryRepository.ListAsync(spec);
                
                countries = countries.Where(c => userCountries.Contains(c.Id)).ToList();

                _logger.LogInformation($"Returned all countries from database.");

                var response = _mapper.Map<List<CountryDto>>(countries);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet( "{countryList}", Name = "CountriesFromList")]
        public async Task<IActionResult> GetCountriesFromList(string countryList)
        {
            try
            {
                var countryIds = countryList.Split(',').ToList();
                var filterDto = new CountryFilter();
                filterDto.CountryList = countryList;
                var spec = new CountrySpecification(_mapper.Map<CountryFilter>(filterDto));
                var countries = await _countryRepository.ListAsync(spec);

                var countriesSelectList = CreateCountrySelectList(countries);
                return Ok(countriesSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet(Name = "CountriesSelectList")]
        public async Task<IActionResult> GetCountrySelectList([FromQuery] CountriesFilterDto filterDto)
        {
            try
            {
                var spec = new CountrySpecification(_mapper.Map<CountryFilter>(filterDto));
                var countries = await _countryRepository.ListAsync(spec);

                var countriesSelectList = CreateCountrySelectList(countries);
                return Ok(countriesSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<CountryDto> CreateCountrySelectList(IReadOnlyList<Country> list)
        {
            var selectList = new List<CountryDto>();
            var countrySelect = new CountryDto("Select Country");
            countrySelect.Id = 0;

            selectList.Add(countrySelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<CountryDto>(list[i]));
            }

            return selectList;
        }

        #endregion

        #region Regions

        [HttpGet(Name = "Regions")]
        public async Task<IActionResult> GetAllRegions(int brandId, string? searchText = "")
        {
            try
            {
                var userProfile = await this.MappedUser();
                var userRegions = userProfile.RegionList.Split(",").Select(int.Parse).ToList();
                var filterDto = new RegionsFilterDto();
                filterDto.BrandId = brandId;
                var spec = new RegionSpecification(_mapper.Map<RegionFilter>(filterDto));
                var regions = await _regionRepository.ListAsync(spec);
                regions = regions.Where(r => userRegions.Contains(r.Id)).ToList();
                _logger.LogInformation($"Returned regions from database.");

                return Ok(regions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetRegions action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost(Name = "RegionsSelectList")]
        public async Task<IActionResult> GetRegions(RegionsFilterDto filterDto)
        {
            try
            {
                var userProfile = await this.MappedUser();
                var spec = new RegionSpecification(_mapper.Map<RegionFilter>(filterDto));
                var regions = await _regionRepository.ListAsync(spec);
                if (userProfile.RegionList != null)
                {
                    var userRegions = userProfile.RegionList.Split(",").Select(int.Parse).ToList();
                regions = regions.Where(r => userRegions.Contains(r.Id)).ToList();
                }

                var response = _mapper.Map<List<RegionDto>>(regions);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetRegions action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("{id}", Name = "RegionById")]
        public async Task<IActionResult> GetRegionById(int id)
        {
            try
            {
                var region = await _regionRepository.GetByIdAsync(id);

                if (region == null)
                {
                    _logger.LogWarning($"Region with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned region with id: {id}");
                    var response = _mapper.Map<RegionDto>(region);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetRegionById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveRegion(RegionDto region)
        {
            try
            {
                if (region != null)
                {
                    if (region.Id == 0)
                    {
                        var regionEntity = _mapper.Map<Region>(region);
                        await _regionRepository.AddAsync(regionEntity);
                        _logger.LogInformation($"Region with id: {regionEntity.Id} has been created.");
                        return Ok(regionEntity.Id);
                    }
                    else
                    {
                        var regionFilter = new RegionFilter();
                        regionFilter.Id = region.Id;
                        regionFilter.LoadChildren=true;
                        var spec = new RegionSpecification(regionFilter);
                        var regionEntity = await _regionRepository.FirstAsync(spec);

                        if (regionEntity == null)
                        {
                            _logger.LogWarning($"Region with id: {region.Id}, hasn't been found in db.");
                            return NotFound();
                        }

                        regionEntity.Name = region.Name;
                        await UpdateCountryCollection(regionEntity, region);
                        await _regionRepository.UpdateAsync(regionEntity);
                        _logger.LogInformation($"Region with id: {regionEntity.Id} has been updated.");
                        return Ok(regionEntity.Id);
                    }
                }
                else
                {
                    _logger.LogWarning($"Region object sent from client is null.");
                    return BadRequest("Region object is null");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside SaveRegion action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private List<RegionDto> CreateSelectList(IReadOnlyList<Region> list)
        {
            var selectList = new List<RegionDto>();
            var regionSelect = new RegionDto("Select Region");
            regionSelect.Id = 0;

            selectList.Add(regionSelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<RegionDto>(list[i]));
            }

            return selectList;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateCountryCollection(Region origRegion, RegionDto updateRegion)
        {
            //add new countries
            //var standCountries = JsonConvert.DeserializeObject<List<CountryDto>>(updateStand.Countries);
            foreach (var country in updateRegion.Countries)
            {
                var origCountry = origRegion.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(country.Id);
                    if (dbCountry != null)
                    {
                        origRegion.Countries.Add(dbCountry);
                    }
                }
            }
            //remove deleted countries
            for (int i = origRegion.Countries.Count - 1; i >= 0; i--)
            {
                var origCountry = origRegion.Countries[i];
                var updatedCountry = updateRegion.Countries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(origCountry.Id);
                    origRegion.Countries.Remove(dbCountry);
                }
            }

            //update Part.CountryList string
            origRegion.CountryList = string.Join(",", origRegion.Countries.Select(c => c.Id));
        }

        #endregion
    }
}
