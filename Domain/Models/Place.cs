namespace Domain.Models
{
    public class Place
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Egypt";
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string OpeningHours { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsRecommended { get; set; }
        public bool IsPopular { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServiceOffering> Services { get; set; } = new List<ServiceOffering>();
        public ICollection<SavedPlace> SavedByUsers { get; set; } = new List<SavedPlace>();
        public ICollection<VisitedPlace> VisitedByUsers { get; set; } = new List<VisitedPlace>();
        public ICollection<PlaceReview> Reviews { get; set; } = new List<PlaceReview>();
    }
}
