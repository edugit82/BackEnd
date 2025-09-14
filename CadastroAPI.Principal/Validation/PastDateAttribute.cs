using System;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Validation
{
    public class PastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime date)
            {
                return new ValidationResult(ErrorMessage ?? "A data deve ser uma data válida.");
            }
            {
                if (date > DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage ?? "A data não pode ser futura.");
                }
            }
            return ValidationResult.Success;
        }
    }
}