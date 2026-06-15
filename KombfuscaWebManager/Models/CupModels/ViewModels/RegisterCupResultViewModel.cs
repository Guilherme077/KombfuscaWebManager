namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class RegisterCupResultViewModel
    {
        public int CupId { get; set; }

        public string CupName { get; set; } = "";

        public bool HasDivergence =>
            Players.Any(p => p.HasDivergence);

        public List<PlayerCupResultViewModel> Players { get; set; }
            = new();
    }
}
