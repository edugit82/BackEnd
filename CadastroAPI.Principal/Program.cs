using BackEnd.Services;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var app = builder.Build();
var env = app.Environment;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CadastroAPI", Version = "v1" });
});

// Configurar e registrar o serviço RabbitMQ
services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
services.AddSingleton<IRabbitMQService, RabbitMQService>();
services.Configure<RabbitMQConsumerOptions>(configuration.GetSection("RabbitMQConsumer"));

// Registrar serviços de consumo de mensagens
services.AddSingleton<RabbitMQConsumerFactory>();
services.AddTransient<ClienteMessageProcessor>();
services.AddHostedService<RabbitMQConsumerHostedService>();

services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://20.3.237.173:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
// Configure Redis
var redisOptions = ConfigurationOptions.Parse(configuration.GetConnectionString("RedisConnection") ?? throw new InvalidOperationException("RedisConnection not found"));
redisOptions.AbortOnConnectFail = false;
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
services.AddScoped<IRedisService, RedisService>();

// Configure PostgreSQL with Entity Framework Core
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection")));

// Register Cliente Repository
services.AddScoped<IClienteRepository, ClienteRepository>();

// Configure the HTTP request pipeline.
if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseAuthorization();