using Microsoft.EntityFrameworkCore;
using Locadora_veiculos.Data;
using System.Text.Json.Serialization;

namespace Locadora_veiculos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Conecta ao banco de dados
            builder.Services.AddDbContext<LocadoraDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configura os controllers
            builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    // Exibe enums como texto ("Aberto") ao invés de número (0)
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    // Evita erro de referência circular entre entidades
                    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            // Swagger para documentação e testes
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Locadora de Veículos API",
                    Version = "v1",
                    Description = "API para gerenciamento de locadora de veículos"
                });
            });

            var app = builder.Build();

            // Swagger sempre disponível
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}