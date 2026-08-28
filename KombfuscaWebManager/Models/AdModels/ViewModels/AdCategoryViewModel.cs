using System.ComponentModel.DataAnnotations;

namespace KombfuscaWebManager.Models.AdModels.ViewModels
{
    public class AdCategoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxAds { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public double MinValue { get; set; }

        public int AdPeriodId { get; set; }
    }
}
