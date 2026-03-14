namespace Cura520.Models
{
    public class MedicalService
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal DefaultPrice { get; set; }
        public bool IsDeleted { get; set; } 
    }
}
