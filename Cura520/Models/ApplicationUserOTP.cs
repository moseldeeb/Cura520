namespace Cura520.Models
{
    public class ApplicationUserOTP
    {
        public string Id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsValid { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public  ApplicationUser ApplicationUser { get; set; }

        public ApplicationUserOTP()
        {
            
        }   
        public ApplicationUserOTP(string OTP ,  string ApplicationUserId)
        {
            this.Id = Guid.NewGuid().ToString();
            this.ExpireAt = DateTime.UtcNow.AddMinutes(10);
            this.CreatedAt = DateTime.UtcNow;
            this.IsValid = true;
            this.OTP = OTP;
            this.ApplicationUserId = ApplicationUserId;
        }


    }
}
