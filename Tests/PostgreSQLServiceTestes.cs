using Xunit;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Threading.Tasks;

namespace Tests
{
    public class PostgreSQLServiceTestes
    {
        private readonly IConfiguration _configuration;

        public PostgreSQLServiceTestes()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("d:/Repositorio/BackEnd/Project/appsettings.json")
                .Build();
        }

        [Fact]
        public void DatabaseConnection_ShouldBeOpenAndCloseSuccessfully()
        {
            // Arrange
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            Assert.False(string.IsNullOrEmpty(connectionString), "Connection string cannot be null or empty.");

            // Act & Assert
            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Assert.True(connection.State == System.Data.ConnectionState.Open, "Database connection should be open.");
                    connection.Close();
                    Assert.True(connection.State == System.Data.ConnectionState.Closed, "Database connection should be closed.");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to connect to the database: {ex.Message}");
                }
            }
        }

        [Fact]
        public async Task CrudOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            Assert.False(string.IsNullOrEmpty(connectionString), "Connection string cannot be null or empty.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                // Ensure the database is clean for this test
                context.Clients.RemoveRange(context.Clients);
                await context.SaveChangesAsync();

                // Create
                var newClient = new Client
                {
                    Nome = "Test Client",
                    Email = "test@example.com",
                    CPF = "12345678901",
                    Telefone = "11987654321",
                    DataNascimento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Endereco = "Rua Teste, 123",
                    Numero = "123",
                    Complemento = "Apto 101",
                    Cidade = "Sao Paulo",
                    Estado = "SP",
                    CEP = "01000000",
                    Genero = "Masculino",
                    Profissao = "Engenheiro"
                };
                context.Clients.Add(newClient);
                await context.SaveChangesAsync();

                Assert.NotEqual(0, newClient.Id);

                // Read
                var retrievedClient = await context.Clients.FirstOrDefaultAsync(c => c.Nome == "Test Client");
                Assert.NotNull(retrievedClient);
                Assert.Equal("Test Client", retrievedClient.Nome);

                // Update
                retrievedClient.Nome = "Updated Client";
                context.Clients.Update(retrievedClient);
                await context.SaveChangesAsync();

                // Assert update
                var updatedClient = await context.Clients.FirstOrDefaultAsync(c => c.Id == newClient.Id);
                Assert.NotNull(updatedClient);
                Assert.Equal("Updated Client", updatedClient.Nome);

                // Delete
                context.Clients.Remove(updatedClient);
                await context.SaveChangesAsync();

                var deletedClient = await context.Clients.FindAsync(newClient.Id);
                Assert.Null(deletedClient);
            }
        }
    }
}