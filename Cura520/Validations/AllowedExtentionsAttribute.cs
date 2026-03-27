using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
            if (value is IFormFile FormImg)
            {
                var ImgExtention = Path.GetExtension(FormImg.FileName).ToLower(); 
                if (AllowedExtentions.Contains(ImgExtention))
                {
                    return true; 
                }
            }
            return false; 
        }
    }
}
