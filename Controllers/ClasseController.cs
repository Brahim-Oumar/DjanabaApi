using DjanabaApi1.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BulletinApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClasseController : ControllerBase
{
    private readonly string _connectionString;
    public ClasseController(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection");

    [HttpPost("bulk")]
    public async Task<IActionResult> Sync([FromBody] List<ClasseDto> classes)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var c in classes)
        {
            var cmd = new NpgsqlCommand(@"
                INSERT INTO classe (id, nom, montantscolarite, anneeid)
                VALUES (@id, @nom, @montant, @anneeid)
                ON CONFLICT (id) DO UPDATE
                SET nom=@nom, montantscolarite=@montant, anneeid=@anneeid;", conn);

            cmd.Parameters.AddWithValue("id", c.Id);
            cmd.Parameters.AddWithValue("nom", c.Nom);
            cmd.Parameters.AddWithValue("montant", c.MontantScolarite);
            cmd.Parameters.AddWithValue("anneeid", c.AnneeId);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = new List<ClasseDto>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT id, nom, montantscolarite, anneeid FROM classe;", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ClasseDto
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                MontantScolarite = reader.GetDecimal(2),
                AnneeId = reader.GetInt32(3)
            });
        }

        return Ok(result);
    }
}