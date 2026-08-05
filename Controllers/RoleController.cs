using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Entities;
using PMApplication.Interfaces;

namespace PlanMatr_API.Controllers
{
    [Authorize]
    [Route("api/roles/[action]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly ILogger<RoleController> _logger;

        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Role> _roleRepository;
        private readonly IAsyncRepository<Permission> _permissionRepository;
        private readonly IConfiguration _configuration;


        public RoleController(IMapper mapper, IAsyncRepository<Role> roleRepository, IAsyncRepository<Permission> permissionRepository, ILogger<RoleController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleRepository.ListAllAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetRoles action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet(Name = "Permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            try
            {
                var permissions = await _permissionRepository.ListAllAsync();

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetPermissions action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }





    }
}

