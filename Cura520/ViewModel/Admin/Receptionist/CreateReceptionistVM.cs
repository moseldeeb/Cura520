using Cura520.Validations;
using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Admin.Receptionist
{
    public class CreateReceptionistVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [AllowedExtentions([ ".png", ".jpg", ".jpeg", ".gif" ])]
        public IFormFile ImageFile { get; set; }
        public string? Img { get; set; }
        public string PhoneNumber { get; set; }
    }
}