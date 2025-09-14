using System.ComponentModel.DataAnnotations;
using BackEnd.Validation;

namespace BackEnd.Models
{
    public class Cliente
    {
        [Required(ErrorMessage = "Id é obrigatório.")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Nome é obrigatório.")]
        [StringLength(300, ErrorMessage = "Nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório.")]
        [CpfValidation(ErrorMessage = "CPF inválido.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório.")]
        [StringLength(20, ErrorMessage = "Telefone não pode exceder 20 caracteres.")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data de Nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [PastDate(ErrorMessage = "Data de Nascimento não pode ser futura.")]
        public DateTime DataNascimento { get; set; } = default(DateTime);

        [Required(ErrorMessage = "Endereço é obrigatório.")]
        [StringLength(500, ErrorMessage = "Endereço não pode exceder 200 caracteres.")]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número é obrigatório.")]
        [StringLength(10, ErrorMessage = "Número não pode exceder 10 caracteres.")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cidade é obrigatória.")]
        [StringLength(300, ErrorMessage = "Cidade não pode exceder 100 caracteres.")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Estado é obrigatório.")]
        [BrazilianState(ErrorMessage = "Estado inválido. Utilize uma sigla brasileira oficial.")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "CEP é obrigatório.")]
        [CepValidation(ErrorMessage = "Formato de CEP inválido. Utilize o formato XXXXX-XXX.")]
        public string Cep { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gênero é obrigatório.")]
        [StringLength(50, ErrorMessage = "Gênero não pode exceder 50 caracteres.")]
        public string Genero { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profissão é obrigatória.")]
        [StringLength(300, ErrorMessage = "Profissão não pode exceder 100 caracteres.")]
        public string Profissao { get; set; } = string.Empty;
    }
}