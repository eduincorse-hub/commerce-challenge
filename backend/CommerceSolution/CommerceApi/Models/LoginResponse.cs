namespace CommerceApi.Models;

/// <summary>Resultado de un inicio de sesión correcto.</summary>
public record LoginResponse(string Token, DateTime ExpiresAt, string Username);