using Microsoft.AspNetCore.Identity;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Seed Roles: Admin, HR, Manager, Employee
        string[] roles = { "Admin", "HR", "Manager", "Employee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    NormalizedName = role.ToUpper(),
                    Description = $"{role} Role for EMS"
                });
            }
        }

        // 2. Seed Default Admin User
        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@ems.com";
        var adminPassword = configuration["AdminSeed:Password"] ?? "Admin@123Password!";
        var adminFullName = configuration["AdminSeed:FullName"] ?? "System Administrator";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
