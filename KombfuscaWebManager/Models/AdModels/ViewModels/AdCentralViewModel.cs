using KombfuscaWebManager.Controllers;

namespace KombfuscaWebManager.Models.AdModels.ViewModels
{
    public class AdCentralViewModel
    {
        public AdPeriodStatus AdPeriodStatus { get; set; }

        public AdSubscriptionPeriod? CurrentAdPeriod { get; set; }

        public List<AdRequest> UserAdRequest { get; set; } = [];

        public List<AuctionBid> UserValidAuctionBids { get; set; } = [];

        public List<AuctionBid> AllValidAuctionBids { get; set; } = [];

        public Dictionary<int, AdCategory> BidCategories { get; set; } = [];
    }
}
