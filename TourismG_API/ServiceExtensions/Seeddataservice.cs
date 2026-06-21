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
                _logger.LogInformation("Applying pending migrations (if any)...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migrations applied.");

                _logger.LogInformation("Clearing existing data (keeping Users and Roles)...");
                // Clear only tourism-related data, NOT users/roles
                await _context.PlaceReviews.ExecuteDeleteAsync();
                await _context.TripReviews.ExecuteDeleteAsync();
                await _context.SavedPlaces.ExecuteDeleteAsync();
                await _context.VisitedPlaces.ExecuteDeleteAsync();
                await _context.Bookings.ExecuteDeleteAsync();
                await _context.HotelBookings.ExecuteDeleteAsync();
                await _context.TransportBookings.ExecuteDeleteAsync();
                await _context.ProgramBookings.ExecuteDeleteAsync();
                await _context.GuideBookings.ExecuteDeleteAsync();
                await _context.TripActivities.ExecuteDeleteAsync();

                await _context.TripDays.ExecuteDeleteAsync();
                await _context.ServiceOfferings.ExecuteDeleteAsync();
                await _context.Hotels.ExecuteDeleteAsync();
                await _context.Transports.ExecuteDeleteAsync();
                await _context.Programs.ExecuteDeleteAsync();
                await _context.Guides.ExecuteDeleteAsync();

                await _context.Trips.ExecuteDeleteAsync();
                await _context.Places.ExecuteDeleteAsync();
                await _context.ProviderRequests.ExecuteDeleteAsync();
                await _context.ProviderEarnings.ExecuteDeleteAsync();

                // PRESERVE: Do NOT delete Identity users and roles
                // await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserClaims");
                // await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserLogins");
                // await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserTokens");
                // await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserRoles");
                // await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUsers");

                _logger.LogInformation("Tourism data cleared. Users and Roles preserved.");

                // STEP 1: Reassign any existing guide users (from before) to Provider role
                await ReassignGuideUsersToProviderRoleAsync();

                await CreateRolesAsync();

                // Create Users (or get existing ones) - NOT expanding user count per instructions
                var admin = await GetOrCreateUserAsync("admin@example.com", "Admin@123456", "admin", "Admin", "Egyptian");

                var provider1 = await GetOrCreateUserAsync("provider1@example.com", "Provider@123456", "provider1", "Provider", "Egyptian");
                var provider2 = await GetOrCreateUserAsync("provider2@example.com", "Provider@123456", "provider2", "Provider", "Egyptian");
                var provider3 = await GetOrCreateUserAsync("provider3@example.com", "Provider@123456", "provider3", "Provider", "Egyptian");
                var provider4 = await GetOrCreateUserAsync("provider4@example.com", "Provider@123456", "provider4", "Provider", "Egyptian");

                // Fetch guide users from database (they were reassigned to Provider role)
                var khaledGuide = await _userManager.FindByEmailAsync("khaled.guide@tourism.eg");
                var marianGuide = await _userManager.FindByEmailAsync("marian.guide@tourism.eg");
                var joeGuide = await _userManager.FindByEmailAsync("joe.guide@tourism.eg");
                var amrGuide = await _userManager.FindByEmailAsync("amr.guide@tourism.eg");

                _logger.LogInformation("Guide user IDs: Khaled={KhaledId}, Marian={MarianId}, Joe={JoeId}, Amr={AmrId}",
                    khaledGuide?.Id ?? "NOT_FOUND",
                    marianGuide?.Id ?? "NOT_FOUND",
                    joeGuide?.Id ?? "NOT_FOUND",
                    amrGuide?.Id ?? "NOT_FOUND");

                var customer1 = await GetOrCreateUserAsync("customer1@example.com", "Customer@123456", "customer1", "Customer", "American");
                var customer2 = await GetOrCreateUserAsync("customer2@example.com", "Customer@123456", "customer2", "Customer", "British");
                var customer3 = await GetOrCreateUserAsync("customer3@example.com", "Customer@123456", "customer3", "Customer", "German");
                var customer4 = await GetOrCreateUserAsync("customer4@example.com", "Customer@123456", "customer4", "Customer", "Egyptian");

                // Helper arrays to cycle through the 4 existing customers/providers when a table needs 10 rows
                var customers = new[] { customer1, customer2, customer3, customer4 };
                var providers = new[] { provider1, provider2, provider3, provider4 };

                // ============================================================
                // Seed Places - 10 Aswan attractions
                // ============================================================
                var places = new List<Place>
                {
                    new()
                    {
                        Name = "Abu Simbel Temples",
                        Category = "Historical",
                        LocationName = "Abu Simbel, Aswan Governorate, Egypt",
                        City = "Abu Simbel",
                        Country = "Egypt",
                        Description = "Two monumental temples carved out of the mountainside during the reign of Ramesses II. Famous for the solar alignment phenomenon that illuminates the inner sanctuary twice yearly.",
                        ImageUrl = "https://images.unsplash.com/photo-1539650116574-75c0c6d73f6e",
                        Rating = 4.95m,
                        ReviewCount = 8920,
                        PriceFrom = 500,
                        OpeningHours = "06:00 AM - 05:00 PM",
                        DistanceKm = 280.0m,
                        Latitude = 22.3412m,
                        Longitude = 31.6256m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Philae Temple",
                        Category = "Historical",
                        LocationName = "Philae Island, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Ancient Egyptian temple complex dedicated to Isis, relocated to Agilkia Island during the Aswan High Dam construction. A stunning example of Ptolemaic architecture.",
                        ImageUrl = "https://images.unsplash.com/photo-1590076247563-718b5ef87d55",
                        Rating = 4.9m,
                        ReviewCount = 7640,
                        PriceFrom = 250,
                        OpeningHours = "07:00 AM - 04:00 PM",
                        DistanceKm = 12.5m,
                        Latitude = 24.0225m,
                        Longitude = 32.8859m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Aswan Botanical Garden",
                        Category = "Nature",
                        LocationName = "Kitchener's Island, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Lush botanical garden on Kitchener's Island featuring rare plants and trees from around the world, with stunning Nile views.",
                        ImageUrl = "https://images.unsplash.com/photo-1504280390367-361c6d9f38f4",
                        Rating = 4.7m,
                        ReviewCount = 3420,
                        PriceFrom = 100,
                        OpeningHours = "08:00 AM - 05:00 PM",
                        DistanceKm = 3.0m,
                        Latitude = 24.0845m,
                        Longitude = 32.8954m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Nubian Museum",
                        Category = "Cultural",
                        LocationName = "Fatimid Island, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "World-class museum showcasing Nubian culture, history, and artifacts spanning from prehistoric times to the present day.",
                        ImageUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d",
                        Rating = 4.85m,
                        ReviewCount = 5120,
                        PriceFrom = 150,
                        OpeningHours = "09:00 AM - 09:00 PM",
                        DistanceKm = 5.0m,
                        Latitude = 24.0612m,
                        Longitude = 32.8754m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Aswan High Dam",
                        Category = "Landmark",
                        LocationName = "Aswan High Dam, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "One of the world's largest embankment dams, holding back Lake Nasser. A major feat of 20th-century engineering with a viewing platform and exhibits.",
                        ImageUrl = "https://images.unsplash.com/photo-1539768942893-daf53e448371",
                        Rating = 4.4m,
                        ReviewCount = 2980,
                        PriceFrom = 120,
                        OpeningHours = "07:00 AM - 04:00 PM",
                        DistanceKm = 13.0m,
                        Latitude = 23.9709m,
                        Longitude = 32.8775m,
                        IsRecommended = false,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Unfinished Obelisk",
                        Category = "Historical",
                        LocationName = "Northern Quarries, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "An enormous ancient granite obelisk abandoned in its quarry after cracks appeared during carving, offering insight into pharaonic stone-working techniques.",
                        ImageUrl = "https://images.unsplash.com/photo-1568322445389-f64ac2515020",
                        Rating = 4.5m,
                        ReviewCount = 2140,
                        PriceFrom = 100,
                        OpeningHours = "07:00 AM - 05:00 PM",
                        DistanceKm = 2.0m,
                        Latitude = 24.0764m,
                        Longitude = 32.8838m,
                        IsRecommended = false,
                        IsPopular = false
                    },
                    new()
                    {
                        Name = "Tombs of the Nobles (Qubbet el-Hawa)",
                        Category = "Historical",
                        LocationName = "West Bank, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Rock-cut tombs of ancient Aswan governors and dignitaries set into a hillside on the Nile's west bank, with panoramic views over the river.",
                        ImageUrl = "https://images.unsplash.com/photo-1591025207163-942350e47db2",
                        Rating = 4.6m,
                        ReviewCount = 1860,
                        PriceFrom = 100,
                        OpeningHours = "07:00 AM - 04:00 PM",
                        DistanceKm = 4.5m,
                        Latitude = 24.0916m,
                        Longitude = 32.8806m,
                        IsRecommended = true,
                        IsPopular = false
                    },
                    new()
                    {
                        Name = "Elephantine Island",
                        Category = "Cultural",
                        LocationName = "Elephantine Island, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Nile island home to Nubian villages, the ruins of the ancient city of Abu, and the Aswan Museum, reachable by a short ferry ride.",
                        ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                        Rating = 4.65m,
                        ReviewCount = 2650,
                        PriceFrom = 80,
                        OpeningHours = "08:00 AM - 05:00 PM",
                        DistanceKm = 1.5m,
                        Latitude = 24.0867m,
                        Longitude = 32.8869m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Monastery of St. Simeon",
                        Category = "Historical",
                        LocationName = "West Bank, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "One of the largest and best-preserved Coptic monasteries in Egypt, dating to the 7th century, set in the desert hills west of the Nile.",
                        ImageUrl = "https://images.unsplash.com/photo-1572252847-89ec1cb7a141",
                        Rating = 4.45m,
                        ReviewCount = 1320,
                        PriceFrom = 100,
                        OpeningHours = "08:00 AM - 04:00 PM",
                        DistanceKm = 6.0m,
                        Latitude = 24.1083m,
                        Longitude = 32.8761m,
                        IsRecommended = false,
                        IsPopular = false
                    },
                    new()
                    {
                        Name = "Sehel Island Nubian Village",
                        Category = "Cultural",
                        LocationName = "Sehel Island, Aswan, Egypt",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Colorful Nubian village on a Nile island near the First Cataract, known for ancient rock inscriptions and warm Nubian hospitality.",
                        ImageUrl = "https://images.unsplash.com/photo-1589838082998-14c690c3936a",
                        Rating = 4.75m,
                        ReviewCount = 1740,
                        PriceFrom = 150,
                        OpeningHours = "08:00 AM - 06:00 PM",
                        DistanceKm = 9.0m,
                        Latitude = 24.0258m,
                        Longitude = 32.8456m,
                        IsRecommended = true,
                        IsPopular = false
                    }
                };

                await _context.Places.AddRangeAsync(places);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed ServiceOfferings - 10 Aswan tours
                // ============================================================
                var serviceOfferings = new List<ServiceOffering>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        PlaceId = places[0].Id,
                        Title = "Abu Simbel Full Day Guided Tour with Breakfast",
                        Category = "Tours",
                        Description = "Early morning departure to Abu Simbel temples with a professional Egyptologist guide, includes hotel pickup, breakfast, and guided tour of both temples.",
                        Price = 1200,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(4),
                        EndDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(17),
                        LocationName = "Abu Simbel, Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1539650116574-75c0c6d73f6e",
                        Rating = 4.95m,
                        BookingCount = 280,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        PlaceId = places[1].Id,
                        Title = "Philae Temple Sunset Felucca Tour",
                        Category = "Tours",
                        Description = "Experience a traditional felucca sailboat tour to Philae Temple, including sunset views and a guided tour of the beautifully illuminated temple.",
                        Price = 450,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(15),
                        EndDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(19),
                        LocationName = "Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1590076247563-718b5ef87d55",
                        Rating = 4.9m,
                        BookingCount = 165,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        PlaceId = places[2].Id,
                        Title = "Botanical Garden & Kitchener's Island Tour",
                        Category = "Nature Tours",
                        Description = "Half-day guided tour of Kitchener's Botanical Garden with a professional botanist, exploring rare plants and enjoying panoramic Nile valley views.",
                        Price = 300,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
                        EndDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(12),
                        LocationName = "Kitchener's Island, Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1504280390367-361c6d9f38f4",
                        Rating = 4.7m,
                        BookingCount = 98,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        PlaceId = places[3].Id,
                        Title = "Nubian Museum & Cultural Heritage Walking Tour",
                        Category = "Cultural Tours",
                        Description = "Comprehensive museum tour followed by an exploration of Nubian villages, traditional crafts, and authentic Nubian cuisine experience.",
                        Price = 550,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(9),
                        EndDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(17),
                        LocationName = "Aswan Nubian Area, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d",
                        Rating = 4.85m,
                        BookingCount = 142,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        PlaceId = places[4].Id,
                        Title = "Aswan High Dam Engineering Tour",
                        Category = "Tours",
                        Description = "Guided visit to the Aswan High Dam viewing platform with a short documentary stop, explaining the dam's history and impact on Egypt.",
                        Price = 250,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(9),
                        EndDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(11),
                        LocationName = "Aswan High Dam, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1539768942893-daf53e448371",
                        Rating = 4.4m,
                        BookingCount = 120,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        PlaceId = places[5].Id,
                        Title = "Unfinished Obelisk Quarry Tour",
                        Category = "Historical Tours",
                        Description = "Short guided stop at the Northern Quarries to see the Unfinished Obelisk and learn about ancient Egyptian stone-cutting methods.",
                        Price = 200,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(11),
                        EndDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(12),
                        LocationName = "Northern Quarries, Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1568322445389-f64ac2515020",
                        Rating = 4.5m,
                        BookingCount = 90,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        PlaceId = places[6].Id,
                        Title = "Tombs of the Nobles Hillside Tour",
                        Category = "Historical Tours",
                        Description = "Cross the Nile to the West Bank and climb to the rock-cut Tombs of the Nobles with an Egyptologist explaining the wall paintings.",
                        Price = 350,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(4).Date.AddHours(8),
                        EndDateTime = DateTime.UtcNow.AddDays(4).Date.AddHours(11),
                        LocationName = "West Bank, Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1591025207163-942350e47db2",
                        Rating = 4.6m,
                        BookingCount = 75,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        PlaceId = places[7].Id,
                        Title = "Elephantine Island Ferry & Village Walk",
                        Category = "Cultural Tours",
                        Description = "Local ferry crossing to Elephantine Island followed by a walk through Nubian villages and a visit to the Aswan Museum ruins.",
                        Price = 280,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10),
                        EndDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(13),
                        LocationName = "Elephantine Island, Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                        Rating = 4.65m,
                        BookingCount = 160,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        PlaceId = places[8].Id,
                        Title = "Monastery of St. Simeon Desert Walk",
                        Category = "Historical Tours",
                        Description = "Camel or walking excursion through the desert hills to the ancient Monastery of St. Simeon, one of Egypt's best-preserved Coptic sites.",
                        Price = 320,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(5).Date.AddHours(8),
                        EndDateTime = DateTime.UtcNow.AddDays(5).Date.AddHours(12),
                        LocationName = "West Bank, Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1572252847-89ec1cb7a141",
                        Rating = 4.45m,
                        BookingCount = 65,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        PlaceId = places[9].Id,
                        Title = "Sehel Island Nubian Village Felucca Trip",
                        Category = "Cultural Tours",
                        Description = "Sail to Sehel Island to explore ancient rock inscriptions and enjoy a traditional Nubian lunch with a local family.",
                        Price = 500,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(6).Date.AddHours(9),
                        EndDateTime = DateTime.UtcNow.AddDays(6).Date.AddHours(15),
                        LocationName = "Sehel Island, Aswan, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1589838082998-14c690c3936a",
                        Rating = 4.75m,
                        BookingCount = 110,
                        IsActive = true
                    }
                };

                await _context.ServiceOfferings.AddRangeAsync(serviceOfferings);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed SavedPlaces (10 rows, cycling through 4 customers)
                // ============================================================
                var savedPlaces = new List<SavedPlace>();
                for (int i = 0; i < 10; i++)
                {
                    savedPlaces.Add(new() { UserId = customers[i % customers.Length].Id, PlaceId = places[i].Id });
                }
                await _context.SavedPlaces.AddRangeAsync(savedPlaces);

                // ============================================================
                // Seed VisitedPlaces (10 rows)
                // ============================================================
                var visitedPlaces = new List<VisitedPlace>();
                for (int i = 0; i < 10; i++)
                {
                    visitedPlaces.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        PlaceId = places[i].Id,
                        VisitedAt = DateTime.UtcNow.AddDays(-(i + 1) * 3)
                    });
                }
                await _context.VisitedPlaces.AddRangeAsync(visitedPlaces);

                // ============================================================
                // Seed Trips (10 Aswan-themed trips)
                // ============================================================
                var tripTemplates = new (string Title, string Notes, int StartOffset, int Days)[]
                {
                    ("Aswan Nubian Heritage Trip", "Felucca sailing and Nubian village visit.", 10, 4),
                    ("Abu Simbel Special Excursion", "Catching the solar alignment event.", 20, 2),
                    ("Philae Temple & Felucca Getaway", "Sunset sailing and temple tour.", 5, 3),
                    ("Aswan Family History Weekend", "Visiting the Nubian Museum and botanical garden.", 1, 2),
                    ("West Bank Tombs & Monastery Tour", "Tombs of the Nobles and St. Simeon's Monastery.", 14, 2),
                    ("Aswan High Dam & City Exploration", "Engineering history and Elephantine Island.", 18, 3),
                    ("Sehel Island Nubian Immersion", "Rock inscriptions and a traditional Nubian lunch.", 8, 2),
                    ("Aswan Photography Adventure", "Capturing sunrise at Abu Simbel and sunset at Philae.", 25, 4),
                    ("Aswan Relaxation & Culture Mix", "Felucca rides, museum visits, and quiet evenings by the Nile.", 30, 5),
                    ("Aswan Quick Layover Tour", "Hitting the highlights in a short stopover.", 22, 1)
                };

                var trips = new List<Trip>();
                for (int i = 0; i < tripTemplates.Length; i++)
                {
                    var t = tripTemplates[i];
                    trips.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        Title = t.Title,
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(t.StartOffset)),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(t.StartOffset + t.Days)),
                        Notes = t.Notes
                    });
                }

                await _context.Trips.AddRangeAsync(trips);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed TripDays (10 rows, one per trip)
                // ============================================================
                var tripDays = new List<TripDay>();
                for (int i = 0; i < trips.Count; i++)
                {
                    tripDays.Add(new()
                    {
                        TripId = trips[i].Id,
                        DayNumber = 1,
                        Date = trips[i].StartDate
                    });
                }

                await _context.TripDays.AddRangeAsync(tripDays);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed TripActivities (10 rows)
                // ============================================================
                var activityTemplates = new (string Title, TimeOnly Time, string Notes)[]
                {
                    ("Felucca Sail at Sunset", new TimeOnly(17, 30), "Bring a light jacket."),
                    ("Abu Simbel Sunrise Viewing", new TimeOnly(5, 30), "Arrive early to beat the crowds."),
                    ("Philae Temple Illuminated Tour", new TimeOnly(18, 0), "Bring a camera with low-light settings."),
                    ("Nubian Museum Guided Visit", new TimeOnly(10, 0), "Allow at least two hours."),
                    ("Tombs of the Nobles Climb", new TimeOnly(8, 0), "Wear comfortable walking shoes."),
                    ("Aswan High Dam Viewing Stop", new TimeOnly(9, 30), "Bring your passport for the checkpoint."),
                    ("Sehel Island Inscriptions Walk", new TimeOnly(10, 30), "Sun hat recommended."),
                    ("Botanical Garden Morning Stroll", new TimeOnly(8, 30), "Best light for photos is early morning."),
                    ("Elephantine Island Village Visit", new TimeOnly(11, 0), "Try the local hibiscus tea."),
                    ("Monastery of St. Simeon Desert Trek", new TimeOnly(7, 30), "Bring water and sun protection.")
                };

                var tripActivities = new List<TripActivity>();
                for (int i = 0; i < tripDays.Count; i++)
                {
                    var a = activityTemplates[i];
                    tripActivities.Add(new()
                    {
                        TripDayId = tripDays[i].Id,
                        PlaceId = places[i].Id,
                        ServiceOfferingId = serviceOfferings[i % serviceOfferings.Count].Id,
                        Title = a.Title,
                        ScheduledAt = a.Time,
                        Notes = a.Notes
                    });
                }
                await _context.TripActivities.AddRangeAsync(tripActivities);

                // ============================================================
                // Seed Bookings (10 rows)
                // ============================================================
                var statuses = new[] { "confirmed", "pending", "completed" };
                var bookings = new List<Booking>();
                for (int i = 0; i < 10; i++)
                {
                    var service = serviceOfferings[i % serviceOfferings.Count];
                    int guests = (i % 4) + 1;
                    bookings.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        ServiceOfferingId = service.Id,
                        BookingDate = DateTime.UtcNow.AddDays(i + 1),
                        Guests = guests,
                        TotalPrice = service.Price * guests,
                        Status = statuses[i % statuses.Length]
                    });
                }
                await _context.Bookings.AddRangeAsync(bookings);

                // ============================================================
                // Seed PlaceReviews (10 rows)
                // ============================================================
                var placeReviewComments = new[]
                {
                    "Incredible historical landmark! A must-visit.",
                    "The pillars are awe-inspiring. Hire a guide to understand the details.",
                    "Beautiful gardens, peaceful and relaxing.",
                    "Loved the atmosphere, the spices, and the coffee shops.",
                    "Impressive engineering, worth the short stop.",
                    "Fascinating to see how the obelisk was abandoned mid-carving.",
                    "Great views over the Nile from the tombs.",
                    "Lovely village walk, friendly locals.",
                    "Peaceful and historic, a hidden gem.",
                    "Authentic Nubian hospitality, loved the lunch."
                };
                var placeReviews = new List<PlaceReview>();
                for (int i = 0; i < 10; i++)
                {
                    placeReviews.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        PlaceId = places[i].Id,
                        Rating = 4 + (i % 2),
                        Comment = placeReviewComments[i]
                    });
                }
                await _context.PlaceReviews.AddRangeAsync(placeReviews);

                // ============================================================
                // Seed TripReviews (10 rows)
                // ============================================================
                var tripReviewComments = new[]
                {
                    "Nubian village visit was the highlight.",
                    "Watching the sunrise at Abu Simbel was magical.",
                    "Sailing to Philae at sunset was unforgettable.",
                    "Great mix of history and family-friendly activities.",
                    "Tombs and monastery made for a fascinating day.",
                    "Loved learning about the dam's history.",
                    "Sehel Island felt like stepping back in time.",
                    "Perfect for photography, light was stunning every day.",
                    "Relaxing pace, never felt rushed.",
                    "Short but packed with highlights."
                };
                var tripReviews = new List<TripReview>();
                for (int i = 0; i < 10; i++)
                {
                    tripReviews.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        TripId = trips[i].Id,
                        Rating = 4 + (i % 2),
                        Comment = tripReviewComments[i]
                    });
                }
                await _context.TripReviews.AddRangeAsync(tripReviews);

                // ============================================================
                // Seed Hotels - 10 Aswan accommodations
                // ============================================================
                var hotels = new List<Hotel>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Sofitel Legend Old Cataract Aswan",
                        Location = "Corniche El Nile Street, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Legendary 5-star palace hotel overlooking the Nile, hosting distinguished guests since 1899. Historic charm meets modern luxury.",
                        ImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                        StarRating = 5,
                        PricePerNight = 7500m,
                        Rating = 4.95m,
                        ReviewCount = 2850,
                        AvailableRooms = 18,
                        Amenities = "Nile River View, Historic Gardens, Outdoor Pool, Infinity Spa, French Cuisine Restaurant, Free WiFi",
                        ContactNumber = "+20973023000",
                        Email = "h5490@sofitel.com"
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Nile Cruise Resort Aswan",
                        Location = "Elephantine Island, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "All-inclusive resort experience on the Nile with access to private island, water sports, and daily excursions included in stay.",
                        ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                        StarRating = 5,
                        PricePerNight = 5200m,
                        Rating = 4.85m,
                        ReviewCount = 1620,
                        AvailableRooms = 25,
                        Amenities = "All-Inclusive, Private Beach, Water Sports, Daily Tours, Entertainment, Multiple Restaurants",
                        ContactNumber = "+20973222222",
                        Email = "reservations@nilecruiseaswan.com"
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Aswan Oberoi Luxury Resort",
                        Location = "Nobles Island, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Ultra-luxury resort with bungalow villas, private gardens, and exclusive access to ancient sites. Personalized butler service available.",
                        ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4",
                        StarRating = 5,
                        PricePerNight = 9800m,
                        Rating = 4.92m,
                        ReviewCount = 980,
                        AvailableRooms = 12,
                        Amenities = "Private Island, Butler Service, Spa, Fine Dining, Yacht Cruises, Helicopter Tours",
                        ContactNumber = "+20973903737",
                        Email = "reservations@oberoi-aswan.com"
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Aswan Nubian Village Resort",
                        Location = "West Bank, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Authentic Nubian-style resort offering cultural immersion with traditional architecture, local crafts workshops, and authentic cuisine.",
                        ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                        StarRating = 4,
                        PricePerNight = 2500m,
                        Rating = 4.7m,
                        ReviewCount = 1340,
                        AvailableRooms = 35,
                        Amenities = "Nubian Decor, Craft Workshops, Traditional Restaurant, Local Guide Services, BBQ Terrace",
                        ContactNumber = "+20973015555",
                        Email = "info@nubianvillage.com"
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Mövenpick Resort Aswan",
                        Location = "Elephantine Island, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Modern resort set on its own private island, offering panoramic Nile views and easy access to Aswan's main attractions by private boat.",
                        ImageUrl = "https://images.unsplash.com/photo-1564501049412-61c2a3083791",
                        StarRating = 5,
                        PricePerNight = 4800m,
                        Rating = 4.78m,
                        ReviewCount = 2100,
                        AvailableRooms = 30,
                        Amenities = "Private Island, Outdoor Pool, Spa, Free WiFi, Private Boat Shuttle",
                        ContactNumber = "+20973303455",
                        Email = "resort.aswan@movenpick.com"
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Basma Hotel Aswan",
                        Location = "El Fanadek Street, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Hilltop hotel with sweeping views over the Nile and the city, offering comfortable rooms at a more accessible price point.",
                        ImageUrl = "https://images.unsplash.com/photo-1551776235-dde6d4829808",
                        StarRating = 4,
                        PricePerNight = 2100m,
                        Rating = 4.4m,
                        ReviewCount = 1480,
                        AvailableRooms = 40,
                        Amenities = "Nile View, Outdoor Pool, Restaurant, Free WiFi, Garden",
                        ContactNumber = "+20973317332",
                        Email = "info@basmahotel.com"
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Pyramisa Isis Island Aswan",
                        Location = "Isis Island, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Resort hotel located on its own Nile island, featuring spacious rooms, multiple pools, and a quiet, scenic setting away from the city bustle.",
                        ImageUrl = "https://images.unsplash.com/photo-1571896349842-33c89424de2d",
                        StarRating = 5,
                        PricePerNight = 4200m,
                        Rating = 4.6m,
                        ReviewCount = 1760,
                        AvailableRooms = 28,
                        Amenities = "Private Island, Multiple Pools, Spa, Free WiFi, Boat Transfer",
                        ContactNumber = "+20973317901",
                        Email = "reservations@pyramisaisisisland.com"
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Sara Hotel Aswan",
                        Location = "Corniche El Nile Street, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Budget-friendly Nile-front hotel popular with backpackers and independent travelers, with a rooftop terrace overlooking the river.",
                        ImageUrl = "https://images.unsplash.com/photo-1564501049412-61c2a3083791",
                        StarRating = 3,
                        PricePerNight = 950m,
                        Rating = 4.2m,
                        ReviewCount = 980,
                        AvailableRooms = 20,
                        Amenities = "Nile View, Rooftop Terrace, Free WiFi, Airport Shuttle",
                        ContactNumber = "+20973304230",
                        Email = "info@sarahotelaswan.com"
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Nuba Nile Hotel & Spa",
                        Location = "Corniche El Nile Street, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Mid-range hotel blending Nubian design elements with modern comfort, featuring a riverside pool and on-site spa.",
                        ImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                        StarRating = 4,
                        PricePerNight = 2800m,
                        Rating = 4.5m,
                        ReviewCount = 1120,
                        AvailableRooms = 24,
                        Amenities = "Nile View, Spa, Outdoor Pool, Free WiFi, Restaurant",
                        ContactNumber = "+20973318800",
                        Email = "info@nubanilehotel.com"
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Philae Aswan Boutique Hotel",
                        Location = "Corniche El Nile Street, Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Description = "Small boutique hotel near the Philae boat dock, offering personalized service, Nubian-inspired décor, and a peaceful courtyard garden.",
                        ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                        StarRating = 4,
                        PricePerNight = 1900m,
                        Rating = 4.55m,
                        ReviewCount = 640,
                        AvailableRooms = 16,
                        Amenities = "Courtyard Garden, Free WiFi, Breakfast Included, Airport Shuttle",
                        ContactNumber = "+20973319944",
                        Email = "stay@philaeboutique.com"
                    }
                };

                await _context.Hotels.AddRangeAsync(hotels);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed Transports - 10 Aswan transport options
                // ============================================================
                var transports = new List<Transport>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Luxury Felucca Sail (Traditional Nile Boat)",
                        Type = "Boat",
                        Description = "Traditional Egyptian sailing boat experience on the Nile with expert Nubian captain, perfect for sunset tours.",
                        ImageUrl = "https://images.unsplash.com/photo-1587975844610-40f1ad10d07e",
                        DepartureLocation = "Aswan Corniche",
                        ArrivalLocation = "Elephantine Island / Kitchener's Island",
                        DepartureTime = "15:00",
                        ArrivalTime = "18:00",
                        Price = 400,
                        AvailableSeats = 12,
                        TotalCapacity = 15,
                        Rating = 4.9m,
                        ReviewCount = 720
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Abela Sleeping Train (Aswan - Abu Simbel)",
                        Type = "Train",
                        Description = "Overnight luxury sleeper train with en-suite cabins, dinner, and breakfast to Abu Simbel temples.",
                        ImageUrl = "https://images.unsplash.com/photo-1541427468627-a89a96e5ca1d",
                        DepartureLocation = "Aswan Railway Station",
                        ArrivalLocation = "Abu Simbel",
                        DepartureTime = "20:00",
                        ArrivalTime = "06:00",
                        Price = 1800,
                        AvailableSeats = 18,
                        TotalCapacity = 24,
                        Rating = 4.7m,
                        ReviewCount = 480
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Private Motorboat Charter (Nile Cruise)",
                        Type = "Boat",
                        Description = "Modern speedboat with air conditioning for comfortable river exploration and island hopping around Aswan.",
                        ImageUrl = "https://images.unsplash.com/photo-1502784444185-5b6056a82f1f",
                        DepartureLocation = "Aswan City Center",
                        ArrivalLocation = "Philae / Botanical Garden / Abu Simbel",
                        DepartureTime = "08:00",
                        ArrivalTime = "Flexible",
                        Price = 2500,
                        AvailableSeats = 6,
                        TotalCapacity = 8,
                        Rating = 4.85m,
                        ReviewCount = 340
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Air-Conditioned Minibus Tour Transport",
                        Type = "Car",
                        Description = "Comfortable minibus with professional English-speaking driver for daily tours to Abu Simbel, Nubian villages, and temples.",
                        ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341",
                        DepartureLocation = "Aswan Hotels",
                        ArrivalLocation = "Abu Simbel / Nubian Areas",
                        DepartureTime = "04:00",
                        ArrivalTime = "18:00",
                        Price = 800,
                        AvailableSeats = 10,
                        TotalCapacity = 12,
                        Rating = 4.75m,
                        ReviewCount = 560
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Aswan High Dam Shuttle Bus",
                        Type = "Car",
                        Description = "Shared shuttle bus running between central Aswan hotels and the Aswan High Dam viewing platform.",
                        ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341",
                        DepartureLocation = "Aswan Hotels",
                        ArrivalLocation = "Aswan High Dam",
                        DepartureTime = "09:00",
                        ArrivalTime = "11:00",
                        Price = 150,
                        AvailableSeats = 16,
                        TotalCapacity = 20,
                        Rating = 4.35m,
                        ReviewCount = 210
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Elephantine Island Local Ferry",
                        Type = "Boat",
                        Description = "Public ferry connecting the Aswan Corniche with Elephantine Island, the everyday way locals reach the Nubian villages.",
                        ImageUrl = "https://images.unsplash.com/photo-1502784444185-5b6056a82f1f",
                        DepartureLocation = "Aswan Corniche",
                        ArrivalLocation = "Elephantine Island",
                        DepartureTime = "07:00",
                        ArrivalTime = "07:10",
                        Price = 15,
                        AvailableSeats = 30,
                        TotalCapacity = 35,
                        Rating = 4.2m,
                        ReviewCount = 380
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "West Bank Donkey & Cart Village Tour",
                        Type = "Car",
                        Description = "Traditional donkey cart ride through West Bank villages near the Tombs of the Nobles and Monastery of St. Simeon.",
                        ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341",
                        DepartureLocation = "West Bank Landing",
                        ArrivalLocation = "Monastery of St. Simeon",
                        DepartureTime = "08:00",
                        ArrivalTime = "10:00",
                        Price = 180,
                        AvailableSeats = 4,
                        TotalCapacity = 6,
                        Rating = 4.5m,
                        ReviewCount = 150
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Camel Ride - West Bank Desert Trail",
                        Type = "Other",
                        Description = "Guided camel ride along the desert trail on Aswan's West Bank, passing the Tombs of the Nobles with Nile valley views.",
                        ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341",
                        DepartureLocation = "West Bank Landing",
                        ArrivalLocation = "Tombs of the Nobles",
                        DepartureTime = "07:30",
                        ArrivalTime = "09:30",
                        Price = 350,
                        AvailableSeats = 8,
                        TotalCapacity = 10,
                        Rating = 4.55m,
                        ReviewCount = 195
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Lake Nasser Cruise Tender Boat",
                        Type = "Boat",
                        Description = "Tender boat service for guests staying on Lake Nasser cruise ships, connecting to the Abu Simbel temple landing.",
                        ImageUrl = "https://images.unsplash.com/photo-1587975844610-40f1ad10d07e",
                        DepartureLocation = "Lake Nasser Cruise Ship",
                        ArrivalLocation = "Abu Simbel Landing",
                        DepartureTime = "06:30",
                        ArrivalTime = "07:00",
                        Price = 150,
                        AvailableSeats = 25,
                        TotalCapacity = 30,
                        Rating = 4.7m,
                        ReviewCount = 190
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Aswan Airport Private Transfer",
                        Type = "Car",
                        Description = "Private air-conditioned sedan transfer between Aswan International Airport and city hotels, with flight tracking included.",
                        ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341",
                        DepartureLocation = "Aswan International Airport",
                        ArrivalLocation = "Aswan City Hotels",
                        DepartureTime = "Flexible",
                        ArrivalTime = "Flexible",
                        Price = 300,
                        AvailableSeats = 3,
                        TotalCapacity = 4,
                        Rating = 4.6m,
                        ReviewCount = 260
                    }
                };

                await _context.Transports.AddRangeAsync(transports);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed Programs - 10 Aswan tour packages
                // ============================================================
                var programs = new List<Domain.Models.Program>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Ancient Nubia Explorer: 5-Day Comprehensive Package",
                        Description = "Complete Aswan experience including Abu Simbel temples, Philae Island, Nubian Museum, botanical gardens, and traditional felucca sailing.",
                        ImageUrl = "https://images.unsplash.com/photo-1503177119275-0aa32b3a9368",
                        Category = "Historical Tours",
                        Location = "Aswan, Abu Simbel",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 12500,
                        Duration = 120,
                        MaxParticipants = 15,
                        AvailableSpots = 12,
                        IncludedServices = "5-Star Hotel, Daily Tours, Professional Guide, All Entrance Fees, Meals",
                        Rating = 4.95m,
                        ReviewCount = 620,
                        StartDate = DateTime.UtcNow.AddDays(14)
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Abu Simbel Sunset & Nubian Village Experience",
                        Description = "3-day package combining Abu Simbel temple visit with authentic Nubian village homestay and traditional cooking experience.",
                        ImageUrl = "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                        Category = "Cultural Experience",
                        Location = "Abu Simbel, Nubian Villages",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 4800,
                        Duration = 72,
                        MaxParticipants = 10,
                        AvailableSpots = 8,
                        IncludedServices = "Hotel, Village Homestay, All Meals, Local Transport, English Guide",
                        Rating = 4.88m,
                        ReviewCount = 380,
                        StartDate = DateTime.UtcNow.AddDays(7)
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Philae & Botanical Garden Leisurely Day Tour",
                        Description = "Relaxed 1-day tour combining Philae Temple's ancient history with Kitchener's Botanical Garden's natural beauty and peaceful Nile sailing.",
                        ImageUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5",
                        Category = "Mixed Adventures",
                        Location = "Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 950,
                        Duration = 8,
                        MaxParticipants = 20,
                        AvailableSpots = 18,
                        IncludedServices = "Transport, Guide, Felucca Ride, Entrance Fees, Lunch",
                        Rating = 4.8m,
                        ReviewCount = 290,
                        StartDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Nubian Culture Immersion: 2-Day Workshop",
                        Description = "Deep dive into Nubian heritage with museum tours, traditional craft workshops, cooking classes, and stays in authentic Nubian accommodations.",
                        ImageUrl = "https://images.unsplash.com/photo-1589838082998-14c690c3936a",
                        Category = "Culture & Crafts",
                        Location = "Aswan Nubian Areas",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 2200,
                        Duration = 48,
                        MaxParticipants = 12,
                        AvailableSpots = 10,
                        IncludedServices = "2-Night Hotel, Museum Tour, Workshops, Meals, Local Transport",
                        Rating = 4.85m,
                        ReviewCount = 210,
                        StartDate = DateTime.UtcNow.AddDays(5)
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Aswan High Dam & West Bank Heritage Day",
                        Description = "Full day covering the Aswan High Dam, Unfinished Obelisk, and a West Bank visit to the Tombs of the Nobles.",
                        ImageUrl = "https://images.unsplash.com/photo-1539768942893-daf53e448371",
                        Category = "Historical Tours",
                        Location = "Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 1100,
                        Duration = 9,
                        MaxParticipants = 16,
                        AvailableSpots = 14,
                        IncludedServices = "Transport, Guide, Entrance Fees, Lunch",
                        Rating = 4.55m,
                        ReviewCount = 240,
                        StartDate = DateTime.UtcNow.AddDays(9)
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Elephantine Island & Nubian Villages Tour",
                        Description = "Ferry crossing to Elephantine Island with a guided walk through Nubian villages and the ancient ruins of Abu.",
                        ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                        Category = "Cultural Experience",
                        Location = "Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 700,
                        Duration = 6,
                        MaxParticipants = 18,
                        AvailableSpots = 15,
                        IncludedServices = "Ferry Tickets, Guide, Entrance Fees, Tea Stop",
                        Rating = 4.65m,
                        ReviewCount = 260,
                        StartDate = DateTime.UtcNow.AddDays(11)
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Sehel Island Nubian Lunch Experience",
                        Description = "Felucca sailing trip to Sehel Island to see ancient rock inscriptions, followed by a home-cooked Nubian lunch with a local family.",
                        ImageUrl = "https://images.unsplash.com/photo-1503177119275-0aa32b3a9368",
                        Category = "Culture & Crafts",
                        Location = "Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 1200,
                        Duration = 6,
                        MaxParticipants = 14,
                        AvailableSpots = 12,
                        IncludedServices = "Felucca, Guide, Lunch, Entrance Fees",
                        Rating = 4.75m,
                        ReviewCount = 175,
                        StartDate = DateTime.UtcNow.AddDays(13)
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Monastery of St. Simeon Desert Camel Trek",
                        Description = "Camel trek through the desert hills of the West Bank to the Monastery of St. Simeon, with views over the Nile valley.",
                        ImageUrl = "https://images.unsplash.com/photo-1572252847-89ec1cb7a141",
                        Category = "Mixed Adventures",
                        Location = "Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 850,
                        Duration = 5,
                        MaxParticipants = 10,
                        AvailableSpots = 8,
                        IncludedServices = "Camel Ride, Guide, Water, Entrance Fees",
                        Rating = 4.45m,
                        ReviewCount = 130,
                        StartDate = DateTime.UtcNow.AddDays(16)
                    },
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Grand Aswan Tour: 7-Day Complete Package",
                        Description = "Comprehensive journey through every major Aswan landmark — Abu Simbel, Philae, the High Dam, museums, tombs, and Nubian villages.",
                        ImageUrl = "https://images.unsplash.com/photo-1503177119275-0aa32b3a9368",
                        Category = "Multi-Site Tours",
                        Location = "Aswan, Abu Simbel",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 18500,
                        Duration = 168,
                        MaxParticipants = 14,
                        AvailableSpots = 9,
                        IncludedServices = "5-Star Hotel, Daily Tours, Guide, All Entrance Fees, Full Board, Felucca Cruise",
                        Rating = 4.93m,
                        ReviewCount = 410,
                        StartDate = DateTime.UtcNow.AddDays(28)
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Aswan Quick Layover Highlights Tour",
                        Description = "Compact half-day tour covering Philae Temple and the Unfinished Obelisk, perfect for cruise passengers or short stopovers.",
                        ImageUrl = "https://images.unsplash.com/photo-1590076247563-718b5ef87d55",
                        Category = "Day Trips",
                        Location = "Aswan",
                        City = "Aswan",
                        Country = "Egypt",
                        Price = 650,
                        Duration = 5,
                        MaxParticipants = 20,
                        AvailableSpots = 17,
                        IncludedServices = "Transport, Guide, Entrance Fees",
                        Rating = 4.5m,
                        ReviewCount = 195,
                        StartDate = DateTime.UtcNow.AddDays(4)
                    }
                };

                await _context.Programs.AddRangeAsync(programs);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed Guides (10 rows)
                // ============================================================
                var guides = new List<Guide>
                {
                    new()
                    {
                        FullName = "Khaled El-Sayed",
                        PhoneNumber = "+201001111111",
                        Email = "khaled.guide@tourism.eg",
                        Description = "Licensed Egyptologist with 15 years' experience leading archaeological tours.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "History,Archaeology",
                        ImageUrl = "https://images.unsplash.com/photo-1554151228-14d9def656e4",
                        Bio = "PhD in Egyptology; expert in Pharaonic temples and rituals.",
                        PricePerDay = 900m,
                        Rating = 4.95m,
                        ReviewCount = 134,
                        IsAvailable = true,
                        ProviderId = provider1.Id
                    },
                    new()
                    {
                        FullName = "Marian Hassan",
                        PhoneNumber = "+201002222222",
                        Email = "marian.guide@tourism.eg",
                        Description = "Professional photography guide specializing in scenic and cultural tours.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "Photography,Culture",
                        ImageUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2",
                        Bio = "Freelance photographer and guide with deep local knowledge.",
                        PricePerDay = 700m,
                        Rating = 4.88m,
                        ReviewCount = 98,
                        IsAvailable = true,
                        ProviderId = provider2.Id
                    },
                    new()
                    {
                        FullName = "Joe Karim",
                        PhoneNumber = "+201003333333",
                        Email = "joe.guide@tourism.eg",
                        Description = "Botany and nature guide with a passion for Nile ecology.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "Nature,Botany",
                        ImageUrl = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1",
                        Bio = "University-trained botanist providing insightful nature walks.",
                        PricePerDay = 650m,
                        Rating = 4.80m,
                        ReviewCount = 76,
                        IsAvailable = true,
                        ProviderId = provider3.Id
                    },
                    new()
                    {
                        FullName = "Amr Nasser",
                        PhoneNumber = "+201004444444",
                        Email = "amr.guide@tourism.eg",
                        Description = "Cultural storyteller and Nubian heritage specialist.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "Culture,Storytelling",
                        ImageUrl = "https://images.unsplash.com/photo-1547425260-76bcadfb4f2c",
                        Bio = "Local cultural expert focused on immersive experiences.",
                        PricePerDay = 600m,
                        Rating = 4.75m,
                        ReviewCount = 83,
                        IsAvailable = true,
                        ProviderId = provider4.Id
                    },
                    new()
                    {
                        FullName = "Aisha Saleh",
                        PhoneNumber = "+201005555555",
                        Email = "aisha.guide@tourism.eg",
                        Description = "Experienced guide for family-friendly and educational tours.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English,French",
                        Specialization = "Family,Education",
                        ImageUrl = "https://images.unsplash.com/photo-1545996124-1c3a5b8c9f6b",
                        Bio = "Background in museum education and children's programming.",
                        PricePerDay = 500m,
                        Rating = 4.70m,
                        ReviewCount = 64,
                        IsAvailable = true,
                        ProviderId = provider1.Id
                    },
                    new()
                    {
                        FullName = "Fatima El-Gohary",
                        PhoneNumber = "+201006666666",
                        Email = "fatima.guide@tourism.eg",
                        Description = "Local historian with a focus on Coptic and Islamic heritage.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "History,Religion",
                        ImageUrl = "https://images.unsplash.com/photo-1544006659-f0b21884ce1d",
                        Bio = "Researcher and lecturer with years of guiding experience.",
                        PricePerDay = 720m,
                        Rating = 4.82m,
                        ReviewCount = 71,
                        IsAvailable = true,
                        ProviderId = provider2.Id
                    },
                    new()
                    {
                        FullName = "Omar Abdel",
                        PhoneNumber = "+201007777777",
                        Email = "omar.guide@tourism.eg",
                        Description = "Adventure guide and expert on desert treks.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "Adventure,Desert",
                        ImageUrl = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d",
                        Bio = "Specializes in desert ecology and safe trekking.",
                        PricePerDay = 680m,
                        Rating = 4.68m,
                        ReviewCount = 58,
                        IsAvailable = true,
                        ProviderId = provider3.Id
                    },
                    new()
                    {
                        FullName = "Youssef Amr",
                        PhoneNumber = "+201008888888",
                        Email = "youssef.guide@tourism.eg",
                        Description = "Photography and cultural guide with local connections.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "Photography,Culture",
                        ImageUrl = "https://images.unsplash.com/photo-1547425260-76bcadfb4f2c",
                        Bio = "Works closely with local photographers to create unique itineraries.",
                        PricePerDay = 640m,
                        Rating = 4.66m,
                        ReviewCount = 49,
                        IsAvailable = true,
                        ProviderId = provider4.Id
                    },
                    new()
                    {
                        FullName = "Laila Mahmoud",
                        PhoneNumber = "+201009999999",
                        Email = "laila.guide@tourism.eg",
                        Description = "Specialist in culinary and market tours.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "Food,Culture",
                        ImageUrl = "https://images.unsplash.com/photo-1545996124-1c3a5b8c9f6b",
                        Bio = "Local food expert highlighting Nubian and Aswan cuisine.",
                        PricePerDay = 560m,
                        Rating = 4.60m,
                        ReviewCount = 45,
                        IsAvailable = true,
                        ProviderId = provider1.Id
                    },
                    new()
                    {
                        FullName = "Sameh Farouk",
                        PhoneNumber = "+201011000111",
                        Email = "sameh.guide@tourism.eg",
                        Description = "Seasoned guide specializing in family and small group tours.",
                        Nationality = "Egyptian",
                        Languages = "Arabic,English",
                        Specialization = "Family,SmallGroups",
                        ImageUrl = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1",
                        Bio = "Friendly professional with extensive local knowledge.",
                        PricePerDay = 600m,
                        Rating = 4.65m,
                        ReviewCount = 52,
                        IsAvailable = true,
                        ProviderId = provider2.Id
                    }
                };

                await _context.Guides.AddRangeAsync(guides);
                await _context.SaveChangesAsync();

                // ============================================================
                // Seed HotelBookings (10 rows)
                // ============================================================
                var hotelBookings = new List<HotelBooking>();
                var specialRequests = new[]
                {
                    "Nile view room if possible.", "Quiet room, anniversary trip.", "Twin beds.",
                    "High floor.", "Boat shuttle pickup requested.", "Early check-in if available.",
                    "Late check-out requested.", "Near elevator.", "Breakfast included please.",
                    "Airport pickup requested."
                };
                for (int i = 0; i < 10; i++)
                {
                    var hotel = hotels[i];
                    int nights = (i % 5) + 2;
                    int rooms = (i % 2) + 1;
                    hotelBookings.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        HotelId = hotel.Id,
                        CheckInDate = DateTime.UtcNow.AddDays(i + 2),
                        CheckOutDate = DateTime.UtcNow.AddDays(i + 2 + nights),
                        NumberOfRooms = rooms,
                        NumberOfGuests = rooms * 2,
                        TotalPrice = hotel.PricePerNight * nights * rooms,
                        Status = statuses[i % statuses.Length],
                        SpecialRequests = specialRequests[i]
                    });
                }
                await _context.HotelBookings.AddRangeAsync(hotelBookings);

                // ============================================================
                // Seed TransportBookings (10 rows)
                // ============================================================
                var transportBookings = new List<TransportBooking>();
                for (int i = 0; i < 10; i++)
                {
                    var transport = transports[i];
                    int seats = (i % 4) + 1;
                    transportBookings.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        TransportId = transport.Id,
                        TotalPrice = transport.Price * seats,
                        Status = statuses[i % statuses.Length]
                    });
                }
                await _context.TransportBookings.AddRangeAsync(transportBookings);

                // ============================================================
                // Seed ProgramBookings (10 rows)
                // ============================================================
                var programBookings = new List<ProgramBooking>();
                for (int i = 0; i < 10; i++)
                {
                    var program = programs[i];
                    int participants = (i % 4) + 1;
                    programBookings.Add(new()
                    {
                        UserId = customers[i % customers.Length].Id,
                        ProgramId = program.Id,
                        NumberOfParticipants = participants,
                        TotalPrice = program.Price * participants,
                        Status = statuses[i % statuses.Length],
                        BookingDate = program.StartDate
                    });
                }
                await _context.ProgramBookings.AddRangeAsync(programBookings);

                // NOTE: GuideBookings seeding removed - guides are no longer part of the system

                // ============================================================
                // Seed ProviderRequests - one per distinct provider/guide account
                // (capped at the number of provider-role users that exist; we are
                // not creating additional users per your instructions)
                // ============================================================
                var providerRequests = new List<ProviderRequest>
                {
                    new()
                    {
                        UserId = provider1.Id,
                        BusinessName = "Aswan Heritage Tours & Travel",
                        BusinessType = "Tour Operator",
                        BusinessDescription = "Offering temple tours, felucca rides, and Nubian village excursions around Aswan.",
                        ContactNumber = "+201001234567",
                        Email = "provider1@example.com",
                        TaxNumber = "TAX111111",
                        RegistrationNumber = "REG111111",
                        DocumentUrl = "https://example.com/docs/aswan_heritage.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new()
                    {
                        UserId = provider2.Id,
                        BusinessName = "Nile Cataract Travel Services",
                        BusinessType = "Hotel & Cultural Tour Provider",
                        BusinessDescription = "Specialized in Aswan hotel bookings, Philae tours, and Abu Simbel transport.",
                        ContactNumber = "+201229876543",
                        Email = "provider2@example.com",
                        TaxNumber = "TAX222222",
                        RegistrationNumber = "REG222222",
                        DocumentUrl = "https://example.com/docs/nile_cataract.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-25)
                    },
                    new()
                    {
                        UserId = provider3.Id,
                        BusinessName = "Aswan Nile Adventures Co.",
                        BusinessType = "Boat Charter & Excursion Provider",
                        BusinessDescription = "Private motorboat charters and guided excursions to Aswan's islands and West Bank sites.",
                        ContactNumber = "+201115556667",
                        Email = "provider3@example.com",
                        TaxNumber = "TAX333333",
                        RegistrationNumber = "REG333333",
                        DocumentUrl = "https://example.com/docs/aswan_adventures.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-20)
                    },
                    new()
                    {
                        UserId = provider4.Id,
                        BusinessName = "Nubian Heritage Walks Aswan",
                        BusinessType = "Tour Guide Service",
                        BusinessDescription = "Guided Nubian village walks and cultural craft workshops in the Aswan area.",
                        ContactNumber = "+201064448889",
                        Email = "provider4@example.com",
                        TaxNumber = "TAX444444",
                        RegistrationNumber = "REG444444",
                        DocumentUrl = "https://example.com/docs/nubian_walks.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-15)
                    }
                };

                if (khaledGuide != null)
                {
                    providerRequests.Add(new()
                    {
                        UserId = khaledGuide.Id,
                        BusinessName = "Dr. Khaled Egyptology Tours",
                        BusinessType = "Independent Tour Guide",
                        BusinessDescription = "Licensed Egyptologist offering specialized tours of Abu Simbel and Aswan's ancient sites.",
                        ContactNumber = "+201223334444",
                        Email = "khaled.guide@tourism.eg",
                        TaxNumber = "TAX555555",
                        RegistrationNumber = "REG555555",
                        DocumentUrl = "https://example.com/docs/khaled_guide.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-40)
                    });
                }

                if (marianGuide != null)
                {
                    providerRequests.Add(new()
                    {
                        UserId = marianGuide.Id,
                        BusinessName = "Marian Photography Tours",
                        BusinessType = "Independent Tour Guide",
                        BusinessDescription = "Photography-focused guided tours of Aswan landmarks.",
                        ContactNumber = "+201334445555",
                        Email = "marian.guide@tourism.eg",
                        TaxNumber = "TAX666666",
                        RegistrationNumber = "REG666666",
                        DocumentUrl = "https://example.com/docs/marian_guide.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-38)
                    });
                }

                if (joeGuide != null)
                {
                    providerRequests.Add(new()
                    {
                        UserId = joeGuide.Id,
                        BusinessName = "Joe's Nature & Botany Tours",
                        BusinessType = "Independent Tour Guide",
                        BusinessDescription = "Botanical and ecology-focused guided tours in Aswan.",
                        ContactNumber = "+201445556666",
                        Email = "joe.guide@tourism.eg",
                        TaxNumber = "TAX777777",
                        RegistrationNumber = "REG777777",
                        DocumentUrl = "https://example.com/docs/joe_guide.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-35)
                    });
                }

                if (amrGuide != null)
                {
                    providerRequests.Add(new()
                    {
                        UserId = amrGuide.Id,
                        BusinessName = "Amr Nubian Cultural Experiences",
                        BusinessType = "Independent Tour Guide",
                        BusinessDescription = "Authentic Nubian cultural immersion tours and storytelling experiences.",
                        ContactNumber = "+201556667777",
                        Email = "amr.guide@tourism.eg",
                        TaxNumber = "TAX888888",
                        RegistrationNumber = "REG888888",
                        DocumentUrl = "https://example.com/docs/amr_guide.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-33)
                    });
                }

                await _context.ProviderRequests.AddRangeAsync(providerRequests);

                // ============================================================
                // Seed ProviderEarnings - mirrors ProviderRequests (1 per provider account)
                // ============================================================
                var providerEarnings = new List<ProviderEarnings>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        TotalEarnings = 55000m,
                        PendingEarnings = 1500m,
                        WithdrawnAmount = 20000m,
                        CompletedBookings = 15,
                        LastUpdated = DateTime.UtcNow
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        TotalEarnings = 38000m,
                        PendingEarnings = 2800m,
                        WithdrawnAmount = 15000m,
                        CompletedBookings = 10,
                        LastUpdated = DateTime.UtcNow
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        TotalEarnings = 64000m,
                        PendingEarnings = 4500m,
                        WithdrawnAmount = 30000m,
                        CompletedBookings = 8,
                        LastUpdated = DateTime.UtcNow
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        TotalEarnings = 22000m,
                        PendingEarnings = 800m,
                        WithdrawnAmount = 10000m,
                        CompletedBookings = 12,
                        LastUpdated = DateTime.UtcNow
                    }
                };

                if (khaledGuide != null)
                {
                    providerEarnings.Add(new()
                    {
                        ProviderId = khaledGuide.Id,
                        TotalEarnings = 31000m,
                        PendingEarnings = 1200m,
                        WithdrawnAmount = 12000m,
                        CompletedBookings = 9,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                if (marianGuide != null)
                {
                    providerEarnings.Add(new()
                    {
                        ProviderId = marianGuide.Id,
                        TotalEarnings = 18500m,
                        PendingEarnings = 950m,
                        WithdrawnAmount = 8000m,
                        CompletedBookings = 6,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                if (joeGuide != null)
                {
                    providerEarnings.Add(new()
                    {
                        ProviderId = joeGuide.Id,
                        TotalEarnings = 14200m,
                        PendingEarnings = 600m,
                        WithdrawnAmount = 5000m,
                        CompletedBookings = 5,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                if (amrGuide != null)
                {
                    providerEarnings.Add(new()
                    {
                        ProviderId = amrGuide.Id,
                        TotalEarnings = 26800m,
                        PendingEarnings = 1100m,
                        WithdrawnAmount = 11000m,
                        CompletedBookings = 8,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                await _context.ProviderEarnings.AddRangeAsync(providerEarnings);

                await _context.SaveChangesAsync();

                try
                {
                    var placesCount = await _context.Places.CountAsync();
                    var servicesCount = await _context.ServiceOfferings.CountAsync();
                    var savedPlacesCount = await _context.SavedPlaces.CountAsync();
                    var visitedPlacesCount = await _context.VisitedPlaces.CountAsync();
                    var tripsCount = await _context.Trips.CountAsync();
                    var tripDaysCount = await _context.TripDays.CountAsync();
                    var tripActivitiesCount = await _context.TripActivities.CountAsync();
                    var bookingsCount = await _context.Bookings.CountAsync();
                    var placeReviewsCount = await _context.PlaceReviews.CountAsync();
                    var tripReviewsCount = await _context.TripReviews.CountAsync();
                    var hotelsCount = await _context.Hotels.CountAsync();
                    var transportsCount = await _context.Transports.CountAsync();
                    var programsCount = await _context.Programs.CountAsync();
                    var hotelBookingsCount = await _context.HotelBookings.CountAsync();
                    var transportBookingsCount = await _context.TransportBookings.CountAsync();
                    var programBookingsCount = await _context.ProgramBookings.CountAsync();
                    var requestsCount = await _context.ProviderRequests.CountAsync();
                    var earningsCount = await _context.ProviderEarnings.CountAsync();
                    var usersCount = await _context.Users.CountAsync();

                    _logger.LogInformation("DB counts after seeding: " +
                        "Places:{Places} Services:{Services} SavedPlaces:{SavedPlaces} VisitedPlaces:{VisitedPlaces} " +
                        "Trips:{Trips} TripDays:{TripDays} TripActivities:{TripActivities} Bookings:{Bookings} " +
                        "PlaceReviews:{PlaceReviews} TripReviews:{TripReviews} Hotels:{Hotels} Transports:{Transports} " +
                        "Programs:{Programs} HotelBookings:{HotelBookings} TransportBookings:{TransportBookings} " +
                        "ProgramBookings:{ProgramBookings} Requests:{Requests} Earnings:{Earnings} Users:{Users} " +
                        "(Note: Guides and GuideBookings removed from system)",
                        placesCount, servicesCount, savedPlacesCount, visitedPlacesCount,
                        tripsCount, tripDaysCount, tripActivitiesCount, bookingsCount,
                        placeReviewsCount, tripReviewsCount, hotelsCount, transportsCount,
                        programsCount, hotelBookingsCount, transportBookingsCount,
                        programBookingsCount, requestsCount, earningsCount, usersCount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read counts from database after seeding.");
                }

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

        private async Task ReassignGuideUsersToProviderRoleAsync()
        {
            try
            {
                // Known guide user emails that we're converting to Provider role
                var guideEmails = new[]
                {
                    "khaled.guide@tourism.eg",
                    "marian.guide@tourism.eg",
                    "joe.guide@tourism.eg",
                    "amr.guide@tourism.eg"
                };

                foreach (var email in guideEmails)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        // Check if user currently has Guide role
                        var hasGuideRole = await _userManager.IsInRoleAsync(user, "Guide");
                        if (hasGuideRole)
                        {
                            // Remove Guide role
                            await _userManager.RemoveFromRoleAsync(user, "Guide");
                            _logger.LogInformation($"Removed 'Guide' role from user '{email}'");
                        }

                        // Check if user already has Provider role
                        var hasProviderRole = await _userManager.IsInRoleAsync(user, "Provider");
                        if (!hasProviderRole)
                        {
                            // Add Provider role
                            await _userManager.AddToRoleAsync(user, "Provider");
                            _logger.LogInformation($"Added 'Provider' role to user '{email}'");
                        }
                        else
                        {
                            _logger.LogInformation($"User '{email}' already has 'Provider' role.");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"User '{email}' not found in system (may have been deleted earlier).");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reassigning guide users to Provider role.");
            }
        }

        private async Task<User> CreateUserWithRoleAsync(string email, string password, string username, string role, string nationality)
        {
            var user = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                Nationality = nationality
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                _logger.LogInformation("User '{Email}' created successfully with '{Role}' role.", email, role);
                return user;
            }
            else
            {
                var errors = FormatIdentityErrors(result);
                _logger.LogError("Failed to create user '{Email}'. Errors: {Errors}", email, errors);
                throw new InvalidOperationException($"Failed to create user '{email}'. Errors: {errors}");
            }
        }

        private static string FormatIdentityErrors(IdentityResult result)
        {
            return string.Join(", ", result.Errors.Select(e => e.Description));
        }

        private async Task<User> GetOrCreateUserAsync(string email, string password, string username, string role, string nationality)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogInformation("User '{Email}' already exists, skipping creation.", email);
                return existingUser;
            }

            // User doesn't exist, create them
            return await CreateUserWithRoleAsync(email, password, username, role, nationality);
        }
    }
}