namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class ScoreConfirmationViewModel
    {
        public int PeriodId { get; set; }


        public List<PlayersScoreConfirmationViewModel> Players { get; set; }
            = new();
    }
}
