using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Investimentos.Api.Middlewares;

public class DevelopmentBypassMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DevelopmentBypassMiddleware> _logger;

    public DevelopmentBypassMiddleware(RequestDelegate next, ILogger<DevelopmentBypassMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verifica se o header X-Debug-Bypass está presente
        if (context.Request.Headers.TryGetValue("X-Debug-Bypass", out var bypassValue) && 
            bypassValue == "1")
        {
            _logger.LogWarning("🔓 Development bypass ativado - autenticando como cliente 123");

            // Cria claims para o cliente 123
            var claims = new[]
            {
                new System.Security.Claims.Claim("clienteId", "123"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            context.User = principal;

            _logger.LogDebug("Cliente 123 autenticado via bypass");
        }

        await _next(context);
    }
}
