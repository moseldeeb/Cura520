using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Identity
{
    public class ValidateOTPVM
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "OTP must contain only digits")]
        public string OTP { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "User ID is required")]
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
