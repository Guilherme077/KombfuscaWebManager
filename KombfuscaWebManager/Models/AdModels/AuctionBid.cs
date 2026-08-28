namespace KombfuscaWebManager.Models.AdModels
{
    public class AuctionBid
    {
        public int Id { get; set; }
        public required AdRequest Request { get; set; }
        public double Value { get; set; }
        public bool Valid { get; set; } = true;
    }
}
