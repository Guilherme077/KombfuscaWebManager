using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;

namespace KombfuscaWebManager.Models.SystemModels.ViewModels
{
    public class PlayerAreaViewModel
    {
        public int Victory { get; set; }
        public int CupNumbers { get; set; }
        public List<MyScoreViewModel> UserCups { get; set; }
        public List<Cup> CupsToSubscription { get; set; }
    }
}
