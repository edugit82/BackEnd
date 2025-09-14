using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BackEnd.Validation
{
    public class BrazilianStateAttribute : ValidationAttribute
    {
        private readonly string[] _estadosBrasileiros = { "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string estado)
            {
                return new ValidationResult(ErrorMessage ?? "O estado deve ser uma string.");
            }

            if (string.IsNullOrWhiteSpace(estado))
            {
                return new ValidationResult(ErrorMessage ?? "Estado é obrigatório.");
            }

            if (!_estadosBrasileiros.Contains(estado.ToUpper()))
            {
                return new ValidationResult(ErrorMessage ?? "Estado inválido. Utilize uma sigla brasileira oficial.");
            }

            return ValidationResult.Success;
        }
    }
}