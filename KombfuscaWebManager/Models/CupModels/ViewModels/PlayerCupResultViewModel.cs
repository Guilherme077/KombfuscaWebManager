namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class PlayerCupResultViewModel
    {
        public string UserId { get; set; } = "";

        public string UserName { get; set; } = "";

        public int Fusca { get; set; }

        public int Kombi { get; set; }

        public int NewBeetle { get; set; }

        public bool HasDivergence { get; set; }

        public List<PeriodDivergenceViewModel> Divergences { get; set; }
        public PlayerCupResultViewModel()
        {
            Divergences = new List<PeriodDivergenceViewModel>();
        }
    }
}
