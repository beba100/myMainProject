using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.MasterUser;
using zaaerIntegration.Services.Interfaces;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for managing Master Users
    /// Accessible without authentication for developer-users.html page
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // Removed [Authorize] to allow access without authentication
    public class MasterUserController : ControllerBase
    {
        private readonly IMasterUserService _masterUserService;
        private readonly zaaerIntegration.Data.MasterDbContext _masterDbContext;
        private readonly ILogger<MasterUserController> _logger;

        /// <summary>
        /// Constructor for MasterUserController
        /// </summary>
        /// <param name="masterUserService">Master user service</param>
        /// <param name="masterDbContext">Master database context</param>
        /// <param name="logger">Logger instance</param>
        public MasterUserController(
            IMasterUserService masterUserService,
            zaaerIntegration.Data.MasterDbContext masterDbContext,
            ILogger<MasterUserController> logger)
        {
            _masterUserService = masterUserService;
            _masterDbContext = masterDbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get all master users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _masterUserService.GetAllUsersAsync();
                var usersList = users.ToList();
                
                // Get UserTenants for each user
                List<UserTenant> userTenants = new List<UserTenant>();
                if (usersList.Any())
                {
                    var userIds = usersList.Select(u => u.Id).ToList();
                    userTenants = await _masterDbContext.UserTenants
                        .AsNoTracking()
                        .Include(ut => ut.Tenant)
                        .Where(ut => userIds.Contains(ut.UserId))
                        .ToListAsync();
                }

                var response = usersList.Select(u => new MasterUserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    TenantId = u.TenantId,
                    TenantName = u.Tenant?.Name,
                    TenantCode = u.Tenant?.Code,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    EmployeeNumber = u.EmployeeNumber,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = u.UserRoles.Select(ur => new RoleDto
                    {
                        Id = ur.Role?.Id ?? 0,
                        Name = ur.Role?.Name ?? string.Empty,
                        Code = ur.Role?.Code ?? string.Empty
                    }).ToList(),
                    AccessibleTenants = userTenants
                        .Where(ut => ut.UserId == u.Id)
                        .Select(ut => new TenantDto
                        {
                            Id = ut.Tenant?.Id ?? 0,
                            Code = ut.Tenant?.Code ?? string.Empty,
                            Name = ut.Tenant?.Name ?? string.Empty
                        })
                        .Concat(new[] { new TenantDto
                        {
                            Id = u.TenantId,
                            Code = u.Tenant?.Code ?? string.Empty,
                            Name = u.Tenant?.Name ?? string.Empty
                        }})
                        .GroupBy(t => t.Id)
                        .Select(g => g.First())
                        .ToList()
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users: {Message}", ex.Message);
                return StatusCode(500, new { error = $"An error occurred while retrieving users: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _masterUserService.GetByIdAsync(id);
                if (user == null)
                    return NotFound(new { error = $"User with id {id} not found" });

                // Get UserTenants
                var userTenants = await _masterDbContext.UserTenants
                    .AsNoTracking()
                    .Include(ut => ut.Tenant)
                    .Where(ut => ut.UserId == id)
                    .ToListAsync();

                var response = new MasterUserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    TenantId = user.TenantId,
                    TenantName = user.Tenant?.Name,
                    TenantCode = user.Tenant?.Code,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    EmployeeNumber = user.EmployeeNumber,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = user.UserRoles.Select(ur => new RoleDto
                    {
                        Id = ur.Role?.Id ?? 0,
                        Name = ur.Role?.Name ?? string.Empty,
                        Code = ur.Role?.Code ?? string.Empty
                    }).ToList(),
                    AccessibleTenants = userTenants
                        .Select(ut => new TenantDto
                        {
                            Id = ut.Tenant?.Id ?? 0,
                            Code = ut.Tenant?.Code ?? string.Empty,
                            Name = ut.Tenant?.Name ?? string.Empty
                        })
                        .Concat(new[] { new TenantDto
                        {
                            Id = user.TenantId,
                            Code = user.Tenant?.Code ?? string.Empty,
                            Name = user.Tenant?.Name ?? string.Empty
                        }})
                        .GroupBy(t => t.Id)
                        .Select(g => g.First())
                        .ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the user." });
            }
        }

        /// <summary>
        /// Create a new master user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateMasterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _masterUserService.CreateUserAsync(
                    dto.Username,
                    dto.Password,
                    dto.TenantId,
                    dto.RoleIds,
                    dto.PhoneNumber,
                    dto.Email,
                    dto.EmployeeNumber,
                    dto.FullName,
                    dto.AdditionalTenantIds
                );

                // Reload with includes
                var createdUser = await _masterUserService.GetByIdAsync(user.Id);
                if (createdUser == null)
                    return NotFound(new { error = "User was created but could not be retrieved" });

                // Get UserTenants
                var userTenants = await _masterDbContext.UserTenants
                    .AsNoTracking()
                    .Include(ut => ut.Tenant)
                    .Where(ut => ut.UserId == user.Id)
                    .ToListAsync();

                var response = new MasterUserResponseDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    TenantId = createdUser.TenantId,
                    TenantName = createdUser.Tenant?.Name,
                    TenantCode = createdUser.Tenant?.Code,
                    PhoneNumber = createdUser.PhoneNumber,
                    Email = createdUser.Email,
                    EmployeeNumber = createdUser.EmployeeNumber,
                    FullName = createdUser.FullName,
                    IsActive = createdUser.IsActive,
                    CreatedAt = createdUser.CreatedAt,
                    UpdatedAt = createdUser.UpdatedAt,
                    Roles = createdUser.UserRoles.Select(ur => new RoleDto
                    {
                        Id = ur.Role?.Id ?? 0,
                        Name = ur.Role?.Name ?? string.Empty,
                        Code = ur.Role?.Code ?? string.Empty
                    }).ToList(),
                    AccessibleTenants = userTenants
                        .Select(ut => new TenantDto
                        {
                            Id = ut.Tenant?.Id ?? 0,
                            Code = ut.Tenant?.Code ?? string.Empty,
                            Name = ut.Tenant?.Name ?? string.Empty
                        })
                        .Concat(new[] { new TenantDto
                        {
                            Id = createdUser.TenantId,
                            Code = createdUser.Tenant?.Code ?? string.Empty,
                            Name = createdUser.Tenant?.Name ?? string.Empty
                        }})
                        .DistinctBy(t => t.Id)
                        .ToList()
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tenant not found while creating user");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating user");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { error = "An error occurred while creating the user." });
            }
        }

        /// <summary>
        /// Update an existing master user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateMasterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _masterUserService.UpdateUserAsync(
                    id,
                    dto.Username,
                    dto.Password,
                    dto.TenantId,
                    dto.PhoneNumber,
                    dto.Email,
                    dto.EmployeeNumber,
                    dto.FullName,
                    dto.IsActive,
                    dto.RoleIds,
                    dto.AdditionalTenantIds
                );

                // Reload with includes
                var updatedUser = await _masterUserService.GetByIdAsync(user.Id);
                if (updatedUser == null)
                    return NotFound(new { error = "User was updated but could not be retrieved" });

                // Get UserTenants
                var userTenants = await _masterDbContext.UserTenants
                    .AsNoTracking()
                    .Include(ut => ut.Tenant)
                    .Where(ut => ut.UserId == user.Id)
                    .ToListAsync();

                var response = new MasterUserResponseDto
                {
                    Id = updatedUser.Id,
                    Username = updatedUser.Username,
                    TenantId = updatedUser.TenantId,
                    TenantName = updatedUser.Tenant?.Name,
                    TenantCode = updatedUser.Tenant?.Code,
                    PhoneNumber = updatedUser.PhoneNumber,
                    Email = updatedUser.Email,
                    EmployeeNumber = updatedUser.EmployeeNumber,
                    FullName = updatedUser.FullName,
                    IsActive = updatedUser.IsActive,
                    CreatedAt = updatedUser.CreatedAt,
                    UpdatedAt = updatedUser.UpdatedAt,
                    Roles = updatedUser.UserRoles.Select(ur => new RoleDto
                    {
                        Id = ur.Role?.Id ?? 0,
                        Name = ur.Role?.Name ?? string.Empty,
                        Code = ur.Role?.Code ?? string.Empty
                    }).ToList(),
                    AccessibleTenants = userTenants
                        .Select(ut => new TenantDto
                        {
                            Id = ut.Tenant?.Id ?? 0,
                            Code = ut.Tenant?.Code ?? string.Empty,
                            Name = ut.Tenant?.Name ?? string.Empty
                        })
                        .Concat(new[] { new TenantDto
                        {
                            Id = updatedUser.TenantId,
                            Code = updatedUser.Tenant?.Code ?? string.Empty,
                            Name = updatedUser.Tenant?.Name ?? string.Empty
                        }})
                        .DistinctBy(t => t.Id)
                        .ToList()
                };

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User or tenant not found while updating user");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating user");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the user." });
            }
        }

        /// <summary>
        /// Delete a master user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _masterUserService.DeleteUserAsync(id);
                if (!result)
                    return NotFound(new { error = $"User with id {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the user." });
            }
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _masterDbContext.Roles
                    .AsNoTracking()
                    .OrderBy(r => r.Name)
                    .Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Code = r.Code
                    })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles: {Message}", ex.Message);
                return StatusCode(500, new { error = $"An error occurred while retrieving roles: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all tenants (hotels)
        /// </summary>
        [HttpGet("tenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            try
            {
                var tenants = await _masterDbContext.Tenants
                    .AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => new TenantDto
                    {
                        Id = t.Id,
                        Code = t.Code,
                        Name = t.Name
                    })
                    .ToListAsync();

                return Ok(tenants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenants: {Message}", ex.Message);
                return StatusCode(500, new { error = $"An error occurred while retrieving tenants: {ex.Message}" });
            }
        }
    }
}

