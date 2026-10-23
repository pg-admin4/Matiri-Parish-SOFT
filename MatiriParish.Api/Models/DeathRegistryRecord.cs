using System;

namespace MatiriParish.Api.Models
{
    public class DeathRegistryRecord
    {
        public int Id { get; set; }
        public int? PersonId { get; set; }
        public Person? Person { get; set; }

        public DateTime DateOfDeath { get; set; }
        public string PlaceOfDeath { get; set; } = string.Empty;
        public string CauseOfDeath { get; set; } = string.Empty;

        public string InformantName { get; set; } = string.Empty;

        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        public string? Remarks { get; set; }
    }
}
