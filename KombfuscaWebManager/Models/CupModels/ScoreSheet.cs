using Azure;
using KombfuscaWebManager.Models;

namespace KombfuscaWebManager.Models.CupModels
{
    public class ScoreSheet
    {
        public int Id { get; set; }

        public int Fusca { get; set; }

        public int Kombi { get; set; }

        public int NewBeetle { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public int PeriodId { get; set; }

        public Period Period { get; set; } = null!;
    }
}
