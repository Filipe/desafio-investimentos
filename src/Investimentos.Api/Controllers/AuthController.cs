using Investimentos.Api.Configuration;
using Investimentos.Api.Data;
using Investimentos.Api.DTOs;
using Investimentos.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtProvider _jwtProvider;
    private readonly AppDbContext _context;
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IJwtProvider jwtProvider,
        AppDbContext context,
        JwtConfig jwtConfig,
        ILogger<AuthController> logger)
    {
        _jwtProvider = jwtProvider;
        _context = context;
        _jwtConfig = jwtConfig;
        _logger = logger;
    }

    /// <summary>
    /// Autentica um cliente e retorna um token JWT
    /// </summary>
    /// <param name="request">Dados de login contendo o ID do cliente</param>
    /// <returns>Token JWT para acesso aos endpoints protegidos</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Tentativa de login para cliente {ClienteId}", request.ClienteId);

        if (request.ClienteId <= 0)
        {
            _logger.LogWarning("ClienteId inválido: {ClienteId}", request.ClienteId);
            return BadRequest(new { error = "ClienteId inválido" });
        }

        // Verifica se o cliente existe no banco
        var clienteExists = await _context.Clientes
            .AnyAsync(c => c.Id == request.ClienteId);

        if (!clienteExists)
        {
            _logger.LogWarning("Cliente {ClienteId} não encontrado", request.ClienteId);
            return NotFound(new { error = $"Cliente com ID {request.ClienteId} não encontrado" });
        }

        // Gera o token JWT
        var token = _jwtProvider.GenerateToken(request.ClienteId);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationMinutes);

        _logger.LogInformation("Login bem-sucedido para cliente {ClienteId}", request.ClienteId);

        return Ok(new LoginResponse
        {
            Token = token,
            ClienteId = request.ClienteId,
            ExpiresAt = expiresAt
        });
    }
}
