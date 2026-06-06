namespace KombfuscaWebManager.Models.UsersModels.ViewModels
{
    public class EditRolesViewModel
    {
        public string UserId { get; set; } = "";

        public string UserName { get; set; } = "";

        public bool Admin { get; set; }

        public bool ScoreCounter { get; set; }

        public bool Player { get; set; }
    }
}
