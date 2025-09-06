using DjanabaApi1.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BulletinApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaiementController : ControllerBase
{
    private readonly string _connectionString;
    public PaiementController(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection");

    [HttpPost("bulk")]
    public async Task<IActionResult> Sync([FromBody] List<PaiementDto> paiements)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var p in paiements)
        {
            var cmd = new NpgsqlCommand(@"
                INSERT INTO paiement (id, eleveid, montant, methode, datepaiement)
                VALUES (@id, @eleveid, @montant, @methode, @datepaiement)
                ON CONFLICT (id) DO UPDATE
                SET eleveid=@eleveid, montant=@montant, methode=@methode, datepaiement=@datepaiement;", conn);

            cmd.Parameters.AddWithValue("id", p.Id);
            cmd.Parameters.AddWithValue("eleveid", p.EleveId);
            cmd.Parameters.AddWithValue("montant", p.Montant);
            cmd.Parameters.AddWithValue("methode", p.Methode);
            cmd.Parameters.AddWithValue("datepaiement", p.DatePaiement);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = new List<PaiementDto>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT id, eleveid, montant, methode, datepaiement FROM paiement;", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new PaiementDto
            {
                Id = reader.GetInt32(0),
                EleveId = reader.GetInt32(1),
                Montant = reader.GetDecimal(2),
                Methode = reader.GetString(3),
                DatePaiement = reader.GetDateTime(4)
            });
        }

        return Ok(result);
    }
}