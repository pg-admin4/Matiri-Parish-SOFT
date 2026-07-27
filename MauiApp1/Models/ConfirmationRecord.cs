namespace MatiriParish.Registry.Models
{
    public class ConfirmationRecord
    {
        public int Id { get; set; }
        public DateTime DateOfConfirmation { get; set; }
        public string OfficiatingBishop { get; set; } = string.Empty;

        // Ledger Reference
        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        public string SponsorName { get; set; } = string.Empty;
        public string? Remarks { get; set; }

        // Foreign Key
        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;
    }
}
