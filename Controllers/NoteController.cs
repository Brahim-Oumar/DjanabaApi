using DjanabaApi1.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BulletinApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly string _connectionString;
    public NoteController(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection");

    [HttpPost("bulk")]
    public async Task<IActionResult> Sync([FromBody] List<NoteDto> notes)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var n in notes)
        {
            var cmd = new NpgsqlCommand(@"
                INSERT INTO note (id, eleveid, competenceid, trimestreid, oral, ecrit, pratique, savoir)
                VALUES (@id, @eleveid, @competenceid, @trimestreid, @oral, @ecrit, @pratique, @savoir)
                ON CONFLICT (id) DO UPDATE
                SET eleveid=@eleveid, competenceid=@competenceid, trimestreid=@trimestreid,
                    oral=@oral, ecrit=@ecrit, pratique=@pratique, savoir=@savoir;", conn);

            cmd.Parameters.AddWithValue("id", n.Id);
            cmd.Parameters.AddWithValue("eleveid", n.EleveId);
            cmd.Parameters.AddWithValue("competenceid", n.CompetenceId);
            cmd.Parameters.AddWithValue("trimestreid", n.TrimestreId);
            cmd.Parameters.AddWithValue("oral", n.Oral);
            cmd.Parameters.AddWithValue("ecrit", n.Ecrit);
            cmd.Parameters.AddWithValue("pratique", n.Pratique);
            cmd.Parameters.AddWithValue("savoir", n.Savoir);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = new List<NoteDto>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT id, eleveid, competenceid, trimestreid, oral, ecrit, pratique, savoir FROM note;", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new NoteDto
            {
                Id = reader.GetInt32(0),
                EleveId = reader.GetInt32(1),
                CompetenceId = reader.GetInt32(2),
                TrimestreId = reader.GetInt32(3),
                Oral = reader.GetDouble(4),
                Ecrit = reader.GetDouble(5),
                Pratique = reader.GetDouble(6),
                Savoir = reader.GetDouble(7)
            });
        }

        return Ok(result);
    }
}