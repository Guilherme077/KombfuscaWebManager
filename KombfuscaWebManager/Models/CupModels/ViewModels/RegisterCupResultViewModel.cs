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

        public List<PeriodsRegisterStatusViewModel> PeriodsRegisterStatus { get; set; }
            = new();
    }

    public class PeriodsRegisterStatusViewModel
    {
        public string PeriodDescription { get; set; }
        public string CounterName { get; set; }
        public PeriodRegisterStatus Status { get; set; }
    }

    public enum PeriodRegisterStatus
    {
        Registered,
        NotRegistered
    }
}
