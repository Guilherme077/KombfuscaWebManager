namespace KombfuscaWebManager.Models.CupModels.ViewModels
{
    public class CupAssignmentViewModel
    {
        public int CupId { get; set; }
        public string CupName { get; set; } = string.Empty;
        public List<UserAssignmentViewModel> UserAssignments { get; set; } = new();
    }

    public class UserAssignmentViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}
