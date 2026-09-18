using KombfuscaWebManager.Models.CupModels;

namespace KombfuscaWebManager.Models.CertificateModels
{
    public class Certificate
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int CupId { get; set; }
        public Cup? Cup { get; set; } // Cup here is used to control the relationship between the certificate and the cup, but it is not used in the certificate generation process.

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(150)]
        public string Title { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(150)]
        public string CupName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(500)]
        public string Description1 { get; set; } = string.Empty; // Text above the player's name
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(150)]
        public string PlayerName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(500)]
        public string Description2 { get; set; } = string.Empty; // Text below the player's name
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(50)]
        public string CertificateFileModel { get; set; } = "Model1";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(25)]
        public string ValidationCode { get; set; } = string.Empty;

        public CertificateStatus Status { get; set; } = CertificateStatus.Active;

        public DateTime? RevokedAt { get; set; }

        public string? RevocationReason { get; set; }

    }
    public enum CertificateStatus
    {
        Active,
        Revoked
    }
}
