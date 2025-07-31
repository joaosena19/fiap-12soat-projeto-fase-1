using API.Middleware;
using Application;
using Application.Auth.Interfaces;
using Application.Auth.Services;
using Application.Cadastros.Interfaces;
using Application.Cadastros.Services;
using Application.Estoque.Interfaces;
using Application.Estoque.Services;
using Application.OrdemServico.Interfaces;
using Application.OrdemServico.Interfaces.External;
using Application.OrdemServico.Services;
using AutoMapper;
using DotNetEnv;
using Infrastructure.AntiCorruptionLayer.OrdemServico;
using Infrastructure.Authentication;
using Infrastructure.Database;
using Infrastructure.Repositories.Cadastros;
using Infrastructure.Repositories.Estoque;
using Infrastructure.Repositories.OrdemServico;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
    {
        // Require authorization for all controllers by default
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Oficina Mecânica API", 
        Version = "v1",
        Description = "API para gerenciamento de oficina mecânica",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "João Dainese",
            Email = "joaosenadainese@gmail.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Configure JWT authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure AutoMapper
var mapperConfig = AutoMapperConfig.GetConfiguration();
builder.Services.AddSingleton(mapperConfig);
builder.Services.AddSingleton<IMapper>(provider => provider.GetRequiredService<MapperConfiguration>().CreateMapper());

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing. Please check appsettings.json.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IItemEstoqueService, ItemEstoqueService>();
builder.Services.AddScoped<IItemEstoqueRepository, ItemEstoqueRepository>();

// Register anti-corruption layer services for OrdemServico bounded context
builder.Services.AddScoped<IServicoExternalService, ServicoExternalService>();
builder.Services.AddScoped<IEstoqueExternalService, EstoqueExternalService>();
builder.Services.AddScoped<IVeiculoExternalService, VeiculoExternalService>();
builder.Services.AddScoped<IClienteExternalService, ClienteExternalService>();

builder.Services.AddScoped<IOrdemServicoService, OrdemServicoService>();
builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Add exception handling middleware (should be one of the first middlewares)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oficina Mecânica API v1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at app root
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
    //remove dbcontext hereto avoid issues with multiple instances
    public static void RemoveDbContext(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(AppDbContext));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
        var optionsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
        if (optionsDescriptor != null)
        {
            services.Remove(optionsDescriptor);
        }
    }
}