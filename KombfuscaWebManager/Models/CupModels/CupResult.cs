namespace KombfuscaWebManager.Models.CupModels
{
    public class CupResult
    {
        public int Id { get; set; }

        public int CupId { get; set; }

        public string UserId { get; set; } = "";

        public int Position { get; set; }

        public int QtdKombi {  get; set; }

        public int QtdFusca { get; set; }

        public int QtdNewBeetle { get; set; }

        public int TotalScore { get; set; }

        public string TeamName { get; set; } = "";

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
