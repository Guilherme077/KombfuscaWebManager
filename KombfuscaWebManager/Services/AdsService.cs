using KombfuscaWebManager.Models.AdModels;

namespace KombfuscaWebManager.Services
{
    public class AdsService
    {
        public Dictionary<int, AdCategory> RankBidsByCategory(
            IEnumerable<AuctionBid> bids,
            IEnumerable<AdCategory> categories)
        {
            var orderedCategories = categories
                .OrderByDescending(category => category.MinValue)
                .ThenBy(category => category.Id)
                .ToList();

            var availableSlots = orderedCategories.ToDictionary(
                category => category.Id,
                category => Math.Max(category.MaxAds, 0));
            var bidCategories = new Dictionary<int, AdCategory>();

            foreach (var bid in bids.OrderByDescending(bid => bid.Value).ThenBy(bid => bid.Id))
            {
                foreach (var category in orderedCategories)
                {
                    if (bid.Value < category.MinValue || availableSlots[category.Id] == 0)
                    {
                        continue;
                    }

                    bidCategories[bid.Id] = category;
                    availableSlots[category.Id]--;
                    break;
                }
            }

            return bidCategories;
        }
    }
}
