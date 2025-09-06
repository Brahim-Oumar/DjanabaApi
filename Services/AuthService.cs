using Npgsql;
using WebApiAuth.Models;
using Microsoft.AspNetCore.Identity;

namespace WebApiAuth.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IConfiguration config)
    {
        _config = config;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        await using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        string query = "SELECT id, username, passwordhash FROM users WHERE username=@username LIMIT 1";
        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("username", username);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var user = new User
            {
                Id = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2)
            };

            // Vérification du mot de passe avec PasswordHasher
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Success)
                return user;
        }

        return null;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        await using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        // Vérifier si l’utilisateur existe déjà
        string checkQuery = "SELECT COUNT(1) FROM users WHERE username=@username";
        await using var checkCmd = new NpgsqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("username", username);
        var exists = (long)await checkCmd.ExecuteScalarAsync();
        if (exists > 0) return false;

        var user = new User { UserName = username };
        string passwordHash = _passwordHasher.HashPassword(user, password);

        string insertQuery = "INSERT INTO users (username, passwordhash) VALUES (@username, @passwordhash)";
        await using var insertCmd = new NpgsqlCommand(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("username", username);
        insertCmd.Parameters.AddWithValue("passwordhash", passwordHash);
        await insertCmd.ExecuteNonQueryAsync();

        return true;
    }
}