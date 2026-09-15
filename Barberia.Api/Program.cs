using Barberia.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Configurar conexión a MySQL
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<BarberiaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//Configuración de JWT y validación de tokens 
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSecretaSuperSeguraParaElProyectoDeBarberia2026";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };

    //Confirma contra la base que el usuario siga activo y su rol no haya cambiado
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async contexto =>
        {
            var db = contexto.HttpContext.RequestServices.GetRequiredService<BarberiaDbContext>();
            
            // Extraer el ID del usuario del claim 'sub' o NameIdentifier
            var userIdClaim = contexto.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                              ?? contexto.Principal?.FindFirst("sub")?.Value;

            if (userIdClaim == null || !long.TryParse(userIdClaim, out var usuarioId))
            {
                contexto.Fail("Token inválido.");
                return;
            }

            // Consultar base de datos
            var usuario = await db.Usuarios.AsNoTracking()
                .Where(x => x.Id == usuarioId)
                .Select(x => new { x.Activo, x.RolId })
                .FirstOrDefaultAsync();

            if (usuario == null || !usuario.Activo)
            {
                contexto.Fail("El usuario ya no está activo.");
                return;
            }
        }
    };
});


//Agregar Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//Configurar CORS para permitir peticiones desde el frontend o teléfono
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


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();