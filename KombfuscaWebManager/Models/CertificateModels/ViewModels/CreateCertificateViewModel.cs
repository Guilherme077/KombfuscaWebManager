using KombfuscaWebManager.Models.CupModels;

namespace KombfuscaWebManager.Models.CertificateModels.ViewModels
{
    public class CreateCertificateViewModel
    {

        public string UserId { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(150), System.ComponentModel.DataAnnotations.Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;
        public string CupName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(500), System.ComponentModel.DataAnnotations.Display(Name = "Texto acima do nome")]
        public string Description1 { get; set; } = string.Empty; // Text above the player's name
        public string PlayerName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(500), System.ComponentModel.DataAnnotations.Display(Name = "Texto abaixo do nome")]
        public string Description2 { get; set; } = string.Empty; // Text below the player's name
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int CupId { get; set; }

        public string CertificateFileModel { get; set; } = "Model1";

        //For reference:
        public CupResult? cupResult { get; set; }
    }
}
