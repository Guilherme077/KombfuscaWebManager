namespace KombfuscaWebManager.Models.CupModels
{
    public class CupAssignment
    {
        public int Id { get; set; }

        public int CupId { get; set; }
        public Cup? Cup { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
