using Microsoft.EntityFrameworkCore;
using Project.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Project.Extensions; // Adicionar este using

namespace Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura a convenção de nomenclatura para snake_case para PostgreSQL
            // modelBuilder.HasPostgresExtension(); // Removido pois estava causando erro de compilação
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Converte nomes de tabelas para snake_case
                entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

                // Converte nomes de colunas para snake_case
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName()?.ToSnakeCase());
                }
            }
        }
    }
}