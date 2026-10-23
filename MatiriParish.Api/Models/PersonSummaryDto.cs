using System;

namespace MatiriParish.Api.Models
{
    public class PersonSummaryDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;

        public string BaptismBook { get; set; } = string.Empty;
        public int BaptismPage { get; set; }
        public int BaptismEntry { get; set; }

        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";
    }
}
