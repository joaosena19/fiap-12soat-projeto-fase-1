using Application;
using AutoMapper;
using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Tests.Integration
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private readonly string _databaseName;

        public TestWebApplicationFactory()
        {
            // Gera uma database única para cada classe de teste, útil para múltiplos testes não interferirem no mesmo banco e corromperem outros testes.
            var stackTrace = new StackTrace();
            var callingFrame = stackTrace.GetFrames()
                ?.FirstOrDefault(f => f.GetMethod()?.DeclaringType?.Name?.EndsWith("Tests") == true);
            
            var testClassName = callingFrame?.GetMethod()?.DeclaringType?.Name ?? "Unknown";
            _databaseName = $"TestDb_{testClassName}_{Guid.NewGuid():N}";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove DbContext atual, com conexão com o banco real
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Cria conexão com banco de dados em memória
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // Configure AutoMapper for integration tests
                var mapperConfig = AutoMapperConfig.GetConfiguration();
                services.AddSingleton(mapperConfig);
                services.AddSingleton<IMapper>(provider => provider.GetRequiredService<MapperConfiguration>().CreateMapper());
            });

            base.ConfigureWebHost(builder);
        }
    }
}
