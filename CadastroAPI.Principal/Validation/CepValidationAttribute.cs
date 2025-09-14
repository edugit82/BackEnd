using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BackEnd.Validation
{
    public class CepValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string cep)
            {
                return new ValidationResult(ErrorMessage ?? "O CEP deve ser uma string.");
            }

            if (string.IsNullOrWhiteSpace(cep))
            {
                return new ValidationResult(ErrorMessage ?? "CEP é obrigatório.");
            }

            if (!Regex.IsMatch(cep, @"^\d{5}-\d{3}$"))
            {
                return new ValidationResult(ErrorMessage ?? "Formato de CEP inválido. Utilize o formato XXXXX-XXX.");
            }

            return ValidationResult.Success;
        }
    }
}