using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.API.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
