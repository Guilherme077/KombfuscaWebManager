namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class CupStatisticsViewModel
    {
        public int CupId { get; set; }
        public string CupName { get; set; } = string.Empty;
        public int CupYear { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalKombi { get; set; }
        public int TotalFusca { get; set; }
        public int TotalNewBeetle { get; set; }
        public List<CompetitorStatisticsViewModel> Competitors { get; set; } = new();
        public List<string> PeriodLabels { get; set; } = new();
        public List<ScoreEvolutionViewModel> ScoreEvolution { get; set; } = new();
    }

    public class CompetitorStatisticsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Kombi { get; set; }
        public int Fusca { get; set; }
        public int NewBeetle { get; set; }
        public int TotalScore { get; set; }
    }

    public class ScoreEvolutionViewModel
    {
        public string Name { get; set; } = string.Empty;
        public List<int?> CumulativeScores { get; set; } = new();
    }
}
