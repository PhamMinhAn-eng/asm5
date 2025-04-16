using Microsoft.AspNetCore.Identity;
using ASM5.Models;
using ASM5.Data;

public static class SeedData
{




    public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Kiểm tra nếu chưa có danh mục nào
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { CategoryName = "Electronics" },
                    new Category { CategoryName = "Fashion" }
                );

                await context.SaveChangesAsync();
            }
        }
        // Lấy RoleManager và UserManager từ service provider
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
  
        // Danh sách các role cần seed
        string[] roles = new string[] { "Customer", "Employee", "Manager" };

        // Seed các role nếu chưa tồn tại
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
      
        // Seed User: Customer
        if (await userManager.FindByEmailAsync("customer1@gmail.com") == null)
        {
            var customerUser = new ApplicationUser
            {
                UserName = "customer1@gmail.com",
                Email = "customer1@gmail.com",
                FullName = "Customer User",
                Address = "Customer Address",
                TinhTrangHoatDong = "on"
            };

            var result = await userManager.CreateAsync(customerUser, "Pass@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, "Customer");
            }
        }

        // Seed User: Employee
        if (await userManager.FindByEmailAsync("employee@gmail.com") == null)
        {
            var employeeUser = new ApplicationUser
            {
                UserName = "employee@gmail.com",
                Email = "employee@gmail.com",
                FullName = "Employee User",
                Address = "Employee Address",
                TinhTrangHoatDong = "on"
            };

            var result = await userManager.CreateAsync(employeeUser, "Pass@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employeeUser, "Employee");
            }
        }

        // Seed User: Manager
        if (await userManager.FindByEmailAsync("manager1@gmail.com") == null)
        {
            var managerUser = new ApplicationUser
            {
                UserName = "manager1@gmail.com",
                Email = "manager1@gmail.com",
                FullName = "Manager User",
                Address = "Manager Address",
                TinhTrangHoatDong = "on"
            };

            var result = await userManager.CreateAsync(managerUser, "Pass@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }
    }
}
