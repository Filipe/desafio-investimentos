using System.ComponentModel;

namespace Investimentos.Api.DTOs;

public class LoginRequest
{
    [DefaultValue(1)]
    public int ClienteId { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
