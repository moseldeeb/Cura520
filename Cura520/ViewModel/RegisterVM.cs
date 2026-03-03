using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel
{
    public class RegisterVM
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password) , Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
