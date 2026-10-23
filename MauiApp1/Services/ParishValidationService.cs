using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MauiApp1.Data;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ParishValidationService
    {
        // ----------------------------------------------------
        // 1. DUPLICATE PREVENTION LOGIC (With Optional Middle Name)
        // ----------------------------------------------------

        public async Task<bool> IsDuplicatePersonAsync(
            MatiriParishDbContext db,
            string firstName,
            string lastName,
            DateTime? dateOfBirth,
            string? middleName = null,
            int? currentPersonId = null)
        {
            var cleanFirstName = firstName?.Trim().ToLower() ?? string.Empty;
            var cleanLastName = lastName?.Trim().ToLower() ?? string.Empty;
            var cleanMiddleName = middleName?.Trim().ToLower();

            var query = db.Persons.AsNoTracking().Where(p =>
                p.FirstName.ToLower() == cleanFirstName &&
                p.LastName.ToLower() == cleanLastName);

            if (!string.IsNullOrWhiteSpace(cleanMiddleName))
            {
                query = query.Where(p => p.MiddleName != null && p.MiddleName.ToLower() == cleanMiddleName);
            }

            if (dateOfBirth.HasValue)
            {
                query = query.Where(p => p.DateOfBirth.Date == dateOfBirth.Value.Date);
            }

            if (currentPersonId.HasValue && currentPersonId.Value > 0)
            {
                query = query.Where(p => p.Id != currentPersonId.Value);
            }

            return await query.AnyAsync();
        }

        // ----------------------------------------------------
        // 2. MEMBER & SACRAMENT VALIDATION RULES
        // ----------------------------------------------------

        public void ValidatePerson(Person person)
        {
            if (person == null)
                throw new ValidationException("Person information is missing.");

            if (string.IsNullOrWhiteSpace(person.FirstName))
                throw new ValidationException("First Name is required.");

            if (string.IsNullOrWhiteSpace(person.LastName))
                throw new ValidationException("Last Name is required.");

            if (person.DateOfBirth > DateTime.Today)
                throw new ValidationException("Date of Birth cannot be in the future.");
        }

        public void ValidateBaptism(BaptismRecord baptism, Person person)
        {
            if (baptism == null)
                throw new ValidationException("Baptism record details are missing.");

            if (baptism.BaptismDate < person.DateOfBirth)
                throw new ValidationException("Baptism date cannot be earlier than Date of Birth.");

            if (baptism.BaptismDate > DateTime.Today)
                throw new ValidationException("Baptism date cannot be in the future.");
        }

        public void ValidateConfirmation(ConfirmationRecord confirmation, Person person)
        {
            if (confirmation == null)
                throw new ValidationException("Confirmation record details are missing.");

            if (confirmation.DateOfConfirmation < person.DateOfBirth)
                throw new ValidationException("Confirmation date cannot be earlier than Date of Birth.");

            if (confirmation.DateOfConfirmation > DateTime.Today)
                throw new ValidationException("Confirmation date cannot be in the future.");
        }

        public void ValidateMarriage(MarriageRecord marriage, Person groom, Person bride)
        {
            if (marriage == null)
                throw new ValidationException("Marriage record details are missing.");

            if (groom == null || bride == null)
                throw new ValidationException("Both Groom and Bride must be specified.");

            if (groom.FirstName.Trim().Equals(bride.FirstName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                groom.LastName.Trim().Equals(bride.LastName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Groom and Bride cannot be the same person.");
            }

            if (marriage.DateOfMarriage < groom.DateOfBirth)
                throw new ValidationException("Marriage date cannot be before the Groom's Date of Birth.");

            if (marriage.DateOfMarriage < bride.DateOfBirth)
                throw new ValidationException("Marriage date cannot be before the Bride's Date of Birth.");

            if (marriage.DateOfMarriage > DateTime.Today)
                throw new ValidationException("Marriage date cannot be in the future.");
        }
    }
}