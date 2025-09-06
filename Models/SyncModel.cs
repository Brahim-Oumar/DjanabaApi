namespace DjanabaApi1.Models
{
   // namespace BulletinApi.Dtos;

    public class AnneeScolaireDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
    }

    public class TrimestreDto
    {
        public int Id { get; set; }
        public int AnneeId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public DateTime Debut { get; set; }
        public DateTime Fin { get; set; }
    }

    public class ClasseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal MontantScolarite { get; set; }
        public int AnneeId { get; set; }
    }

    public class EleveDto
    {
        public int Id { get; set; }
        public int ClasseId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public decimal Scolarite { get; set; }
        public int AnneeId { get; set; }
    }

    public class NoteDto
    {
        public int Id { get; set; }
        public int EleveId { get; set; }
        public int CompetenceId { get; set; }
        public int TrimestreId { get; set; }
        public double Oral { get; set; }
        public double Ecrit { get; set; }
        public double Pratique { get; set; }
        public double Savoir { get; set; }
    }

    public class PaiementDto
    {
        public int Id { get; set; }
        public int EleveId { get; set; }
        public decimal Montant { get; set; }
        public string Methode { get; set; } = string.Empty;
        public DateTime DatePaiement { get; set; }
    }
}
