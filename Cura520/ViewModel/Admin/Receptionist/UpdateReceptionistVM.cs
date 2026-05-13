using Cura520.Validations;
using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Admin.Receptionist
{
    public class UpdateReceptionistVM
    {
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }

        public string Img { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }

        [AllowedExtentions(new[] { ".png", ".jpg", ".jpeg", ".gif" })]
        public IFormFile? ImageFile { get; set; }
    }
}
