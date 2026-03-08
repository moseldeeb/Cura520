using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel
{
    public class LoginVm
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Username or Email is required")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "Username or Email must be between 3 and 256 characters")]
        public string UserNameOrEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; }
    }
}
