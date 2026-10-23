using System;
using System.Collections.Generic;

namespace MauiApp1.Models
{
    public class Person
    {
        public int Id { get; set; }

        // Personal Information
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; } = string.Empty;

        // Geographical & Address Details
        public string Residence { get; set; } = string.Empty;
        public string SubCounty { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;

        // Convenient Helper Property for UI Display
        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        // Parent Relationships (Self-Referencing Foreign Keys)
        public int? FatherId { get; set; }
        public virtual Person? Father { get; set; }

        public int? MotherId { get; set; }
        public virtual Person? Mother { get; set; }

        // Linked Sacramental Records (Cross-Referencing)
        public virtual BaptismRecord? Baptism { get; set; }
        public virtual ConfirmationRecord? Confirmation { get; set; }

        // Marriage Relationships (Supports 1-to-1 or 1-to-Many history)
        public virtual MarriageRecord? MarriageAsGroom { get; set; }
        public virtual MarriageRecord? MarriageAsBride { get; set; }
    }
}