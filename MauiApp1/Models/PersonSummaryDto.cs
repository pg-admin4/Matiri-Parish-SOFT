using System;

namespace MauiApp1.Models
{
    public class PersonSummaryDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        // Helpful disambiguators
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;

        // Baptism ledger info if available
        public string BaptismBook { get; set; } = string.Empty;
        public int BaptismPage { get; set; }
        public int BaptismEntry { get; set; }

        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";
    }
}
