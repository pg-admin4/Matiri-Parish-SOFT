using System;

namespace MauiApp1.Models
{
    public class BaptismRecord
    {
        internal string FirstName;
        internal string LastName;
        internal DateTime? DateOfBirth;
        internal string PlaceOfBirth;
        internal string FatherName;
        internal string MotherName;
        internal DateTime? BaptismDate;
        internal string Minister;
        internal string Godparent1;
        internal string Godparent2;
        internal string? Jumuiya;

        public int Id { get; set; }

        // Foreign Key & Navigation to Central Person Profile
        public int PersonId { get; set; }
        public virtual Person Person { get; set; } = null!;

        // Sacrament Details
        public DateTime DateOfBaptism { get; set; }
        public string OfficiatingPriest { get; set; } = string.Empty;

        // Godparents / Sponsors
        public string GodfatherName { get; set; } = string.Empty;
        public string GodmotherName { get; set; } = string.Empty;

        // Physical Ledger Reference
        public string BookNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int EntryNumber { get; set; }

        // Additional Annotations / Notes
        public string? Remarks { get; set; }
        // Prayer House where the sacrament was held (persisted and bindable)
        public string PrayerHouse { get; set; } = string.Empty;
    }
}