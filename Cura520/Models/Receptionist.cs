namespace Cura520.Models
{
    public class Receptionist
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Img { get; set; } = "defaultImg.png";
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        public string? PhoneNumber { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Optional: Track which invoices this receptionist handled
        public ICollection<Invoice>? ManagedInvoices { get; set; }
    }
}
