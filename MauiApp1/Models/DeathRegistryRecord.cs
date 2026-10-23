using System;

namespace MauiApp1.Models
{
    public class DeathRegistryRecord
    {
        public int Id { get; set; }

        // Reference to central person profile (optional: deceased may not have Person entry)
        public int? PersonId { get; set; }
        public Person? Person { get; set; }

        public DateTime DateOfDeath { get; set; }
        public string PlaceOfDeath { get; set; } = string.Empty;
        public string CauseOfDeath { get; set; } = string.Empty;

        // Official informant
        public string InformantName { get; set; } = string.Empty;

        // Physical ledger reference
        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        public string? Remarks { get; set; }
    }
}
