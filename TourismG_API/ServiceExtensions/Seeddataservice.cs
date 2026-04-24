using Domain.Models;
using Microsoft.AspNetCore.Identity;


namespace Presentation.ServiceExtensions
{
    public class SeedDataService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SeedDataService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Create roles if they don't exist
                await CreateRolesAsync();

                // Create admin user if it doesn't exist
                await CreateAdminUserAsync();

                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task CreateRolesAsync()
        {
            var roles = new[] { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Role '{role}' created successfully.");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create role '{role}'.");
                    }
                }
                else
                {
                    _logger.LogInformation($"Role '{role}' already exists.");
                }
            }
        }

        private async Task CreateAdminUserAsync()
        {
            const string adminEmail = "admin@example.com";
            const string adminPassword = "Admin@123456"; // Change this to a secure password

            // Check if admin user already exists
            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                _logger.LogInformation("Admin user already exists.");
                return;
            }

            // Create new admin user
            var adminUser = new User
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                // Assign Admin role to the new user
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Admin user created successfully with Admin role assigned.");
                }
                else
                {
                    _logger.LogError("Admin user created but failed to assign Admin role.");
                }
            }
            else
            {
                _logger.LogError($"Failed to create admin user. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
