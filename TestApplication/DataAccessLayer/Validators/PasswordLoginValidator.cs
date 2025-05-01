using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TestApplication.DataAccessLayer.Validators
{
    public class PasswordLoginValidator : ValidationAttribute
    {
        private static readonly Regex AllowedPattern = new(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled);
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {

            if (value == null)
            {
                return null;
            }

            var input = value as string;
            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult("field is required");

            if (!AllowedPattern.IsMatch(input))
                return new ValidationResult("only letters and digits are allowed");

            return ValidationResult.Success;
        }
    }
}
