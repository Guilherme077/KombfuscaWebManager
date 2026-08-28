namespace KombfuscaWebManager.Models.AdModels
{
    public class AdCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxAds { get; set; }
        public int MinValue { get; set; }
    }
}
