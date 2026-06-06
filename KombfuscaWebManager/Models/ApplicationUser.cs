using KombfuscaWebManager.Models.CupModels;
using Microsoft.AspNetCore.Identity;

namespace KombfuscaWebManager.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<ScoreSheet> ScoreSheets { get; set; }
        = new List<ScoreSheet>();

    public ICollection<Participation> Participations { get; set; }
        = new List<Participation>();

    public ICollection<CupAssignment> CupAssignments { get; set; }
        = new List<CupAssignment>();
}