using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(14, MinimumLength = 11, ErrorMessage = "O CPF deve ter 11 dígitos.")]
        public required string CPF { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public required string Telefone { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        public DateTime DataNascimento { get; set; }

        [Required(ErrorMessage = "O endereço é obrigatório.")]
        [StringLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres.")]
        public required string Endereco { get; set; }

        [Required(ErrorMessage = "O número é obrigatório.")]
        [StringLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres.")]
        public required string Numero { get; set; }

        [StringLength(100, ErrorMessage = "O complemento deve ter no máximo 100 caracteres.")]
        public string? Complemento { get; set; }

        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public required string Cidade { get; set; }

        [Required(ErrorMessage = "O estado é obrigatório.")]
        [StringLength(50, ErrorMessage = "O estado deve ter no máximo 50 caracteres.")]
        public required string Estado { get; set; }

        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [StringLength(10, ErrorMessage = "O CEP deve ter no máximo 10 caracteres.")]
        public required string CEP { get; set; }

        [Required(ErrorMessage = "O gênero é obrigatório.")]
        [StringLength(50, ErrorMessage = "O gênero deve ter no máximo 50 caracteres.")]
        public required string Genero { get; set; }

        [Required(ErrorMessage = "A profissão é obrigatória.")]
        [StringLength(100, ErrorMessage = "A profissão deve ter no máximo 100 caracteres.")]
        public required string Profissao { get; set; }
    }
}