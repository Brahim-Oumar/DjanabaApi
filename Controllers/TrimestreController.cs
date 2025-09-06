using DjanabaApi1.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BulletinApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrimestreController : ControllerBase
{
    private readonly string _connectionString;
    public TrimestreController(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection");

    [HttpPost("bulk")]
    public async Task<IActionResult> Sync([FromBody] List<TrimestreDto> trimestres)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var t in trimestres)
        {
            var cmd = new NpgsqlCommand(@"
                INSERT INTO trimestre (id, anneeid, nom, debut, fin)
                VALUES (@id, @anneeid, @nom, @debut, @fin)
                ON CONFLICT (id) DO UPDATE
                SET anneeid=@anneeid, nom=@nom, debut=@debut, fin=@fin;", conn);

            cmd.Parameters.AddWithValue("id", t.Id);
            cmd.Parameters.AddWithValue("anneeid", t.AnneeId);
            cmd.Parameters.AddWithValue("nom", t.Nom);
            cmd.Parameters.AddWithValue("debut", t.Debut);
            cmd.Parameters.AddWithValue("fin", t.Fin);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = new List<TrimestreDto>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT id, anneeid, nom, debut, fin FROM trimestre;", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TrimestreDto
            {
                Id = reader.GetInt32(0),
                AnneeId = reader.GetInt32(1),
                Nom = reader.GetString(2),
                Debut = reader.GetDateTime(3),
                Fin = reader.GetDateTime(4)
            });
        }

        return Ok(result);
    }
}