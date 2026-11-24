
using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces;
using PrintFlow.Infrastructure.Persistence;
using PrintFlow.Infrastructure.Repositories;
using PrintFlow.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar el contexto de Entity Framework Core.
builder.Services.AddDbContext<PrintFlowDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Registrar el repositorio de pedidos.
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

// Registrar el servicio de registro de pedidos.
builder.Services.AddScoped<RegistrarPedidoService>();

// Registrar documentación OpenAPI.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configurar OpenAPI en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
