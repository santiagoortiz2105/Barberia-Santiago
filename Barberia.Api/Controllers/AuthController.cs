using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth; 
using Barberia.Api.Data; 
using Barberia.Api.Domain.Entities;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BarberiaDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(BarberiaDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest(new { mensaje = "El correo y la contraseña son obligatorios." });
        }

        var emailLimpio = model.Email.Trim().ToLower();

        // Buscamos al usuario en la base de datos
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLimpio, cancellationToken);
            
        if (usuario == null || usuario.PasswordHash != model.Password)
        {
            return Unauthorized(new { mensaje = "Correo electrónico o contraseña incorrectos." });
        }

        if (!usuario.Activo)
        {
            return StatusCode(403, new { codigo = "usuario_no_habilitado", mensaje = "Tu cuenta está desactivada." });
        }

        // Generamos el token JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "ClaveSecretaSuperSeguraParaElProyectoDeBarberia2026");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim("sub", usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Cliente")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new 
        { 
            token = tokenString, 
            mensaje = "Login exitoso",
            usuarioId = usuario.Id,
            rol = usuario.Rol
        });
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginDto model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.IdToken))
        {
            return BadRequest(new { mensaje = "El token de Google es obligatorio." });
        }

        var webClientId = _configuration["GoogleAuth:WebClientId"];
        if (string.IsNullOrEmpty(webClientId))
        {
            return StatusCode(500, new { mensaje = "El WebClientId de Google no está configurado en el servidor." });
        }

        try
        {
            // Validamos firma, vencimiento y audiencia del token de Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { webClientId }
            });

            if (!payload.EmailVerified)
            {
                return Unauthorized(new { codigo = "google_email_sin_verificar", mensaje = "El correo de Google no está verificado." });
            }

            var emailLimpio = (payload.Email ?? string.Empty).Trim().ToLowerInvariant();

            // Buscamos si el usuario ya existe
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLimpio, cancellationToken);

            // Si no existe, lo registramos automáticamente de forma transparente
            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Nombre = !string.IsNullOrEmpty(payload.Name) ? payload.Name.Trim() : "Usuario de Google",
                    Email = emailLimpio,
                    PasswordHash = string.Empty, // Usuarios de Google no usan contraseña local
                    Rol = "Cliente",
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else if (!usuario.Activo)
            {
                return StatusCode(403, new { codigo = "usuario_no_habilitado", mensaje = "Tu cuenta está desactivada. Contacta al administrador." });
            }

            // Emitimos la sesión JWT estándar de la app
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "ClaveSecretaSuperSeguraParaElProyectoDeBarberia2026");
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim("sub", usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol ?? "Cliente")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new 
            { 
                token = tokenString, 
                mensaje = "Login con Google exitoso",
                usuarioId = usuario.Id,
                rol = usuario.Rol
            });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { codigo = "google_invalido", mensaje = "Token de Google inválido." });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest(new { mensaje = "El correo y la contraseña son obligatorios." });
        }

        var emailLimpio = model.Email.Trim().ToLower();

        var existeUsuario = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == emailLimpio, cancellationToken);

        if (existeUsuario)
        {
            return BadRequest(new { mensaje = "El correo electrónico ya se encuentra registrado." });
        }

        var nuevoUsuario = new Usuario
        {
            Nombre = model.Nombre?.Trim() ?? "Usuario",
            Email = emailLimpio,
            PasswordHash = model.Password, 
            Rol = "Cliente",
            Activo = true 
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { mensaje = "Cuenta creada con éxito. Ya puedes iniciar sesión." });
    }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginDto
{
    public string IdToken { get; set; } = string.Empty;
}

public class RegisterDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}