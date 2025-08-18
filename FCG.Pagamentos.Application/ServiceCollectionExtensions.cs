using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Pagamentos.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPagamentosService(this IServiceCollection services)
    {
        services.AddScoped<ITransacaoService, TransacaoService>();
        services.AddScoped<IReembolsoService, ReembolsoService>();
        
        return services;
    }
}
