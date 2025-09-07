using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FCG.Pagamentos.Domain.Pagamentos.Interfaces;
using FCG.Pagamentos.Infrastructure.Pagamentos.Repositories;

namespace FCG.Pagamentos.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPagamentosDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PagamentosDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITransacaoRepository, TransacaoRepository>();

        return services;
    }
}
