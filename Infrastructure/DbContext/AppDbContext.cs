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
                entity.Property(s => s.Duration).HasMaxLength(80);
                entity.Property(s => s.LocationName).HasMaxLength(180);
                entity.Property(s => s.ImageUrl).HasMaxLength(500);
                entity.Property(s => s.Availability).HasMaxLength(160);
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
        }
    }
}
