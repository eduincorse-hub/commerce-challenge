using CommerceApi.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CommerceApi.Data;

/// <summary>Acceso a datos de usuarios.</summary>
public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<bool> AnyAsync();
    Task CreateAsync(string username, string passwordHash);
}

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("CommerceDb")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'CommerceDb'.");
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.QuerySingleOrDefaultAsync<AppUser>(
            @"SELECT id AS Id, username AS Username, password_hash AS PasswordHash
              FROM dbo.app_user WHERE username = @username",
            new { username });
    }

    public async Task<bool> AnyAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.ExecuteScalarAsync<bool>("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.app_user) THEN 1 ELSE 0 END");
    }

    public async Task CreateAsync(string username, string passwordHash)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(
            "INSERT INTO dbo.app_user (username, password_hash) VALUES (@username, @passwordHash)",
            new { username, passwordHash });
    }
}