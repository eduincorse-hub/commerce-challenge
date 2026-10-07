using CommerceApi.Models;
using CommerceApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

/// <summary>Autenticación de usuarios.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Inicia sesión y devuelve un token JWT.</summary>
    /// <response code="200">Credenciales correctas: devuelve el token.</response>
    /// <response code="401">Usuario o contraseña incorrectos.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return result is null
            ? Unauthorized(new { message = "Usuario o contraseña incorrectos." })
            : Ok(result);
    }
}