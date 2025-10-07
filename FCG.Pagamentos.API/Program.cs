using FCG.Pagamentos.Infrastructure;
using FCG.Pagamentos.Application;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/pagamentos-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FCG.Pagamentos.API", Version = "v1" });
});

builder.Services.AddPagamentosDbContext(builder.Configuration);
builder.Services.AddPagamentosService();
builder.Services.AddControllers();

builder.Services.AddHttpClient<FCG.Pagamentos.Application.Pagamentos.Services.IAzureFunctionService, 
    FCG.Pagamentos.Application.Pagamentos.Services.AzureFunctionService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PagamentosDbContext>();
    db.Database.Migrate();
}

app.Run(); 