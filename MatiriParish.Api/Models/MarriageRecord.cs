using System;

namespace MatiriParish.Api.Models
{
    public class MarriageRecord
    {
        public int Id { get; set; }
        public DateTime DateOfMarriage { get; set; }
        public string OfficiatingPriest { get; set; } = string.Empty;

        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        public int GroomId { get; set; }
        public Person Groom { get; set; } = null!;

        public int BrideId { get; set; }
        public Person Bride { get; set; } = null!;

        public string Witness1Name { get; set; } = string.Empty;
        public string Witness2Name { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string PrayerHouse { get; set; } = string.Empty;
    }
}
