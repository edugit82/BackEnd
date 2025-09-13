using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;
using BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CadastroAPI.Tests
{
    public class ClienteRepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task AddClienteAsync_AddsClienteSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Add",
                Email = "add@example.com",
                Cpf = "111.111.111-11",
                Telefone = "(11)99999-9999",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua A",
                Numero = "1",
                Cidade = "Cidade A",
                Estado = "SP",
                Cep = "00000-000",
                Genero = "Masculino",
                Profissao = "Dev"
            };

            // Act
            await repository.AddClienteAsync(cliente);

            // Assert
            var addedCliente = await context.Clientes.FindAsync(cliente.Id);
            Assert.NotNull(addedCliente);
            Assert.Equal(cliente.Nome, addedCliente.Nome);
        }

        [Fact]
        public async Task GetClienteByIdAsync_ReturnsCliente_WhenClienteExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Get",
                Email = "get@example.com",
                Cpf = "222.222.222-22",
                Telefone = "(11)99999-9999",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua B",
                Numero = "2",
                Cidade = "Cidade B",
                Estado = "RJ",
                Cep = "11111-111",
                Genero = "Feminino",
                Profissao = "Analista"
            };
            await context.Clientes.AddAsync(cliente);
            await context.SaveChangesAsync();

            // Act
            var foundCliente = await repository.GetClienteByIdAsync(cliente.Id);

            // Assert
            Assert.NotNull(foundCliente);
            Assert.Equal(cliente.Id, foundCliente.Id);
        }

        [Fact]
        public async Task GetClienteByIdAsync_ReturnsNull_WhenClienteDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);

            // Act
            var foundCliente = await repository.GetClienteByIdAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.Null(foundCliente);
        }

        [Fact]
        public async Task GetAllClientesAsync_ReturnsAllClientes()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            context.Clientes.AddRange(
                new Cliente { Id = Guid.NewGuid().ToString(), Nome = "Cliente 1", Email = "c1@example.com", Cpf = "333.333.333-33", DataNascimento = new DateTime(1990, 1, 1), Telefone = "(11)99999-9999", Endereco = "Rua C", Numero = "3", Cidade = "Cidade C", Estado = "MG", Cep = "22222-222", Genero = "Masculino", Profissao = "Arquiteto" },
                new Cliente { Id = Guid.NewGuid().ToString(), Nome = "Cliente 2", Email = "c2@example.com", Cpf = "444.444.444-44", DataNascimento = new DateTime(1990, 1, 1), Telefone = "(11)99999-9999", Endereco = "Rua D", Numero = "4", Cidade = "Cidade D", Estado = "ES", Cep = "33333-333", Genero = "Feminino", Profissao = "Designer" }
            );
            await context.SaveChangesAsync();

            // Act
            var clientes = await repository.GetAllClientesAsync();

            // Assert
            Assert.Equal(2, clientes.Count());
        }

        [Fact]
        public async Task GetAllClientesAsync_FiltersByNome()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            context.Clientes.AddRange(
                new Cliente { Id = Guid.NewGuid().ToString(), Nome = "Alice", Email = "alice@example.com", Cpf = "555.555.555-55", DataNascimento = new DateTime(1990, 1, 1), Telefone = "(11)99999-9999", Endereco = "Rua E", Numero = "5", Cidade = "Cidade E", Estado = "SP", Cep = "44444-444", Genero = "Feminino", Profissao = "Engenheira" },
                new Cliente { Id = Guid.NewGuid().ToString(), Nome = "Bob", Email = "bob@example.com", Cpf = "666.666.666-66", DataNascimento = new DateTime(1990, 1, 1), Telefone = "(11)99999-9999", Endereco = "Rua F", Numero = "6", Cidade = "Cidade F", Estado = "RJ", Cep = "55555-555", Genero = "Masculino", Profissao = "Desenvolvedor" }
            );
            await context.SaveChangesAsync();

            // Act
            var clientes = await repository.GetAllClientesAsync(nome: "Ali");

            // Assert
            Assert.Single(clientes);
            Assert.Equal("Alice", clientes.First().Nome);
        }

        [Fact]
        public async Task GetAllClientesAsync_FiltersByEmail()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            context.Clientes.AddRange(
                new Cliente { Id = Guid.NewGuid().ToString(), Nome = "Carlos", Email = "carlos@example.com", Cpf = "777.777.777-77", DataNascimento = new DateTime(1990, 1, 1), Telefone = "(11)99999-9999", Endereco = "Rua G", Numero = "7", Cidade = "Cidade G", Estado = "MG", Cep = "66666-666", Genero = "Masculino", Profissao = "Gerente" },
                new Cliente { Id = Guid.NewGuid().ToString(), Nome = "Diana", Email = "diana@example.com", Cpf = "888.888.888-88", DataNascimento = new DateTime(1990, 1, 1), Telefone = "(11)99999-9999", Endereco = "Rua H", Numero = "8", Cidade = "Cidade H", Estado = "ES", Cep = "77777-777", Genero = "Feminino", Profissao = "Contadora" }
            );
            await context.SaveChangesAsync();

            // Act
            var clientes = await repository.GetAllClientesAsync(email: "carlos");

            // Assert
            Assert.Single(clientes);
            Assert.Equal("Carlos", clientes.First().Nome);
        }

        [Fact]
        public async Task GetAllClientesAsync_AppliesPagination()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            for (int i = 0; i < 5; i++)
            {
                context.Clientes.Add(new Cliente { Id = Guid.NewGuid().ToString(), Nome = $"Cliente {i}", Email = $"cliente{i}@example.com", Cpf = $"{i}{i}{i}.{i}{i}{i}.{i}{i}{i}-{i}{i}", DataNascimento = new DateTime(1990, 1, 1), Telefone = "(11)99999-9999", Endereco = "Rua I", Numero = $"{i}", Cidade = "Cidade I", Estado = "SP", Cep = "88888-888", Genero = "Masculino", Profissao = "Profissao" });
            }
            await context.SaveChangesAsync();

            // Act
            var clientesPage1 = await repository.GetAllClientesAsync(pageNumber: 1, pageSize: 2);
            var clientesPage2 = await repository.GetAllClientesAsync(pageNumber: 2, pageSize: 2);

            // Assert
            Assert.Equal(2, clientesPage1.Count());
            Assert.Equal(2, clientesPage2.Count());
            Assert.Equal("Cliente 0", clientesPage1.First().Nome);
            Assert.Equal("Cliente 2", clientesPage2.First().Nome);
        }

        [Fact]
        public async Task UpdateClienteAsync_UpdatesClienteSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Original Name",
                Email = "original@example.com",
                Cpf = "999.999.999-99",
                Telefone = "(11)99999-9999",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua J",
                Numero = "9",
                Cidade = "Cidade J",
                Estado = "SP",
                Cep = "99999-999",
                Genero = "Masculino",
                Profissao = "Profissao"
            };
            await context.Clientes.AddAsync(cliente);
            await context.SaveChangesAsync();

            cliente.Nome = "Updated Name";
            cliente.Email = "updated@example.com";

            // Act
            await repository.UpdateClienteAsync(cliente);

            // Assert
            var updatedCliente = await context.Clientes.FindAsync(cliente.Id);
            Assert.NotNull(updatedCliente);
            Assert.Equal("Updated Name", updatedCliente.Nome);
            Assert.Equal("updated@example.com", updatedCliente.Email);
        }

        [Fact]
        public async Task UpdateClienteAsync_ThrowsKeyNotFoundException_WhenClienteDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            var nonExistentCliente = new Cliente { Id = Guid.NewGuid().ToString(), Nome = "Non Existent" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repository.UpdateClienteAsync(nonExistentCliente));
        }

        [Fact]
        public async Task DeleteClienteAsync_RemovesClienteSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);
            var cliente = new Cliente
            {
                Id = Guid.NewGuid().ToString(),
                Nome = "Teste Delete",
                Email = "delete@example.com",
                Cpf = "000.000.000-00",
                Telefone = "(11)99999-9999",
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Rua K",
                Numero = "10",
                Cidade = "Cidade K",
                Estado = "SP",
                Cep = "00000-000",
                Genero = "Masculino",
                Profissao = "Profissao"
            };
            await context.Clientes.AddAsync(cliente);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteClienteAsync(cliente.Id);

            // Assert
            var deletedCliente = await context.Clientes.FindAsync(cliente.Id);
            Assert.Null(deletedCliente);
        }

        [Fact]
        public async Task DeleteClienteAsync_ThrowsKeyNotFoundException_WhenClienteDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ClienteRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repository.DeleteClienteAsync(Guid.NewGuid().ToString()));
        }
    }
}