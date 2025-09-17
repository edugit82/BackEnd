using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Project.Configurations;
using Project.Messaging;
using Project.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// A configuração é carregada em uma hierarquia padrão: variáveis de ambiente e User Secrets (em desenvolvimento)
// têm precedência sobre appsettings.json. Não é necessário código adicional para priorizar variáveis de ambiente.
builder.Services.Configure<RedisCacheSettings>(builder.Configuration.GetSection("RedisCacheSettings"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RedisCacheSettings>>().Value;
    var configurationOptions = new ConfigurationOptions
    {
        EndPoints = { { settings.Host ?? string.Empty, settings.Port } },
        Password = settings.Password,
        User = settings.User,
        AbortOnConnectFail = false,
        ConnectRetry = 5,
        ConnectTimeout = 5000,
        SyncTimeout = 5000,
        Ssl = true,
        SslHost = settings.Host
    };
    return ConnectionMultiplexer.Connect(configurationOptions);
});
builder.Services.AddSingleton<IDatabase>(sp =>
{
    var redis = sp.GetRequiredService<IConnectionMultiplexer>();
    return redis.GetDatabase();
});

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddControllers();
builder.Services.AddDbContext<Project.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();
builder.Services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();
builder.Services.AddHostedService<RabbitMQConsumerService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://20.3.237.173:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
