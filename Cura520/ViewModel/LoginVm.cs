using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel
{
    public class LoginVm
    {
        public int Id { get; set; }
        public string UserNameOrEmail { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
