namespace MatiriParish.Registry.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; } = string.Empty;

        // Family Links
        public int? FatherId { get; set; }
        public Person? Father { get; set; }

        public int? MotherId { get; set; }
        public Person? Mother { get; set; }

        // Sacramental History
        public BaptismRecord? Baptism { get; set; }
        public ConfirmationRecord? Confirmation { get; set; }
        public MarriageRecord? MarriageAsGroom { get; set; }
        public MarriageRecord? MarriageAsBride { get; set; }
    }
}