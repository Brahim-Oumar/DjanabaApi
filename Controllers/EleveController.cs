using DjanabaApi1.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BulletinApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EleveController : ControllerBase
{
    private readonly string _connectionString;
    public EleveController(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection");

    [HttpPost("bulk")]
    public async Task<IActionResult> Sync([FromBody] List<EleveDto> eleves)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var e in eleves)
        {
            var cmd = new NpgsqlCommand(@"
                INSERT INTO eleve (id, classeid, nom, prenom, scolarite, anneeid)
                VALUES (@id, @classeid, @nom, @prenom, @scolarite, @anneeid)
                ON CONFLICT (id) DO UPDATE
                SET classeid=@classeid, nom=@nom, prenom=@prenom, scolarite=@scolarite, anneeid=@anneeid;", conn);

            cmd.Parameters.AddWithValue("id", e.Id);
            cmd.Parameters.AddWithValue("classeid", e.ClasseId);
            cmd.Parameters.AddWithValue("nom", e.Nom);
            cmd.Parameters.AddWithValue("prenom", e.Prenom);
            cmd.Parameters.AddWithValue("scolarite", e.Scolarite);
            cmd.Parameters.AddWithValue("anneeid", e.AnneeId);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = new List<EleveDto>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT id, classeid, nom, prenom, scolarite, anneeid FROM eleve;", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new EleveDto
            {
                Id = reader.GetInt32(0),
                ClasseId = reader.GetInt32(1),
                Nom = reader.GetString(2),
                Prenom = reader.GetString(3),
                Scolarite = reader.GetDecimal(4),
                AnneeId = reader.GetInt32(5)
            });
        }

        return Ok(result);
    }
}