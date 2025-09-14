using BackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasKey(c => c.Id);
            builder.ToTable("clientes");

            builder.Property(c => c.Id)
                .HasColumnName("id")                
                .IsRequired();

            builder.Property(c => c.Nome)
                .HasColumnName("nome")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Cpf)
                .HasColumnName("cpf")
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(c => c.Telefone)
                .HasColumnName("telefone")
                .HasMaxLength(20);

            builder.Property(c => c.DataNascimento)
                .IsRequired()
                .HasColumnName("datanascimento");

            builder.Property(c => c.Endereco)
                .HasMaxLength(200)
                .HasColumnName("endereco");

            builder.Property(c => c.Numero)
                .HasMaxLength(20)
                .HasColumnName("numero");

            builder.Property(c => c.Cidade)
                .HasMaxLength(100)
                .HasColumnName("cidade");

            builder.Property(c => c.Estado)
                .HasMaxLength(2)
                .HasColumnName("estado");

            builder.Property(c => c.Cep)
                .HasMaxLength(9)
                .HasColumnName("cep");

            builder.Property(c => c.Genero)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("genero");

            builder.Property(c => c.Profissao)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("profissao");
        }
    }
}