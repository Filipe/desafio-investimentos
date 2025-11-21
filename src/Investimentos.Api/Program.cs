using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Investimentos.Api.Data;
using Investimentos.Api.Services;
using Investimentos.Api.Middlewares;
using Investimentos.Api.Configuration;
using Investimentos.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicação Investimentos.Api");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog
    builder.Host.UseSerilog();

    // Database Context
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // JWT Configuration
    var jwtConfig = new JwtConfig();
    builder.Configuration.GetSection("Jwt").Bind(jwtConfig);
    builder.Services.AddSingleton(jwtConfig);

    // Services
    builder.Services.AddScoped<ISimulacaoService, SimulacaoService>();
    builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
    builder.Services.AddScoped<IPerfilRiscoService, PerfilRiscoService>();
    builder.Services.AddScoped<ITelemetriaService, TelemetriaService>();
    builder.Services.AddScoped<IJwtProvider, JwtProvider>();

    // Add services to the container.
    builder.Services.AddControllers();

    // JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret))
        };
    });

    builder.Services.AddAuthorization();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(Program));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Investimentos API",
            Version = "v1",
            Description = "API para gerenciamento de investimentos com autenticação JWT"
        });

        // Configurar JWT no Swagger
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Insira o token retornado pelo endpoint /api/auth/login.",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Seed Database
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData.Seed(dbContext);
    }

    // Configure the HTTP request pipeline.
    // Developer Exception Page deve vir primeiro
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Swagger habilitado em Development e Production (para facilitar testes via Docker)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Investimentos API v1");
        c.RoutePrefix = "swagger"; // Explicitamente definir o prefixo
    });

    // Comentado para Docker - sem HTTPS no container
    // app.UseHttpsRedirection();

    // Development Bypass Middleware (antes da autenticação)
    if (app.Environment.IsDevelopment())
    {
        app.UseMiddleware<DevelopmentBypassMiddleware>();
    }

    // Middleware de Telemetria
    app.UseMiddleware<TelemetryMiddleware>();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Aplicação iniciada com sucesso");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}
