using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Barberia.Api.Data; 

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
        var email = string.IsNullOrWhiteSpace(model.Email) ? "_sinrol@barberia.com" : model.Email;

        // Generamos un token JWT válido directamente
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "ClaveSecretaSuperSeguraParaElProyectoDeBarberia2026");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("sub", "1"),
                new Claim(ClaimTypes.Email, email)
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
            usuarioId = 1 
        });
    }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
}