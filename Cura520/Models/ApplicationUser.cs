using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cura520.Models
{
    public enum UserType
    {
        Patient,
        Doctor,
        Admin,
        Receptionist
    }
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(25)]
        public string FirstName { get; set; }
        [Required, MaxLength(25)]
        public string LastName { get; set; }
        [MaxLength(150)]
        public string? Address { get; set; }
        public UserType Type { get; set; }
    }
}
