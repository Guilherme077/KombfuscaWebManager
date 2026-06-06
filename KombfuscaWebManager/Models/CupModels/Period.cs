namespace KombfuscaWebManager.Models.CupModels
{
    public class Period
    {
        public int Id { get; set; }

        public int PaperNumber { get; set; }

        public string? Description { get; set; } = string.Empty;

        public int CopaId { get; set; }

        public Cup? Copa { get; set; }

        public ICollection<ScoreSheet> ScoreSheets { get; set; }
            = new List<ScoreSheet>();
    }
}
