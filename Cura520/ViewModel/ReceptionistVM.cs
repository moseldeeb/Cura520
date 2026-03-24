using Cura520.Models;
using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel
{
    public class ReceptionistVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public Receptionist Receptionist { get; set; } = new Receptionist();

    }
}