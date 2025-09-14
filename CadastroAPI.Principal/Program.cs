using System.Text.Json;
using System.IO; // Added for Path.Combine
using BackEnd.Models;
using BackEnd.Services;
using BackEnd.Validation;
using StackExchange.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using Microsoft.OpenApi.Models;

namespace BackEnd
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CadastroAPI", Version = "v1" });
            });

            // Configurar e registrar o serviço RabbitMQ
            services.Configure<BackEnd.Services.RabbitMQOptions>(Configuration.GetSection("RabbitMQ"));
            services.AddSingleton<BackEnd.Services.IRabbitMQService, BackEnd.Services.RabbitMQService>();
            services.Configure<BackEnd.Services.RabbitMQConsumerOptions>(Configuration.GetSection("RabbitMQConsumer"));

            // Registrar serviços de consumo de mensagens
            services.AddSingleton<BackEnd.Services.RabbitMQConsumerFactory>();
            services.AddTransient<BackEnd.Services.ClienteMessageProcessor>();
            services.AddHostedService<BackEnd.Services.RabbitMQConsumerHostedService>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://20.3.237.173:3000")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });
            // Configure Redis
            var redisOptions = ConfigurationOptions.Parse(Configuration.GetConnectionString("RedisConnection") ?? throw new InvalidOperationException("RedisConnection not found"));
            redisOptions.AbortOnConnectFail = false;
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
            services.AddScoped<IRedisService, RedisService>();

            // Configure PostgreSQL with Entity Framework Core
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("PostgreSQLConnection")));

            // Register Cliente Repository
            services.AddScoped<IClienteRepository, ClienteRepository>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
