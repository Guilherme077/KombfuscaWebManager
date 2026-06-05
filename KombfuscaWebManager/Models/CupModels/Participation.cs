namespace KombfuscaWebManager.Models.CupModels
{
    public class Participation
    {
        public int Id { get; set; }

        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string TeamName = string.Empty;
    }
}
