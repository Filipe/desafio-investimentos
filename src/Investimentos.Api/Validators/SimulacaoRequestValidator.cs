using FluentValidation;
using Investimentos.Api.DTOs;

namespace Investimentos.Api.Validators;

public class SimulacaoRequestValidator : AbstractValidator<SimulacaoRequest>
{
    public SimulacaoRequestValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("O ID do cliente deve ser maior que zero.");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("O valor do investimento deve ser maior que zero.");

        RuleFor(x => x.PrazoMeses)
            .GreaterThan(0)
            .WithMessage("O prazo em meses deve ser maior que zero.")
            .LessThanOrEqualTo(360)
            .WithMessage("O prazo em meses não pode ser superior a 360 meses (30 anos).");

        RuleFor(x => x.TipoProduto)
            .NotEmpty()
            .WithMessage("O tipo de produto é obrigatório.")
            .MaximumLength(50)
            .WithMessage("O tipo de produto não pode ter mais de 50 caracteres.");
    }
}
