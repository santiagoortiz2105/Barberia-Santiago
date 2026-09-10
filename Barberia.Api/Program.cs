using Barberia.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar conexión a MySQL
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<BarberiaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Agregar Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. Configurar CORS para permitir peticiones desde el frontend o teléfono
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("PermitirTodo");
app.UseAuthorization();
app.MapControllers();

// Endpoint de prueba de vida obligatorio según la guía
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();