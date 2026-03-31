using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace Cura520.Validations
{
    public class AllowedExtentionsAttribute : ValidationAttribute
    {
        private string[] AllowedExtentions;

        public AllowedExtentionsAttribute(string[] AllowedExtentions)
        {
            this.AllowedExtentions = AllowedExtentions;
        }

        public override bool IsValid(object? value)
        {
            if (value is null) return true;
            if (value is IFormFile FormImg)
            {
                var ImgExtention = Path.GetExtension(FormImg.FileName).ToLower();
                return AllowedExtentions.Contains(ImgExtention);
            }
            return false; 
        }
    }
}
