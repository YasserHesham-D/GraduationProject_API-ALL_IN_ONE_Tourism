using Microsoft.AspNetCore.Identity;


namespace Domain.Models
{
    public class User : IdentityUser
    {
        public string? Address { get; set; } 
    }
}
