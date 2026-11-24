
using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces;
using PrintFlow.Infrastructure.Persistence;
using PrintFlow.Infrastructure.Repositories;
using PrintFlow.Application.Services;
using PrintFlow.Application.Interfaces.Documentos;
using PrintFlow.Infrastructure.Storage;

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

// Registrar la configuración del almacenamiento de documentos.
builder.Services.Configure<AlmacenamientoDocumentosOptions>(
    builder.Configuration.GetSection("AlmacenamientoDocumentos")
);

// Registrar el servicio de almacenamiento físico.
builder.Services.AddScoped<
    IAlmacenamientoDocumentos,
    AlmacenamientoDocumentos
>();

// Registrar el servicio de análisis de documentos.
builder.Services.AddScoped<
    IAnalizadorDocumentos,
    AnalizadorDocumentos
>();

// Registrar documentación OpenAPI.
builder.Services.AddOpenApi();

builder.Services.AddControllers();

var app = builder.Build();

// Configurar OpenAPI en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
