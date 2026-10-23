using System;
using System.Collections.Generic;

namespace MatiriParish.Api.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        public int? FatherId { get; set; }
        public Person? Father { get; set; }

        public int? MotherId { get; set; }
        public Person? Mother { get; set; }

        public virtual BaptismRecord? Baptism { get; set; }
        public virtual ConfirmationRecord? Confirmation { get; set; }
        public virtual ICollection<MarriageRecord>? Marriages { get; set; }
    }
}
