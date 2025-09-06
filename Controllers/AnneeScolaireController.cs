using Microsoft.AspNetCore.Mvc;
using Npgsql;
using DjanabaApi1.Models;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly IConfiguration _config;
    public SyncController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost]
    public IActionResult SyncAll([FromBody] SyncRequest request)
    {
        using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        conn.Open();

        using var tx = conn.BeginTransaction();
        try
        {
            // 1. Années scolaires
            foreach (var annee in request.Annees)
            {
                using var cmd = new NpgsqlCommand("INSERT INTO anneescolaire (id, nom, datedebut, datefin) VALUES (@id,@nom,@deb,@fin) ON CONFLICT (id) DO UPDATE SET nom=@nom, datedebut=@deb, datefin=@fin;", conn, tx);
                cmd.Parameters.AddWithValue("id", annee.Id);
                cmd.Parameters.AddWithValue("nom", annee.Nom);
                cmd.Parameters.AddWithValue("deb", annee.DateDebut);
                cmd.Parameters.AddWithValue("fin", annee.DateFin);
                cmd.ExecuteNonQuery();
            }

            // 2. Trimestres
            foreach (var tri in request.Trimestres)
            {
                using var cmd = new NpgsqlCommand("INSERT INTO trimestre (id, anneeid, nom, debut, fin) VALUES (@id,@aid,@nom,@deb,@fin) ON CONFLICT (id) DO UPDATE SET anneeid=@aid, nom=@nom, debut=@deb, fin=@fin;", conn, tx);
                cmd.Parameters.AddWithValue("id", tri.Id);
                cmd.Parameters.AddWithValue("aid", tri.AnneeId);
                cmd.Parameters.AddWithValue("nom", tri.Nom);
                cmd.Parameters.AddWithValue("deb", tri.Debut);
                cmd.Parameters.AddWithValue("fin", tri.Fin);
                cmd.ExecuteNonQuery();
            }

            // 3. Classes
            foreach (var classe in request.Classes)
            {
                using var cmd = new NpgsqlCommand("INSERT INTO classe (id, nom, montantscolarite, anneeid) VALUES (@id,@nom,@mont,@aid) ON CONFLICT (id) DO UPDATE SET nom=@nom, montantscolarite=@mont, anneeid=@aid;", conn, tx);
                cmd.Parameters.AddWithValue("id", classe.Id);
                cmd.Parameters.AddWithValue("nom", classe.Nom);
                cmd.Parameters.AddWithValue("mont", classe.MontantScolarite);
                cmd.Parameters.AddWithValue("aid", classe.AnneeId);
                cmd.ExecuteNonQuery();
            }

            // 4. Élèves
            foreach (var e in request.Eleves)
            {
                using var cmd = new NpgsqlCommand("INSERT INTO eleve (id, classeid, nom, prenom, scolarite, anneeid) VALUES (@id,@cid,@nom,@pre,@sco,@aid) ON CONFLICT (id) DO UPDATE SET classeid=@cid, nom=@nom, prenom=@pre, scolarite=@sco, anneeid=@aid;", conn, tx);
                cmd.Parameters.AddWithValue("id", e.Id);
                cmd.Parameters.AddWithValue("cid", e.ClasseId);
                cmd.Parameters.AddWithValue("nom", e.Nom);
                cmd.Parameters.AddWithValue("pre", e.Prenom);
                cmd.Parameters.AddWithValue("sco", e.Scolarite);
                cmd.Parameters.AddWithValue("aid", e.AnneeId);
                cmd.ExecuteNonQuery();
            }

            // 5. Notes
            foreach (var n in request.Notes)
            {
                using var cmd = new NpgsqlCommand("INSERT INTO note (id, eleveid, competenceid, trimestreid, oral, ecrit, pratique, savoir) VALUES (@id,@eid,@cid,@tid,@oral,@ecrit,@pratique,@savoir) ON CONFLICT (id) DO UPDATE SET eleveid=@eid, competenceid=@cid, trimestreid=@tid, oral=@oral, ecrit=@ecrit, pratique=@pratique, savoir=@savoir;", conn, tx);
                cmd.Parameters.AddWithValue("id", n.Id);
                cmd.Parameters.AddWithValue("eid", n.EleveId);
                cmd.Parameters.AddWithValue("cid", n.CompetenceId);
                cmd.Parameters.AddWithValue("tid", n.TrimestreId);
                cmd.Parameters.AddWithValue("oral", n.Oral);
                cmd.Parameters.AddWithValue("ecrit", n.Ecrit);
                cmd.Parameters.AddWithValue("pratique", n.Pratique);
                cmd.Parameters.AddWithValue("savoir", n.Savoir);
                cmd.ExecuteNonQuery();
            }

            // 6. Paiements
            foreach (var p in request.Paiements)
            {
                using var cmd = new NpgsqlCommand("INSERT INTO paiement (id, eleveid, montant, methode, datepaiement) VALUES (@id,@eid,@mont,@meth,@date) ON CONFLICT (id) DO UPDATE SET eleveid=@eid, montant=@mont, methode=@meth, datepaiement=@date;", conn, tx);
                cmd.Parameters.AddWithValue("id", p.Id);
                cmd.Parameters.AddWithValue("eid", p.EleveId);
                cmd.Parameters.AddWithValue("mont", p.Montant);
                cmd.Parameters.AddWithValue("meth", p.Methode);
                cmd.Parameters.AddWithValue("date", p.DatePaiement);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return Ok(new { message = "Synchronisation réussie ✅" });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return StatusCode(500, ex.Message);
        }
    }
}

public class SyncRequest
{
    public List<AnneeScolaireDto> Annees { get; set; } = new();
    public List<TrimestreDto> Trimestres { get; set; } = new();
    public List<ClasseDto> Classes { get; set; } = new();
    public List<EleveDto> Eleves { get; set; } = new();
    public List<NoteDto> Notes { get; set; } = new();
    public List<PaiementDto> Paiements { get; set; } = new();
}