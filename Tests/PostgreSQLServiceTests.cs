using Xunit;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;

namespace Tests
{
    public class PostgreSQLServiceTests
    {
        private readonly IConfiguration _configuration;

        public PostgreSQLServiceTests()
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
        
    }
}