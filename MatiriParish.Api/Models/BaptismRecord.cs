using System;

namespace MatiriParish.Api.Models
{
    public class BaptismRecord
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;

        public DateTime DateOfBaptism { get; set; }
        public string OfficiatingPriest { get; set; } = string.Empty;

        public string GodfatherName { get; set; } = string.Empty;
        public string GodmotherName { get; set; } = string.Empty;

        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        public string? Remarks { get; set; }
        public string PrayerHouse { get; set; } = string.Empty;
    }
}
