using Ecommerce.Validations;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModel
{
    public class UpdateBrandVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [MinLength(3)]
        [MaxLength(20)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        public bool Status { get; set; }
        public string Img { get; set; } = "defaultImg.png";

        [AllowedExtentions(new[] { ".png" ,".jpg" , ".jpeg" , ".gif"})]
        public IFormFile? FormImg { get; set; }
    }
}
