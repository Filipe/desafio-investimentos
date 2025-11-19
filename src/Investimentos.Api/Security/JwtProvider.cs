using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Investimentos.Api.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Investimentos.Api.Security;

public interface IJwtProvider
{
    string GenerateToken(int clienteId);
}

public class JwtProvider : IJwtProvider
{
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<JwtProvider> _logger;

    public JwtProvider(JwtConfig jwtConfig, ILogger<JwtProvider> logger)
    {
        _jwtConfig = jwtConfig;
        _logger = logger;
    }

    public string GenerateToken(int clienteId)
    {
        _logger.LogInformation("Gerando token JWT para cliente {ClienteId}", clienteId);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, clienteId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("clienteId", clienteId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, clienteId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Token gerado com sucesso para cliente {ClienteId}, expira em {Minutes} minutos",
            clienteId, _jwtConfig.ExpirationMinutes);

        return tokenString;
    }
}
