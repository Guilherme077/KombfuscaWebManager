namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class ScoreIndexViewModel
    {
        public List<CupGroupViewModel> CupGroups { get; set; } = new();
    }

    public class CupGroupViewModel
    {
        public int CupId { get; set; }
        public string CupName { get; set; } = string.Empty;
        public List<PeriodScoreViewModel> PendingPeriods { get; set; } = new();
        public List<PeriodScoreViewModel> CompletedPeriods { get; set; } = new();
    }

    public class PeriodScoreViewModel
    {
        public int PeriodId { get; set; }
        public int PaperNumber { get; set; }
        public int CupId { get; set; }
        public string? Description { get; set; }
        public bool HasScore { get; set; }
        public DateTime? ScoreCreatedAt { get; set; }
    }
}
