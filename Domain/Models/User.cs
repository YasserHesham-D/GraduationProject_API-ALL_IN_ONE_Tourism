using Microsoft.AspNetCore.Identity;


namespace Domain.Models
{
    public class User : IdentityUser
    {
        public string Nationality { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }
    }
}
