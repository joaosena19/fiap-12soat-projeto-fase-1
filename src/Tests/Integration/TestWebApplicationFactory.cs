using Application;
using AutoMapper;
using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
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
                    options.UseInMemoryDatabase("TestDb");
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
