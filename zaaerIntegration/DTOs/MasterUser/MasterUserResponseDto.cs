namespace zaaerIntegration.DTOs.MasterUser
{
    /// <summary>
    /// DTO for Master User response
    /// </summary>
    public class MasterUserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string? TenantName { get; set; }
        public string? TenantCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
        public List<TenantDto> AccessibleTenants { get; set; } = new List<TenantDto>();
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class TenantDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

