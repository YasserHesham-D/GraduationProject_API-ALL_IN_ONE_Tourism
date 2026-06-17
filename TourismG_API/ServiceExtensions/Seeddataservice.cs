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

                _logger.LogInformation("Clearing existing data...");
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

                // Clear Identity users
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserClaims");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserLogins");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserTokens");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserRoles");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUsers");

                _logger.LogInformation("Existing data cleared.");

                await CreateRolesAsync();

                // Create Users
                var admin = await CreateUserWithRoleAsync("admin@example.com", "Admin@123456", "admin", "Admin", "Egyptian");
                
                var provider1 = await CreateUserWithRoleAsync("provider1@example.com", "Provider@123456", "provider1", "Provider", "Egyptian");
                var provider2 = await CreateUserWithRoleAsync("provider2@example.com", "Provider@123456", "provider2", "Provider", "Egyptian");
                var provider3 = await CreateUserWithRoleAsync("provider3@example.com", "Provider@123456", "provider3", "Provider", "Egyptian");
                var provider4 = await CreateUserWithRoleAsync("provider4@example.com", "Provider@123456", "provider4", "Provider", "Egyptian");

                var customer1 = await CreateUserWithRoleAsync("customer1@example.com", "Customer@123456", "customer1", "Customer", "American");
                var customer2 = await CreateUserWithRoleAsync("customer2@example.com", "Customer@123456", "customer2", "Customer", "British");
                var customer3 = await CreateUserWithRoleAsync("customer3@example.com", "Customer@123456", "customer3", "Customer", "German");
                var customer4 = await CreateUserWithRoleAsync("customer4@example.com", "Customer@123456", "customer4", "Customer", "Egyptian");

                // Seed Places (4 rows)
                var places = new List<Place>
                {
                    new()
                    {
                        Name = "Great Pyramids of Giza",
                        Category = "Historical",
                        LocationName = "Al Haram, Giza Governorate, Egypt",
                        City = "Giza",
                        Country = "Egypt",
                        Description = "The oldest and largest of the nine pyramids in the Giza pyramid complex, and the only remaining wonder of the ancient world.",
                        ImageUrl = "https://images.unsplash.com/photo-1539650116574-75c0c6d73f6e",
                        Rating = 4.9m,
                        ReviewCount = 15420,
                        PriceFrom = 240,
                        OpeningHours = "08:00 AM - 05:00 PM",
                        DistanceKm = 15.0m,
                        Latitude = 29.9792m,
                        Longitude = 31.1342m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Karnak Temple Complex",
                        Category = "Historical",
                        LocationName = "Karnak, Luxor, Egypt",
                        City = "Luxor",
                        Country = "Egypt",
                        Description = "A vast open-air museum and the largest ancient religious site in the world, dedicated primarily to the Theban Triad.",
                        ImageUrl = "https://images.unsplash.com/photo-1590076247563-718b5ef87d55",
                        Rating = 4.8m,
                        ReviewCount = 9340,
                        PriceFrom = 300,
                        OpeningHours = "06:00 AM - 06:00 PM",
                        DistanceKm = 4.5m,
                        Latitude = 25.7188m,
                        Longitude = 32.6586m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Ras Mohamed National Park",
                        Category = "Nature",
                        LocationName = "Sharm El Sheikh, South Sinai, Egypt",
                        City = "Sharm El Sheikh",
                        Country = "Egypt",
                        Description = "A world-renowned marine sanctuary at the southern tip of the Sinai Peninsula, famous for its rich coral reefs and marine life.",
                        ImageUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5",
                        Rating = 4.7m,
                        ReviewCount = 4120,
                        PriceFrom = 150,
                        OpeningHours = "08:00 AM - 05:00 PM",
                        DistanceKm = 12.0m,
                        Latitude = 27.7288m,
                        Longitude = 34.2536m,
                        IsRecommended = true,
                        IsPopular = true
                    },
                    new()
                    {
                        Name = "Khan El-Khalili Bazaar",
                        Category = "Cultural",
                        LocationName = "El-Gamaleya, Cairo, Egypt",
                        City = "Cairo",
                        Country = "Egypt",
                        Description = "A vibrant historic souq in the center of Islamic Cairo, packed with colorful lamps, spices, jewelry, and traditional crafts.",
                        ImageUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d",
                        Rating = 4.6m,
                        ReviewCount = 7850,
                        PriceFrom = 0,
                        OpeningHours = "09:00 AM - Midnight",
                        DistanceKm = 1.0m,
                        Latitude = 30.0478m,
                        Longitude = 31.2622m,
                        IsRecommended = true,
                        IsPopular = true
                    }
                };

                await _context.Places.AddRangeAsync(places);
                await _context.SaveChangesAsync();

                var place1 = places[0];
                var place2 = places[1];
                var place3 = places[2];
                var place4 = places[3];

                // Seed ServiceOfferings (4 rows)
                var serviceOfferings = new List<ServiceOffering>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        PlaceId = place1.Id,
                        Title = "Sunrise Pyramids Camel Trek & Breakfast",
                        Category = "Tours",
                        Description = "Ride a camel across the desert sands at sunrise with a professional guide, followed by a traditional Egyptian breakfast.",
                        Price = 650,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(5),
                        EndDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
                        LocationName = "Giza Plateau, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1539650116574-75c0c6d73f6e",
                        Rating = 4.9m,
                        BookingCount = 145,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        PlaceId = place2.Id,
                        Title = "Karnak Sound & Light Show VIP Ticket",
                        Category = "Shows & Entertainment",
                        Description = "Experience the history of ancient Thebes brought to life through a dramatic sound and light show at the Karnak Temple.",
                        Price = 800,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(19),
                        EndDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(21),
                        LocationName = "Karnak, Luxor, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1590076247563-718b5ef87d55",
                        Rating = 4.7m,
                        BookingCount = 92,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        PlaceId = place3.Id,
                        Title = "Ras Mohamed Guided Snorkeling Yacht Trip",
                        Category = "Water Sports",
                        Description = "Sail on a luxury yacht to Ras Mohamed National Park with stops at two premier snorkeling sites and an open buffet lunch.",
                        Price = 1200,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
                        EndDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(15),
                        LocationName = "Marina Sharm El Sheikh, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5",
                        Rating = 4.8m,
                        BookingCount = 110,
                        IsActive = true
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        PlaceId = place4.Id,
                        Title = "Islamic Cairo & Khan El-Khalili Street Food Tour",
                        Category = "Food & Drinks",
                        Description = "Taste authentic Egyptian street foods including Koshary, Falafel, and Molokhia while exploring Cairo's historic bazaar.",
                        Price = 500,
                        Currency = "EGP",
                        StartDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16),
                        EndDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(20),
                        LocationName = "Khan El-Khalili, Cairo, Egypt",
                        ImageUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d",
                        Rating = 4.6m,
                        BookingCount = 75,
                        IsActive = true
                    }
                };

                await _context.ServiceOfferings.AddRangeAsync(serviceOfferings);
                await _context.SaveChangesAsync();

                var service1 = serviceOfferings[0];
                var service2 = serviceOfferings[1];
                var service3 = serviceOfferings[2];
                var service4 = serviceOfferings[3];

                // Seed SavedPlaces (4 rows)
                var savedPlaces = new List<SavedPlace>
                {
                    new() { UserId = customer1.Id, PlaceId = place1.Id },
                    new() { UserId = customer2.Id, PlaceId = place2.Id },
                    new() { UserId = customer3.Id, PlaceId = place3.Id },
                    new() { UserId = customer4.Id, PlaceId = place4.Id }
                };
                await _context.SavedPlaces.AddRangeAsync(savedPlaces);

                // Seed VisitedPlaces (4 rows)
                var visitedPlaces = new List<VisitedPlace>
                {
                    new() { UserId = customer1.Id, PlaceId = place1.Id, VisitedAt = DateTime.UtcNow.AddDays(-10) },
                    new() { UserId = customer2.Id, PlaceId = place2.Id, VisitedAt = DateTime.UtcNow.AddDays(-20) },
                    new() { UserId = customer3.Id, PlaceId = place3.Id, VisitedAt = DateTime.UtcNow.AddDays(-5) },
                    new() { UserId = customer4.Id, PlaceId = place4.Id, VisitedAt = DateTime.UtcNow.AddDays(-1) }
                };
                await _context.VisitedPlaces.AddRangeAsync(visitedPlaces);

                // Seed Trips (4 rows)
                var trips = new List<Trip>
                {
                    new()
                    {
                        UserId = customer1.Id,
                        Title = "Family Egypt Exploration 2026",
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
                        Notes = "Must see the pyramids and take a camel ride."
                    },
                    new()
                    {
                        UserId = customer2.Id,
                        Title = "Luxor Ancient Temples Getaway",
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(23)),
                        Notes = "Focusing on historical sites and Sound & Light Show."
                    },
                    new()
                    {
                        UserId = customer3.Id,
                        Title = "Sinai Diving & Beach Holiday",
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12)),
                        Notes = "Snorkeling at Ras Mohamed is priority #1."
                    },
                    new()
                    {
                        UserId = customer4.Id,
                        Title = "Cairo Historical & Culinary Weekend",
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                        Notes = "Food tour at El-Khalili bazaar."
                    }
                };

                await _context.Trips.AddRangeAsync(trips);
                await _context.SaveChangesAsync();

                var trip1 = trips[0];
                var trip2 = trips[1];
                var trip3 = trips[2];
                var trip4 = trips[3];

                // Seed TripDays (4 rows)
                var tripDays = new List<TripDay>
                {
                    new() { TripId = trip1.Id, DayNumber = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) },
                    new() { TripId = trip2.Id, DayNumber = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)) },
                    new() { TripId = trip3.Id, DayNumber = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) },
                    new() { TripId = trip4.Id, DayNumber = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }
                };

                await _context.TripDays.AddRangeAsync(tripDays);
                await _context.SaveChangesAsync();

                var day1 = tripDays[0];
                var day2 = tripDays[1];
                var day3 = tripDays[2];
                var day4 = tripDays[3];

                // Seed TripActivities (4 rows)
                var tripActivities = new List<TripActivity>
                {
                    new()
                    {
                        TripDayId = day1.Id,
                        PlaceId = place1.Id,
                        ServiceOfferingId = service1.Id,
                        Title = "Camel Ride at the Pyramids",
                        ScheduledAt = new TimeOnly(6, 0),
                        Notes = "Bring sunscreen and camera."
                    },
                    new()
                    {
                        TripDayId = day2.Id,
                        PlaceId = place2.Id,
                        ServiceOfferingId = service2.Id,
                        Title = "Karnak Sound & Light Show",
                        ScheduledAt = new TimeOnly(19, 0),
                        Notes = "Dress warmly for the evening."
                    },
                    new()
                    {
                        TripDayId = day3.Id,
                        PlaceId = place3.Id,
                        ServiceOfferingId = service3.Id,
                        Title = "Snorkeling Yacht Cruise",
                        ScheduledAt = new TimeOnly(9, 0),
                        Notes = "Bring swimsuit and towel."
                    },
                    new()
                    {
                        TripDayId = day4.Id,
                        PlaceId = place4.Id,
                        ServiceOfferingId = service4.Id,
                        Title = "Street Food Crawl",
                        ScheduledAt = new TimeOnly(16, 0),
                        Notes = "Come hungry!"
                    }
                };
                await _context.TripActivities.AddRangeAsync(tripActivities);

                // Seed Bookings (4 rows)
                var bookings = new List<Booking>
                {
                    new()
                    {
                        UserId = customer1.Id,
                        ServiceOfferingId = service1.Id,
                        BookingDate = DateTime.UtcNow.AddDays(10),
                        Guests = 4,
                        TotalPrice = 2600,
                        Status = "confirmed"
                    },
                    new()
                    {
                        UserId = customer2.Id,
                        ServiceOfferingId = service2.Id,
                        BookingDate = DateTime.UtcNow.AddDays(20),
                        Guests = 2,
                        TotalPrice = 1600,
                        Status = "confirmed"
                    },
                    new()
                    {
                        UserId = customer3.Id,
                        ServiceOfferingId = service3.Id,
                        BookingDate = DateTime.UtcNow.AddDays(5),
                        Guests = 3,
                        TotalPrice = 3600,
                        Status = "pending"
                    },
                    new()
                    {
                        UserId = customer4.Id,
                        ServiceOfferingId = service4.Id,
                        BookingDate = DateTime.UtcNow.AddDays(1),
                        Guests = 2,
                        TotalPrice = 1000,
                        Status = "completed"
                    }
                };
                await _context.Bookings.AddRangeAsync(bookings);

                // Seed PlaceReviews (4 rows)
                var placeReviews = new List<PlaceReview>
                {
                    new() { UserId = customer1.Id, PlaceId = place1.Id, Rating = 5, Comment = "Incredible historical landmark! A must-visit." },
                    new() { UserId = customer2.Id, PlaceId = place2.Id, Rating = 5, Comment = "The pillars are awe-inspiring. Hire a guide to understand the details." },
                    new() { UserId = customer3.Id, PlaceId = place3.Id, Rating = 4, Comment = "Beautiful coral reefs, but it was a bit windy today." },
                    new() { UserId = customer4.Id, PlaceId = place4.Id, Rating = 5, Comment = "Loved the atmosphere, the spices, and the coffee shops." }
                };
                await _context.PlaceReviews.AddRangeAsync(placeReviews);

                // Seed TripReviews (4 rows)
                var tripReviews = new List<TripReview>
                {
                    new() { UserId = customer1.Id, TripId = trip1.Id, Rating = 5, Comment = "Excellent itinerary, kids loved it." },
                    new() { UserId = customer2.Id, TripId = trip2.Id, Rating = 4, Comment = "Very informative tour, but a lot of walking in the heat." },
                    new() { UserId = customer3.Id, TripId = trip3.Id, Rating = 5, Comment = "Amazing diving spots, saw a turtle!" },
                    new() { UserId = customer4.Id, TripId = trip4.Id, Rating = 5, Comment = "Wonderful combination of history and great food." }
                };
                await _context.TripReviews.AddRangeAsync(tripReviews);

                // Seed Hotels (4 rows)
                var hotels = new List<Hotel>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Marriott Mena House",
                        Location = "Pyramids Road, Giza",
                        City = "Giza",
                        Country = "Egypt",
                        Description = "A luxury 5-star hotel offering majestic views of the Pyramids of Giza, set in 40 acres of lush green gardens.",
                        ImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                        StarRating = 5,
                        PricePerNight = 8500m,
                        Rating = 4.9m,
                        ReviewCount = 3450,
                        AvailableRooms = 15,
                        Amenities = "Swimming Pool, Spa, Free WiFi, Fitness Center, Room Service, Pyramids View",
                        ContactNumber = "+20233839111",
                        Email = "menahouse.reservations@marriott.com"
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Sofitel Winter Palace",
                        Location = "Corniche El Nile Street, Luxor",
                        City = "Luxor",
                        Country = "Egypt",
                        Description = "A legendary 5-star palace hotel on the Nile River, hosting royalty and celebrities since 1907.",
                        ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                        StarRating = 5,
                        PricePerNight = 6200m,
                        Rating = 4.8m,
                        ReviewCount = 1980,
                        AvailableRooms = 20,
                        Amenities = "Historic Gardens, Outdoor Pool, Library Bar, French Dining, Free WiFi",
                        ContactNumber = "+20952380425",
                        Email = "h1661@sofitel.com"
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Four Seasons Resort Sharm El Sheikh",
                        Location = "1 Ra's Nasrani, Sharm El Sheikh",
                        City = "Sharm El Sheikh",
                        Country = "Egypt",
                        Description = "A hillside luxury resort cascading down to the Red Sea, with a private beach, diving center, and 5 pools.",
                        ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4",
                        StarRating = 5,
                        PricePerNight = 12000m,
                        Rating = 4.9m,
                        ReviewCount = 2150,
                        AvailableRooms = 12,
                        Amenities = "Private Beach, Reef Snorkeling, PADI Dive Center, Spa, 5 Pools, Kids Club",
                        ContactNumber = "+20693603555",
                        Email = "reservations.sharm@fourseasons.com"
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Steigenberger El Tahrir",
                        Location = "Kasr El Nil Street, Tahrir Square, Cairo",
                        City = "Cairo",
                        Country = "Egypt",
                        Description = "A modern 4-star hotel located in the heart of downtown Cairo, walking distance from the Egyptian Museum.",
                        ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                        StarRating = 4,
                        PricePerNight = 4500m,
                        Rating = 4.6m,
                        ReviewCount = 2890,
                        AvailableRooms = 30,
                        Amenities = "Outdoor Pool, Fitness Center, Lounge Bar, Meeting Rooms, Soundproof Rooms",
                        ContactNumber = "+20225750777",
                        Email = "cairo.tahrir@steigenberger.com"
                    }
                };

                await _context.Hotels.AddRangeAsync(hotels);
                await _context.SaveChangesAsync();

                var hotel1 = hotels[0];
                var hotel2 = hotels[1];
                var hotel3 = hotels[2];
                var hotel4 = hotels[3];

                // Seed Transports (4 rows)
                var transports = new List<Transport>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Abela Sleeping Train (Cairo - Luxor)",
                        Type = "Train",
                        Description = "Overnight sleeper train featuring double compartments, dinner, and breakfast served on board.",
                        ImageUrl = "https://images.unsplash.com/photo-1541427468627-a89a96e5ca1d",
                        DepartureLocation = "Ramses Railway Station, Cairo",
                        ArrivalLocation = "Luxor Station",
                        DepartureTime = "21:00",
                        ArrivalTime = "06:30",
                        Price = 1500,
                        AvailableSeats = 24,
                        TotalCapacity = 30,
                        Rating = 4.2m,
                        ReviewCount = 512
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "High-Speed Ferry (Hurghada - Sharm)",
                        Type = "Ferry",
                        Description = "Fast marine transit across the Red Sea, connecting the two major resort cities in 2.5 hours.",
                        ImageUrl = "https://images.unsplash.com/photo-1502784444185-5b6056a82f1f",
                        DepartureLocation = "Hurghada Port",
                        ArrivalLocation = "Sharm El Sheikh Marina",
                        DepartureTime = "08:30",
                        ArrivalTime = "11:00",
                        Price = 1200,
                        AvailableSeats = 45,
                        TotalCapacity = 80,
                        Rating = 4.4m,
                        ReviewCount = 320
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Nile Felucca Sailing Boat Tour",
                        Type = "Boat",
                        Description = "Traditional wooden sailing boat experience on the Nile, guided by a Nubian captain.",
                        ImageUrl = "https://images.unsplash.com/photo-1587975844610-40f1ad10d07e",
                        DepartureLocation = "Felucca Dock, Aswan",
                        ArrivalLocation = "Elephantine Island, Aswan",
                        DepartureTime = "16:00",
                        ArrivalTime = "18:00",
                        Price = 300,
                        AvailableSeats = 8,
                        TotalCapacity = 10,
                        Rating = 4.7m,
                        ReviewCount = 180
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Private Cairo Airport Limousine",
                        Type = "Car",
                        Description = "Luxury Mercedes E-Class private transfer from Cairo International Airport to any hotel in Giza or Cairo.",
                        ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341",
                        DepartureLocation = "Cairo International Airport",
                        ArrivalLocation = "Giza/Cairo Hotels",
                        DepartureTime = "24/7 On Demand",
                        ArrivalTime = "Flexible",
                        Price = 950,
                        AvailableSeats = 3,
                        TotalCapacity = 4,
                        Rating = 4.9m,
                        ReviewCount = 650
                    }
                };

                await _context.Transports.AddRangeAsync(transports);
                await _context.SaveChangesAsync();

                var transport1 = transports[0];
                var transport2 = transports[1];
                var transport3 = transports[2];
                var transport4 = transports[3];

                // Seed Programs (4 rows)
                var programs = new List<Domain.Models.Program>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        Name = "Pharaohs Odyssey: 7-Day Classic Tour",
                        Description = "A comprehensive guided package including Giza Pyramids, Egyptian Museum, and a 4-night 5-star Nile Cruise.",
                        ImageUrl = "https://images.unsplash.com/photo-1503177119275-0aa32b3a9368",
                        Category = "Classic Culture",
                        Location = "Cairo, Luxor, Aswan",
                        City = "Cairo",
                        Country = "Egypt",
                        Price = 18500,
                        Duration = 168,
                        MaxParticipants = 25,
                        AvailableSpots = 20,
                        IncludedServices = "5-Star Hotels, Nile Cruise, Entrance Fees, Egyptologist Guide, Domestic Flights",
                        Rating = 4.9m,
                        ReviewCount = 1420,
                        StartDate = DateTime.UtcNow.AddDays(14)
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        Name = "Sinai Desert Safari & Bedouin Camp",
                        Description = "4x4 Jeep adventure across the Sinai dunes, stargazing, camel riding, and a traditional Bedouin dinner under the stars.",
                        ImageUrl = "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                        Category = "Adventure & Safari",
                        Location = "Sinai Desert, Dahab",
                        City = "Dahab",
                        Country = "Egypt",
                        Price = 2800,
                        Duration = 24,
                        MaxParticipants = 15,
                        AvailableSpots = 12,
                        IncludedServices = "4x4 Transport, Bedouin Guides, Dinner & Tea, Hotel Transfers",
                        Rating = 4.7m,
                        ReviewCount = 540,
                        StartDate = DateTime.UtcNow.AddDays(7)
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        Name = "Red Sea PADI Open Water Diver Course",
                        Description = "Get certified to dive anywhere in the world! 4-day course with theoretical classroom lessons and open sea dives.",
                        ImageUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5",
                        Category = "Diving & Watersports",
                        Location = "Naama Bay, Sharm El Sheikh",
                        City = "Sharm El Sheikh",
                        Country = "Egypt",
                        Price = 9500,
                        Duration = 96,
                        MaxParticipants = 6,
                        AvailableSpots = 4,
                        IncludedServices = "PADI Instructors, Diving Gear, Certification Fees, Course Materials",
                        Rating = 4.8m,
                        ReviewCount = 290,
                        StartDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        Name = "Alexandria Day Tour from Cairo",
                        Description = "Full day excursion from Cairo to Alexandria, visiting the Catacombs of Kom El Shoqafa, Citadel of Qaitbay, and the Library of Alexandria.",
                        ImageUrl = "https://images.unsplash.com/photo-1589838082998-14c690c3936a",
                        Category = "Historical Excursion",
                        Location = "Alexandria, Egypt",
                        City = "Alexandria",
                        Country = "Egypt",
                        Price = 1600,
                        Duration = 12,
                        MaxParticipants = 20,
                        AvailableSpots = 18,
                        IncludedServices = "Air-Conditioned Bus, Licensed Guide, Seafood Lunch, Entrance Tickets",
                        Rating = 4.6m,
                        ReviewCount = 680,
                        StartDate = DateTime.UtcNow.AddDays(5)
                    }
                };

                await _context.Programs.AddRangeAsync(programs);
                await _context.SaveChangesAsync();

                var program1 = programs[0];
                var program2 = programs[1];
                var program3 = programs[2];
                var program4 = programs[3];

                // Seed Guides (4 rows)
                var guides = new List<Guide>
                {
                    new()
                    {
                        ProviderId = provider1.Id,
                        FullName = "Dr. Khaled Abdel-Moneim",
                        PhoneNumber = "+201001234567",
                        Email = "khaled.guide@tourism.eg",
                        Description = "Professional Egyptologist with a PhD in Ancient History from Cairo University.",
                        Nationality = "Egyptian",
                        Languages = "Arabic, English, German",
                        Specialization = "Pharaonic History & Archeology",
                        ImageUrl = "https://images.unsplash.com/photo-1560250097-0b93528c311a",
                        Bio = "Over 15 years guiding tourists through Egypt's ancient wonders. Passionate about storytelling and making history come alive.",
                        PricePerDay = 1500m,
                        Rating = 4.95m,
                        ReviewCount = 380,
                        IsAvailable = true
                    },
                    new()
                    {
                        ProviderId = provider2.Id,
                        FullName = "Marian Shenouda",
                        PhoneNumber = "+201229876543",
                        Email = "marian.guide@tourism.eg",
                        Description = "Expert guide specializing in Coptic & Islamic Cairo tours, and traditional local crafts.",
                        Nationality = "Egyptian",
                        Languages = "Arabic, English, French",
                        Specialization = "Cultural & Religious Heritage",
                        ImageUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2",
                        Bio = "Born and raised in Islamic Cairo. Marian loves introducing travelers to the hidden alleys, local artisans, and street foods of old Cairo.",
                        PricePerDay = 1200m,
                        Rating = 4.88m,
                        ReviewCount = 220,
                        IsAvailable = true
                    },
                    new()
                    {
                        ProviderId = provider3.Id,
                        FullName = "Youssef 'Joe' Bedouin",
                        PhoneNumber = "+201115556667",
                        Email = "joe.guide@tourism.eg",
                        Description = "Experienced desert safari navigator and Sinai wilderness specialist.",
                        Nationality = "Egyptian",
                        Languages = "Arabic, English, Italian",
                        Specialization = "Desert Stargazing & Hiking",
                        ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d",
                        Bio = "A local Sinai Bedouin with deep knowledge of desert geography, navigation, and survival. Known for making the best fire-roasted tea.",
                        PricePerDay = 1000m,
                        Rating = 4.9m,
                        ReviewCount = 150,
                        IsAvailable = true
                    },
                    new()
                    {
                        ProviderId = provider4.Id,
                        FullName = "Amr El-Gamil",
                        PhoneNumber = "+201064448889",
                        Email = "amr.guide@tourism.eg",
                        Description = "Certified marine biologist and Red Sea diving and snorkeling guide.",
                        Nationality = "Egyptian",
                        Languages = "Arabic, English, Russian",
                        Specialization = "Marine Biology & Snorkeling Tours",
                        ImageUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e",
                        Bio = "Spent the last 8 years diving and photographing marine ecosystems in Ras Mohamed. He loves showing guests the best coral gardens.",
                        PricePerDay = 1300m,
                        Rating = 4.75m,
                        ReviewCount = 190,
                        IsAvailable = true
                    }
                };

                await _context.Guides.AddRangeAsync(guides);
                await _context.SaveChangesAsync();

                var guide1 = guides[0];
                var guide2 = guides[1];
                var guide3 = guides[2];
                var guide4 = guides[3];

                // Seed HotelBookings (4 rows)
                var hotelBookings = new List<HotelBooking>
                {
                    new()
                    {
                        UserId = customer1.Id,
                        HotelId = hotel1.Id,
                        CheckInDate = DateTime.UtcNow.AddDays(10),
                        CheckOutDate = DateTime.UtcNow.AddDays(13),
                        NumberOfRooms = 2,
                        NumberOfGuests = 4,
                        TotalPrice = 51000m,
                        Status = "confirmed",
                        SpecialRequests = "Pyramids view room if possible."
                    },
                    new()
                    {
                        UserId = customer2.Id,
                        HotelId = hotel2.Id,
                        CheckInDate = DateTime.UtcNow.AddDays(20),
                        CheckOutDate = DateTime.UtcNow.AddDays(23),
                        NumberOfRooms = 1,
                        NumberOfGuests = 2,
                        TotalPrice = 18600m,
                        Status = "confirmed",
                        SpecialRequests = "Quiet room, anniversary trip."
                    },
                    new()
                    {
                        UserId = customer3.Id,
                        HotelId = hotel3.Id,
                        CheckInDate = DateTime.UtcNow.AddDays(5),
                        CheckOutDate = DateTime.UtcNow.AddDays(12),
                        NumberOfRooms = 1,
                        NumberOfGuests = 2,
                        TotalPrice = 84000m,
                        Status = "pending",
                        SpecialRequests = "Twin beds."
                    },
                    new()
                    {
                        UserId = customer4.Id,
                        HotelId = hotel4.Id,
                        CheckInDate = DateTime.UtcNow.AddDays(1),
                        CheckOutDate = DateTime.UtcNow.AddDays(3),
                        NumberOfRooms = 1,
                        NumberOfGuests = 2,
                        TotalPrice = 9000m,
                        Status = "completed",
                        SpecialRequests = "High floor."
                    }
                };
                await _context.HotelBookings.AddRangeAsync(hotelBookings);

                // Seed TransportBookings (4 rows)
                var transportBookings = new List<TransportBooking>
                {
                    new()
                    {
                        UserId = customer1.Id,
                        TransportId = transport1.Id,
                        TotalPrice = 6000m,
                        Status = "confirmed"
                    },
                    new()
                    {
                        UserId = customer2.Id,
                        TransportId = transport2.Id,
                        TotalPrice = 2400m,
                        Status = "confirmed"
                    },
                    new()
                    {
                        UserId = customer3.Id,
                        TransportId = transport3.Id,
                        TotalPrice = 600m,
                        Status = "pending"
                    },
                    new()
                    {
                        UserId = customer4.Id,
                        TransportId = transport4.Id,
                        TotalPrice = 1900m,
                        Status = "completed"
                    }
                };
                await _context.TransportBookings.AddRangeAsync(transportBookings);

                // Seed ProgramBookings (4 rows)
                var programBookings = new List<ProgramBooking>
                {
                    new()
                    {
                        UserId = customer1.Id,
                        ProgramId = program1.Id,
                        NumberOfParticipants = 4,
                        TotalPrice = 74000m,
                        Status = "confirmed",
                        BookingDate = DateTime.UtcNow.AddDays(14)
                    },
                    new()
                    {
                        UserId = customer2.Id,
                        ProgramId = program2.Id,
                        NumberOfParticipants = 2,
                        TotalPrice = 5600m,
                        Status = "confirmed",
                        BookingDate = DateTime.UtcNow.AddDays(7)
                    },
                    new()
                    {
                        UserId = customer3.Id,
                        ProgramId = program3.Id,
                        NumberOfParticipants = 2,
                        TotalPrice = 19000m,
                        Status = "pending",
                        BookingDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        UserId = customer4.Id,
                        ProgramId = program4.Id,
                        NumberOfParticipants = 2,
                        TotalPrice = 3200m,
                        Status = "completed",
                        BookingDate = DateTime.UtcNow.AddDays(5)
                    }
                };
                await _context.ProgramBookings.AddRangeAsync(programBookings);

                // Seed GuideBookings (4 rows)
                var guideBookings = new List<GuideBooking>
                {
                    new()
                    {
                        UserId = customer1.Id,
                        GuideId = guide1.Id,
                        StartDate = DateTime.UtcNow.AddDays(11),
                        EndDate = DateTime.UtcNow.AddDays(12),
                        NumberOfDays = 2,
                        NumberOfPeople = 4,
                        TotalPrice = 3000m,
                        Status = "confirmed",
                        SpecialRequests = "Need guidance in Giza."
                    },
                    new()
                    {
                        UserId = customer2.Id,
                        GuideId = guide2.Id,
                        StartDate = DateTime.UtcNow.AddDays(21),
                        EndDate = DateTime.UtcNow.AddDays(22),
                        NumberOfDays = 2,
                        NumberOfPeople = 2,
                        TotalPrice = 2400m,
                        Status = "confirmed",
                        SpecialRequests = "Luxor temple guidance."
                    },
                    new()
                    {
                        UserId = customer3.Id,
                        GuideId = guide3.Id,
                        StartDate = DateTime.UtcNow.AddDays(6),
                        EndDate = DateTime.UtcNow.AddDays(8),
                        NumberOfDays = 3,
                        NumberOfPeople = 2,
                        TotalPrice = 3000m,
                        Status = "pending",
                        SpecialRequests = "Desert stargazing trip."
                    },
                    new()
                    {
                        UserId = customer4.Id,
                        GuideId = guide4.Id,
                        StartDate = DateTime.UtcNow.AddDays(2),
                        EndDate = DateTime.UtcNow.AddDays(2),
                        NumberOfDays = 1,
                        NumberOfPeople = 2,
                        TotalPrice = 1300m,
                        Status = "completed",
                        SpecialRequests = "Snorkeling assistance."
                    }
                };
                await _context.GuideBookings.AddRangeAsync(guideBookings);

                // Seed ProviderRequests (4 rows)
                var providerRequests = new List<ProviderRequest>
                {
                    new()
                    {
                        UserId = provider1.Id,
                        BusinessName = "Giza Pyramids Tours & Travel",
                        BusinessType = "Tour Operator",
                        BusinessDescription = "Offering camel tours, day trips, and local guides.",
                        ContactNumber = "+201001234567",
                        Email = "provider1@example.com",
                        TaxNumber = "TAX111111",
                        RegistrationNumber = "REG111111",
                        DocumentUrl = "https://example.com/docs/giza_travel.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new()
                    {
                        UserId = provider2.Id,
                        BusinessName = "Luxor Ancient Heritage Services",
                        BusinessType = "Hotel & Cultural Tour Provider",
                        BusinessDescription = "Specialized in hotel bookings and temple sound & light shows.",
                        ContactNumber = "+201229876543",
                        Email = "provider2@example.com",
                        TaxNumber = "TAX222222",
                        RegistrationNumber = "REG222222",
                        DocumentUrl = "https://example.com/docs/luxor_heritage.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-25)
                    },
                    new()
                    {
                        UserId = provider3.Id,
                        BusinessName = "Red Sea Wonders Co.",
                        BusinessType = "Diving Center & Marine Transport",
                        BusinessDescription = "Luxury yacht charters and certified diving lessons.",
                        ContactNumber = "+201115556667",
                        Email = "provider3@example.com",
                        TaxNumber = "TAX333333",
                        RegistrationNumber = "REG333333",
                        DocumentUrl = "https://example.com/docs/redsea_wonders.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-20)
                    },
                    new()
                    {
                        UserId = provider4.Id,
                        BusinessName = "Old Cairo Culinary & Heritage Walks",
                        BusinessType = "Tour Guide Service",
                        BusinessDescription = "Guided food walks and historical walking tours.",
                        ContactNumber = "+201064448889",
                        Email = "provider4@example.com",
                        TaxNumber = "TAX444444",
                        RegistrationNumber = "REG444444",
                        DocumentUrl = "https://example.com/docs/oldcairo_walks.pdf",
                        Status = "Approved",
                        SubmittedAt = DateTime.UtcNow.AddDays(-15)
                    }
                };
                await _context.ProviderRequests.AddRangeAsync(providerRequests);

                // Seed ProviderEarnings (4 rows)
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
                await _context.ProviderEarnings.AddRangeAsync(providerEarnings);

                await _context.SaveChangesAsync();

                // Diagnostics: report row counts after seeding
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
                    var guidesCount = await _context.Guides.CountAsync();
                    var hotelBookingsCount = await _context.HotelBookings.CountAsync();
                    var transportBookingsCount = await _context.TransportBookings.CountAsync();
                    var programBookingsCount = await _context.ProgramBookings.CountAsync();
                    var guideBookingsCount = await _context.GuideBookings.CountAsync();
                    var requestsCount = await _context.ProviderRequests.CountAsync();
                    var earningsCount = await _context.ProviderEarnings.CountAsync();
                    var usersCount = await _context.Users.CountAsync();

                    _logger.LogInformation("DB counts after seeding: " +
                        "Places:{Places} Services:{Services} SavedPlaces:{SavedPlaces} VisitedPlaces:{VisitedPlaces} " +
                        "Trips:{Trips} TripDays:{TripDays} TripActivities:{TripActivities} Bookings:{Bookings} " +
                        "PlaceReviews:{PlaceReviews} TripReviews:{TripReviews} Hotels:{Hotels} Transports:{Transports} " +
                        "Programs:{Programs} Guides:{Guides} HotelBookings:{HotelBookings} TransportBookings:{TransportBookings} " +
                        "ProgramBookings:{ProgramBookings} GuideBookings:{GuideBookings} Requests:{Requests} Earnings:{Earnings} Users:{Users}",
                        placesCount, servicesCount, savedPlacesCount, visitedPlacesCount,
                        tripsCount, tripDaysCount, tripActivitiesCount, bookingsCount,
                        placeReviewsCount, tripReviewsCount, hotelsCount, transportsCount,
                        programsCount, guidesCount, hotelBookingsCount, transportBookingsCount,
                        programBookingsCount, guideBookingsCount, requestsCount, earningsCount, usersCount);
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
    }
}
