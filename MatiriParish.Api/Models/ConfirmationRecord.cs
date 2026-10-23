using System;

namespace MatiriParish.Api.Models
{
    public class ConfirmationRecord
    {
        public int Id { get; set; }
        public DateTime DateOfConfirmation { get; set; }
        public string OfficiatingBishop { get; set; } = string.Empty;

        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        public string SponsorName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string PrayerHouse { get; set; } = string.Empty;

        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;
    }
}
