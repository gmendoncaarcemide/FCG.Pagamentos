using FCG.Pagamentos.API.Endpoints;
using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.Services;
using FCG.Pagamentos.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FCG.Pagamentos.Domain.Pagamentos.Interfaces;
using FCG.Pagamentos.Infrastructure.Pagamentos.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/pagamentos-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuração dos serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do Entity Framework
builder.Services.AddDbContext<PagamentosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro dos serviços
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IReembolsoRepository, ReembolsoRepository>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IReembolsoService, ReembolsoService>();

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapeamento dos endpoints
app.MapTransacaoEndpoints();
app.MapReembolsoEndpoints();

// Migração automática do banco de dados
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PagamentosDbContext>();
    db.Database.Migrate();
}

app.Run(); 