namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class MyScoreViewModel
    {
        public string UserName { get; set; }
        public string? FullName { get; set; }
        public string TeamName { get; set; }
        public int TotalScore { get; set; }
        public int Kombi { get; set; }
        public int Fusca { get; set; }
        public int NewBeetle { get; set; }
        public int Position { get; set; }
        public int CupId { get; set; }
        public string CupName { get; set; }
        public int CupYear { get; set; }
        public DateTime GeneratedAt { get; set; }

    }
}
