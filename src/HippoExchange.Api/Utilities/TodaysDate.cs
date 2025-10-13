using System;
using System.ComponentModel.DataAnnotations;

namespace HippoExchange.Api.Utilities
{

    public class DateNotInFutureAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date > DateTime.Today)
                    return new ValidationResult("Purchase date cannot be in the future.");
            }

            return ValidationResult.Success!;
        }
    }
}
