namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class PeriodDivergenceViewModel
    {
        public int PeriodId { get; set; }

        public string PeriodDesc { get; set; }

        public List<int> FuscaValues { get; set; } = new();

        public List<int> KombiValues { get; set; } = new();

        public List<int> NewBeetleValues { get; set; } = new();

        public int FuscaFinal { get; set; }

        public int KombiFinal { get; set; }

        public int NewBeetleFinal { get; set; }

        public bool FuscaDivergence { get; set; }

        public bool KombiDivergence { get; set; }

        public bool NewBeetleDivergence { get; set; }
    }
}
