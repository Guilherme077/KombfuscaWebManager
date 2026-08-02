namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class CupSubscriptionStatusViewModel
    {
        public int CupId { get; set; }
        public string CupName { get; set; } = string.Empty;
        public string Placename { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Year { get; set; }
        public double SubscriptionFee { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public bool SubscriptionFeePayed { get; set; }
    }
}
