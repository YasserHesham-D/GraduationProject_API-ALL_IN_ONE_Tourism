using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Presentation.ServiceExtensions
{
    public class SeedDataService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            ILogger<SeedDataService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await CreateRolesAsync();
                await CreateAdminUserAsync();
                var provider = await CreateProviderUserAsync();
                await SeedTourismDataAsync(provider.Id);

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
            var roles = new[] { "Admin", "Customer", "Provider" };

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
            const string adminPassword = "Admin@123456";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                _logger.LogInformation("Admin user already exists.");
                return;
            }

            var adminUser = new User
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Admin user created successfully with Admin role assigned.");
            }
            else
            {
                var errors = FormatIdentityErrors(result);
                _logger.LogError("Failed to create admin user. Errors: {Errors}", errors);
                throw new InvalidOperationException($"Failed to create admin user. Errors: {errors}");
            }
        }

        private async Task<User> CreateProviderUserAsync()
        {
            const string providerEmail = "provider@example.com";
            const string providerPassword = "Provider@123456";

            var existingProvider = await _userManager.FindByEmailAsync(providerEmail);
            if (existingProvider != null)
            {
                if (!await _userManager.IsInRoleAsync(existingProvider, "Provider"))
                {
                    await _userManager.AddToRoleAsync(existingProvider, "Provider");
                }

                return existingProvider;
            }

            var provider = new User
            {
                UserName = "provider",
                Email = providerEmail,
                EmailConfirmed = true,
                Address = "Giza, Egypt"
            };

            var result = await _userManager.CreateAsync(provider, providerPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(provider, "Provider");
            }
            else
            {
                var errors = FormatIdentityErrors(result);
                _logger.LogError("Failed to create provider user. Errors: {Errors}", errors);
                throw new InvalidOperationException($"Failed to create provider user. Errors: {errors}");
            }

            return provider;
        }

        private static string FormatIdentityErrors(IdentityResult result)
        {
            return string.Join(", ", result.Errors.Select(e => e.Description));
        }

        private async Task SeedTourismDataAsync(string providerId)
        {
            if (await _context.Places.AnyAsync())
            {
                return;
            }

            var places = new List<Place>
            {
                new()
                {
                    Name = "Pyramids of Giza",
                    Category = "Historical",
                    LocationName = "Giza, Egypt",
                    City = "Giza",
                    Description = "Ancient royal tombs and one of Egypt's most iconic historical sites.",
                    ImageUrl = "https://images.unsplash.com/photo-1503177119275-0aa32b3a9368",
                    Rating = 4.9m,
                    ReviewCount = 12543,
                    PriceFrom = 200,
                    OpeningHours = "8:00 AM - 5:00 PM",
                    DistanceKm = 15,
                    Latitude = 29.9792m,
                    Longitude = 31.1342m,
                    IsRecommended = true,
                    IsPopular = true
                },
                new()
                {
                    Name = "Luxor Temple",
                    Category = "Historical",
                    LocationName = "Luxor, Egypt",
                    City = "Luxor",
                    Description = "A grand ancient Egyptian temple complex in the heart of Luxor.",
                    ImageUrl = "https://images.unsplash.com/photo-1601225475516-60c383a15d17",
                    Rating = 4.8m,
                    ReviewCount = 8234,
                    PriceFrom = 250,
                    OpeningHours = "6:00 AM - 10:00 PM",
                    DistanceKm = 3,
                    Latitude = 25.6995m,
                    Longitude = 32.6391m,
                    IsRecommended = true,
                    IsPopular = true
                },
                new()
                {
                    Name = "Red Sea Resort",
                    Category = "Nature",
                    LocationName = "Hurghada, Egypt",
                    City = "Hurghada",
                    Description = "A clear-water beach destination for snorkeling, diving, and relaxing.",
                    ImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e",
                    Rating = 4.6m,
                    ReviewCount = 3210,
                    PriceFrom = 450,
                    OpeningHours = "Open all day",
                    DistanceKm = 5,
                    IsRecommended = true,
                    IsPopular = true
                },
                new()
                {
                    Name = "Cairo Museum",
                    Category = "Cultural",
                    LocationName = "Cairo, Egypt",
                    City = "Cairo",
                    Description = "A landmark museum with ancient Egyptian artifacts and royal collections.",
                    ImageUrl = "https://images.unsplash.com/photo-1572252009286-268acec5ca0a",
                    Rating = 4.6m,
                    ReviewCount = 9876,
                    PriceFrom = 180,
                    OpeningHours = "9:00 AM - 5:00 PM",
                    DistanceKm = 2,
                    IsRecommended = true,
                    IsPopular = true
                },
                new()
                {
                    Name = "Great Sphinx",
                    Category = "Historical",
                    LocationName = "Giza, Egypt",
                    City = "Giza",
                    Description = "The limestone guardian of the Giza plateau.",
                    ImageUrl = "https://images.unsplash.com/photo-1539650116574-75c0c6d73f6e",
                    Rating = 4.8m,
                    ReviewCount = 8976,
                    PriceFrom = 150,
                    OpeningHours = "8:00 AM - 5:00 PM",
                    DistanceKm = 14,
                    IsRecommended = true,
                    IsPopular = false
                },
                new()
                {
                    Name = "Nile River",
                    Category = "Nature",
                    LocationName = "Aswan, Egypt",
                    City = "Aswan",
                    Description = "A scenic stretch of the Nile with cruises and riverside views.",
                    ImageUrl = "https://images.unsplash.com/photo-1587975844610-40f1ad10d07e",
                    Rating = 4.7m,
                    ReviewCount = 6123,
                    PriceFrom = 350,
                    OpeningHours = "Open all day",
                    DistanceKm = 8,
                    IsRecommended = false,
                    IsPopular = true
                }
            };

            await _context.Places.AddRangeAsync(places);

            var giza = places.First(p => p.Name == "Pyramids of Giza");
            var luxor = places.First(p => p.Name == "Luxor Temple");
            var nile = places.First(p => p.Name == "Nile River");

            await _context.ServiceOfferings.AddRangeAsync(
                new ServiceOffering
                {
                    ProviderId = providerId,
                    Place = giza,
                    Title = "Private Pyramids Tour",
                    Category = "Tour",
                    Description = "Guided private tour around the Giza plateau with photo stops.",
                    Price = 400,
                    Duration = "4 hours",
                    LocationName = "Giza, Egypt",
                    ImageUrl = giza.ImageUrl,
                    Availability = "Daily",
                    Rating = 4.9m,
                    BookingCount = 45
                },
                new ServiceOffering
                {
                    ProviderId = providerId,
                    Place = nile,
                    Title = "Nile Sunset",
                    Category = "Experience",
                    Description = "Relaxing Nile sunset experience with local refreshments.",
                    Price = 350,
                    Duration = "3 hours",
                    LocationName = "Aswan, Egypt",
                    ImageUrl = nile.ImageUrl,
                    Availability = "Weekends",
                    Rating = 4.8m,
                    BookingCount = 32
                },
                new ServiceOffering
                {
                    ProviderId = providerId,
                    Place = luxor,
                    Title = "Luxor Temple",
                    Category = "Tour",
                    Description = "Evening visit to Luxor Temple with a licensed guide.",
                    Price = 400,
                    Duration = "2 hours",
                    LocationName = "Luxor, Egypt",
                    ImageUrl = luxor.ImageUrl,
                    Availability = "Daily",
                    Rating = 4.7m,
                    BookingCount = 28
                });

            await _context.SaveChangesAsync();
        }
    }
}
