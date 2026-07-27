namespace MatiriParish.Registry.Models
{
    public class BaptismRecord
    {
        public int Id { get; set; }
        public DateTime DateOfBaptism { get; set; }
        public string OfficiatingPriest { get; set; } = string.Empty;

        // Ledger Reference
        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        // Godparents
        public string GodfatherName { get; set; } = string.Empty;
        public string GodmotherName { get; set; } = string.Empty;
        public string? Remarks { get; set; }

        // Foreign Key
        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;
    }
}