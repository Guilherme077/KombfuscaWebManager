using Azure;

namespace KombfuscaWebManager.Models.CupModels
{
    public class Cup
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public int Year { get; set; }

        public string Placename { get; set; } = string.Empty;

        public DateTime? EndDate { get; set; }

        public double SubscriptionFee { get; set; }

        public CupStatus cupStatus { get; set; }

        public ICollection<Period> Periods { get; set; }
            = new List<Period>();
    }

    public enum CupStatus
    {
        openSubscriptions,
        closedSubscriptions,
        running,
        finished,
        finishedResultsAvailable
    }
}
