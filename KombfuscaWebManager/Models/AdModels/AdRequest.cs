namespace KombfuscaWebManager.Models.AdModels
{
    public class AdRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public AdSubscriptionPeriod? SubscriptionPeriod { get; set; }
        public string OficialName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Slogan { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public string StatusMessage { get; set; } = string.Empty;
    }
    public enum RequestStatus
    {
        Approved,
        Rejected,
        Pending
    }
}
