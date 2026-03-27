using Cura520.Validations;

namespace Cura520.ViewModel.Admin.Receptionist
{
    public class UpdateReceptionistVM
    {
        public string? ApplicationUserId { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }

        [AllowedExtentions([ ".png", ".jpg", ".jpeg", ".gif" ])]
        public IFormFile ImageFile { get; set; }
        public string Img { get; set; }
    }
}
