using System.ComponentModel.DataAnnotations;

namespace CommerceApi.Models;

/// <summary>Credenciales enviadas al iniciar sesión.</summary>
public class LoginRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}