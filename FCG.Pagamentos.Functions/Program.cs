using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using FCG.Pagamentos.Infrastructure;
using FCG.Pagamentos.Application;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Configuração das camadas de aplicação (reutilizando a estrutura existente)
        services.AddPagamentosDbContext(context.Configuration);
        services.AddPagamentosService();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
    })
    .Build();

host.Run();
