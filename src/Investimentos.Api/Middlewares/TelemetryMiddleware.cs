using System.Diagnostics;
using Investimentos.Api.Services;

namespace Investimentos.Api.Middlewares;

public class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(RequestDelegate next, ILogger<TelemetryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITelemetriaService telemetriaService)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.Request.Path.Value ?? "/";
        var metodoHttp = context.Request.Method;

        try
        {
            // Continua o pipeline
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var tempoRespostaMs = stopwatch.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;

            // Extrair nome do serviço do endpoint (exemplo: /api/simular-investimento -> simular-investimento)
            var nomeServico = ExtrairNomeServico(endpoint);

            // Registra a telemetria de forma assíncrona
            try
            {
                await telemetriaService.RegistrarChamadaAsync(
                    nomeServico,
                    endpoint,
                    metodoHttp,
                    tempoRespostaMs,
                    statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar telemetria");
            }

            _logger.LogInformation(
                "{Metodo} {Endpoint} - {StatusCode} - {Tempo}ms",
                metodoHttp, endpoint, statusCode, tempoRespostaMs);
        }
    }

    private string ExtrairNomeServico(string endpoint)
    {
        // Remove /api/ do início e pega o primeiro segmento
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length == 0)
            return "unknown";

        // Se começa com "api", pega o próximo segmento
        if (segments.Length > 1 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
            return segments[1];

        return segments[0];
    }
}
