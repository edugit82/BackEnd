using Xunit;
using BackEnd.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CadastroAPI.Tests
{
    public class ClienteTests
    {
        [Fact]
        public void Cliente_CanBeCreated_Successfully()
        {
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Cliente",
                Email = "teste@example.com",
                Cpf = "313.479.208-70", // Valid CPF
                Telefone = "(11)98765-4321",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua Teste, 123",
                Numero = "123",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Cep = "01000-000",
                Genero = "Masculino",
                Profissao = "Engenheiro"
            };

            // Act
            var validationContext = new ValidationContext(cliente);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(cliente, validationContext, validationResults, true);
            
            // Assert
            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    Console.WriteLine($"Validation Error: {validationResult.ErrorMessage}");
                }
            }
            Assert.True(isValid, "Cliente should be valid");
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Cliente_Nome_IsRequired()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = string.Empty, // Invalid
                Email = "teste@example.com",
                Cpf = "123.456.789-00",
                Telefone = "(11)98765-4321",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua Teste, 123",
                Numero = "123",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Cep = "01000-000",
                Genero = "Masculino",
                Profissao = "Engenheiro"
            };

            // Act
            var validationContext = new ValidationContext(cliente);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(cliente, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid, "Cliente should be invalid due to missing Nome");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Nome é obrigatório.");
        }

        [Fact]
        public void Cliente_Email_IsInvalid()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Cliente",
                Email = "invalid-email", // Invalid
                Cpf = "123.456.789-00",
                Telefone = "(11)98765-4321",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua Teste, 123",
                Numero = "123",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Cep = "01000-000",
                Genero = "Masculino",
                Profissao = "Engenheiro"
            };

            // Act
            var validationContext = new ValidationContext(cliente);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(cliente, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid, "Cliente should be invalid due to invalid Email");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Formato de e-mail inválido.");
        }

        [Fact]
        public void Cliente_Cpf_IsInvalid()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Cliente",
                Email = "teste@example.com",
                Cpf = "123", // Invalid
                Telefone = "(11)98765-4321",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua Teste, 123",
                Numero = "123",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Cep = "01000-000",
                Genero = "Masculino",
                Profissao = "Engenheiro"
            };

            // Act
            var validationContext = new ValidationContext(cliente);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(cliente, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid, "Cliente should be invalid due to invalid CPF");
            Assert.Contains(validationResults, r => r.ErrorMessage == "CPF deve conter 11 dígitos.");
        }

        [Fact]
        public void Cliente_DataNascimento_IsFutureDate()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Cliente",
                Email = "teste@example.com",
                Cpf = "123.456.789-00",
                Telefone = "(11)98765-4321",
                DataNascimento = DateTime.Now.AddDays(1), // Invalid
                Endereco = "Rua Teste, 123",
                Numero = "123",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Cep = "01000-000",
                Genero = "Masculino",
                Profissao = "Engenheiro"
            };

            // Act
            var validationContext = new ValidationContext(cliente);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(cliente, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid, "Cliente should be invalid due to future DataNascimento");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Data de Nascimento não pode ser futura.");
        }

        [Fact]
        public void Cliente_Estado_IsInvalid()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Cliente",
                Email = "teste@example.com",
                Cpf = "123.456.789-00",
                Telefone = "(11)98765-4321",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua Teste, 123",
                Numero = "123",
                Cidade = "Sao Paulo",
                Estado = "XX", // Invalid
                Cep = "01000-000",
                Genero = "Masculino",
                Profissao = "Engenheiro"
            };

            // Act
            var validationContext = new ValidationContext(cliente);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(cliente, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid, "Cliente should be invalid due to invalid Estado");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Estado inválido. Utilize uma sigla brasileira oficial.");
        }

        [Fact]
        public void Cliente_Cep_IsInvalid()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Cliente",
                Email = "teste@example.com",
                Cpf = "123.456.789-00",
                Telefone = "(11)98765-4321",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua Teste, 123",
                Numero = "123",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Cep = "123", // Invalid
                Genero = "Masculino",
                Profissao = "Engenheiro"
            };

            // Act
            var validationContext = new ValidationContext(cliente);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(cliente, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid, "Cliente should be invalid due to invalid CEP");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Formato de CEP inválido. Utilize o formato XXXXX-XXX.");
        }
    }
}