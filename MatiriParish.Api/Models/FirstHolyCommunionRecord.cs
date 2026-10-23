using System;

namespace MatiriParish.Api.Models
{
    public class FirstHolyCommunionRecord
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;

        public DateTime DateOfFirstCommunion { get; set; }
        public string OfficiatingPriest { get; set; } = string.Empty;

        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        public string PrayerHouse { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
