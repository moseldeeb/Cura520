namespace Cura520.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } // Points UP to the receipt

        public int MedicalServiceId { get; set; }
        public MedicalService MedicalService { get; set; } // Points TO the service definition

        public decimal UnitPrice { get; set; } // We save the price AT THE TIME of the visit
    }
}
