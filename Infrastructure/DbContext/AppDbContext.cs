using Domain.Models;
using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DbContext
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Place> Places => Set<Place>();
        public DbSet<ServiceOffering> ServiceOfferings => Set<ServiceOffering>();
        public DbSet<SavedPlace> SavedPlaces => Set<SavedPlace>();
        public DbSet<VisitedPlace> VisitedPlaces => Set<VisitedPlace>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<TripDay> TripDays => Set<TripDay>();
        public DbSet<TripActivity> TripActivities => Set<TripActivity>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<PlaceReview> PlaceReviews => Set<PlaceReview>();
        public DbSet<TripReview> TripReviews => Set<TripReview>();

        // New entities for Hotels, Transport, Programs, Guides
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Transport> Transports => Set<Transport>();
        public DbSet<Program> Programs => Set<Program>();
        public DbSet<Guide> Guides => Set<Guide>();

        // Chat messages
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        // Booking entities
        public DbSet<HotelBooking> HotelBookings => Set<HotelBooking>();
        public DbSet<TransportBooking> TransportBookings => Set<TransportBooking>();
        public DbSet<ProgramBooking> ProgramBookings => Set<ProgramBooking>();
        public DbSet<GuideBooking> GuideBookings => Set<GuideBooking>();

        // Provider entities
        public DbSet<ProviderRequest> ProviderRequests => Set<ProviderRequest>();
        public DbSet<ProviderEarnings> ProviderEarnings => Set<ProviderEarnings>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Place>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(160).IsRequired();
                entity.Property(p => p.Category).HasMaxLength(80).IsRequired();
                entity.Property(p => p.LocationName).HasMaxLength(180).IsRequired();
                entity.Property(p => p.City).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Country).HasMaxLength(80).IsRequired();
                entity.Property(p => p.ImageUrl).HasMaxLength(500);
                entity.Property(p => p.OpeningHours).HasMaxLength(120);
                entity.Property(p => p.Rating).HasPrecision(3, 2);
                entity.Property(p => p.PriceFrom).HasPrecision(18, 2);
                entity.Property(p => p.DistanceKm).HasPrecision(8, 2);
                entity.Property(p => p.Latitude).HasPrecision(10, 7);
                entity.Property(p => p.Longitude).HasPrecision(10, 7);
            });

            builder.Entity<ServiceOffering>(entity =>
            {
                entity.Property(s => s.Title).HasMaxLength(180).IsRequired();
                entity.Property(s => s.Category).HasMaxLength(80).IsRequired();
                entity.Property(s => s.Currency).HasMaxLength(12).IsRequired();
                entity.Property(s => s.StartDateTime).IsRequired();
                entity.Property(s => s.EndDateTime).IsRequired();
                entity.Property(s => s.LocationName).HasMaxLength(180);
                entity.Property(s => s.ImageUrl).HasMaxLength(500);
                entity.Property(s => s.Price).HasPrecision(18, 2);
                entity.Property(s => s.Rating).HasPrecision(3, 2);
                entity.HasOne(s => s.Provider)
                    .WithMany()
                    .HasForeignKey(s => s.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(s => s.Place)
                    .WithMany(p => p.Services)
                    .HasForeignKey(s => s.PlaceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<SavedPlace>(entity =>
            {
                entity.HasIndex(s => new { s.UserId, s.PlaceId }).IsUnique();
                entity.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(s => s.Place).WithMany(p => p.SavedByUsers).HasForeignKey(s => s.PlaceId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<VisitedPlace>(entity =>
            {
                entity.HasIndex(v => new { v.UserId, v.PlaceId }).IsUnique();
                entity.HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Place).WithMany(p => p.VisitedByUsers).HasForeignKey(v => v.PlaceId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Trip>(entity =>
            {
                entity.Property(t => t.Title).HasMaxLength(160).IsRequired();
                entity.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TripDay>(entity =>
            {
                entity.HasIndex(d => new { d.TripId, d.DayNumber }).IsUnique();
                entity.HasOne(d => d.Trip).WithMany(t => t.Days).HasForeignKey(d => d.TripId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TripActivity>(entity =>
            {
                entity.Property(a => a.Title).HasMaxLength(180).IsRequired();
                entity.HasOne(a => a.TripDay).WithMany(d => d.Activities).HasForeignKey(a => a.TripDayId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.Place).WithMany().HasForeignKey(a => a.PlaceId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(a => a.ServiceOffering).WithMany().HasForeignKey(a => a.ServiceOfferingId).OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Booking>(entity =>
            {
                entity.Property(b => b.Status).HasMaxLength(40).IsRequired();
                entity.Property(b => b.TotalPrice).HasPrecision(18, 2);
                entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.ServiceOffering).WithMany(s => s.Bookings).HasForeignKey(b => b.ServiceOfferingId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PlaceReview>(entity =>
            {
                entity.Property(r => r.Comment).HasMaxLength(1000);
                entity.HasIndex(r => new { r.UserId, r.PlaceId }).IsUnique();
                entity.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(r => r.Place).WithMany(p => p.Reviews).HasForeignKey(r => r.PlaceId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TripReview>(entity =>
            {
                entity.Property(r => r.Comment).HasMaxLength(1000);
                entity.HasIndex(r => new { r.UserId, r.TripId }).IsUnique();
                entity.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(r => r.Trip).WithMany(t => t.Reviews).HasForeignKey(r => r.TripId).OnDelete(DeleteBehavior.Cascade);
            });

            // Hotel configuration
            builder.Entity<Hotel>(entity =>
            {
                entity.Property(h => h.Name).HasMaxLength(200).IsRequired();
                entity.Property(h => h.Location).HasMaxLength(200).IsRequired();
                entity.Property(h => h.City).HasMaxLength(100).IsRequired();
                entity.Property(h => h.Country).HasMaxLength(100);
                entity.Property(h => h.Description).HasMaxLength(2000);
                entity.Property(h => h.ImageUrl).HasMaxLength(500);
                entity.Property(h => h.PricePerNight).HasPrecision(18, 2);
                entity.Property(h => h.Rating).HasPrecision(3, 2);
                entity.Property(h => h.Amenities).HasMaxLength(1000);
                entity.Property(h => h.ContactNumber).HasMaxLength(20);
                entity.Property(h => h.Email).HasMaxLength(255);
                entity.HasOne(h => h.Provider).WithMany().HasForeignKey(h => h.ProviderId).OnDelete(DeleteBehavior.Restrict);
            });

            // Transport configuration
            builder.Entity<Transport>(entity =>
            {
                entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
                entity.Property(t => t.Type).HasMaxLength(100).IsRequired();
                entity.Property(t => t.Description).HasMaxLength(2000);
                entity.Property(t => t.ImageUrl).HasMaxLength(500);
                entity.Property(t => t.DepartureLocation).HasMaxLength(200).IsRequired();
                entity.Property(t => t.ArrivalLocation).HasMaxLength(200).IsRequired();
                entity.Property(t => t.DepartureTime).HasMaxLength(20);
                entity.Property(t => t.ArrivalTime).HasMaxLength(20);
                entity.Property(t => t.Price).HasPrecision(18, 2);
                entity.Property(t => t.Rating).HasPrecision(3, 2);
                entity.HasOne(t => t.Provider).WithMany().HasForeignKey(t => t.ProviderId).OnDelete(DeleteBehavior.Restrict);
            });

            // Program configuration
            builder.Entity<Program>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(2000);
                entity.Property(p => p.ImageUrl).HasMaxLength(500);
                entity.Property(p => p.Category).HasMaxLength(100);
                entity.Property(p => p.Location).HasMaxLength(200).IsRequired();
                entity.Property(p => p.City).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Country).HasMaxLength(100);
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.IncludedServices).HasMaxLength(1000);
                entity.Property(p => p.Rating).HasPrecision(3, 2);
                entity.HasOne(p => p.Provider).WithMany().HasForeignKey(p => p.ProviderId).OnDelete(DeleteBehavior.Restrict);
            });

            // Guide configuration
            builder.Entity<Guide>(entity =>
            {
                entity.Property(g => g.FullName).HasMaxLength(200).IsRequired();
                entity.Property(g => g.PhoneNumber).HasMaxLength(20).IsRequired();
                entity.Property(g => g.Email).HasMaxLength(255).IsRequired();
                entity.Property(g => g.Description).HasMaxLength(2000);
                entity.Property(g => g.Nationality).HasMaxLength(100);
                entity.Property(g => g.Languages).HasMaxLength(500);
                entity.Property(g => g.Specialization).HasMaxLength(200);
                entity.Property(g => g.ImageUrl).HasMaxLength(500);
                entity.Property(g => g.Bio).HasMaxLength(2000);
                entity.Property(g => g.PricePerDay).HasPrecision(18, 2);
                entity.Property(g => g.Rating).HasPrecision(3, 2);
                entity.HasOne(g => g.Provider).WithMany().HasForeignKey(g => g.ProviderId).OnDelete(DeleteBehavior.Restrict);
            });

            // HotelBooking configuration
            builder.Entity<HotelBooking>(entity =>
            {
                entity.Property(b => b.Status).HasMaxLength(40).IsRequired();
                entity.Property(b => b.TotalPrice).HasPrecision(18, 2);
                entity.Property(b => b.SpecialRequests).HasMaxLength(1000);
                entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.Hotel).WithMany(h => h.Bookings).HasForeignKey(b => b.HotelId).OnDelete(DeleteBehavior.Cascade);
            });

            // TransportBooking configuration
            builder.Entity<TransportBooking>(entity =>
            {
                entity.Property(b => b.Status).HasMaxLength(40).IsRequired();
                entity.Property(b => b.TotalPrice).HasPrecision(18, 2);
                entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.Transport).WithMany(t => t.Bookings).HasForeignKey(b => b.TransportId).OnDelete(DeleteBehavior.Cascade);
            });

            // ProgramBooking configuration
            builder.Entity<ProgramBooking>(entity =>
            {
                entity.Property(b => b.Status).HasMaxLength(40).IsRequired();
                entity.Property(b => b.TotalPrice).HasPrecision(18, 2);
                entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.Program).WithMany(p => p.Bookings).HasForeignKey(b => b.ProgramId).OnDelete(DeleteBehavior.Cascade);
            });

            // GuideBooking configuration
            builder.Entity<GuideBooking>(entity =>
            {
                entity.Property(b => b.Status).HasMaxLength(40).IsRequired();
                entity.Property(b => b.TotalPrice).HasPrecision(18, 2);
                entity.Property(b => b.SpecialRequests).HasMaxLength(1000);
                entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.Guide).WithMany(g => g.Bookings).HasForeignKey(b => b.GuideId).OnDelete(DeleteBehavior.Cascade);
            });

            // ProviderRequest configuration
            builder.Entity<ProviderRequest>(entity =>
            {
                entity.Property(r => r.BusinessName).HasMaxLength(200).IsRequired();
                entity.Property(r => r.BusinessType).HasMaxLength(100).IsRequired();
                entity.Property(r => r.BusinessDescription).HasMaxLength(2000);
                entity.Property(r => r.ContactNumber).HasMaxLength(20);
                entity.Property(r => r.Email).HasMaxLength(255);
                entity.Property(r => r.TaxNumber).HasMaxLength(50);
                entity.Property(r => r.RegistrationNumber).HasMaxLength(50);
                entity.Property(r => r.DocumentUrl).HasMaxLength(500);
                entity.Property(r => r.Status).HasMaxLength(40).IsRequired();
                entity.Property(r => r.RejectionReason).HasMaxLength(500);
                entity.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // ProviderEarnings configuration
            builder.Entity<ProviderEarnings>(entity =>
            {
                entity.Property(e => e.TotalEarnings).HasPrecision(18, 2);
                entity.Property(e => e.PendingEarnings).HasPrecision(18, 2);
                entity.Property(e => e.WithdrawnAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.ProviderId).IsUnique();
                entity.HasOne(e => e.Provider).WithMany().HasForeignKey(e => e.ProviderId).OnDelete(DeleteBehavior.Cascade);
            });

            // ChatMessage configuration
            builder.Entity<ChatMessage>(entity =>
            {
                entity.Property(c => c.Text).HasMaxLength(2000).IsRequired();
                entity.Property(c => c.SenderId).HasMaxLength(450).IsRequired();
                entity.Property(c => c.RecipientId).HasMaxLength(450);
                entity.Property(c => c.GroupName).HasMaxLength(200);
                entity.Property(c => c.SentAt).IsRequired();
            });
        }
    }
}
