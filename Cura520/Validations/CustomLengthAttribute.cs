using System.ComponentModel.DataAnnotations;

namespace Cura520.Validations
{
    public class CustomLengthAttribute : ValidationAttribute
    {
        private int MnLength;
        private int MxLength; 
        public  CustomLengthAttribute (int MnLength  , int MxLength)
        {
            this.MnLength = MnLength;
            this.MxLength = MxLength;
        }
        public override bool IsValid(object? value)
        {
            if (value is string name )
            {
                return name.Length >= MnLength && name.Length <= MxLength; 
            }
            return false; 
        }
        public override string FormatErrorMessage(string name)
        {
            return $"the '{name}'  must be > {MnLength} and < {MxLength}"; 
        }
    }
}
