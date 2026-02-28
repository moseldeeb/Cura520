using System.ComponentModel.DataAnnotations.Schema;

namespace Cura520.Models
{
    public enum PaymentMethod
    {
        Cash,
        Card,
        Insurance
    }
    public class Invoice
    {
        public int Id { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public bool IsPaid { get; set; }
        public DateTime IssuedDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
    }
}
