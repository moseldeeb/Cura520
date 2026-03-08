using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel
{
    public class ForgetPasswordVM
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Username or Email is required")]
        [StringLength(256, MinimumLength = 3, 
            ErrorMessage = "Username or Email must be between 3 and 256 characters")]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }
}
