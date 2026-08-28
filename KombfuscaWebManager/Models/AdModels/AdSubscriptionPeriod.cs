using KombfuscaWebManager.Models.CupModels;

namespace KombfuscaWebManager.Models.AdModels
{
    public class AdSubscriptionPeriod
    {
        public int Id { get; set; }
        public required Cup Cup { get; set; }
        public int CupId { get; set; }
        public DateTime StartSubscription { get; set; }
        public DateTime EndSubscription { get; set; }
        public DateTime StartAuction { get; set; }
        public DateTime EndAuction { get; set; }
        public DateTime SituationReviewDate { get; set; } //Date when the situation of the ads will be reviewed and the winners will be announced
        List<AdCategory> Categories { get; set; } = new List<AdCategory>();
    }
}
