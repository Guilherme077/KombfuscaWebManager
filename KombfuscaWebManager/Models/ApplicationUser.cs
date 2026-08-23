using KombfuscaWebManager.Models.CupModels;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KombfuscaWebManager.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [Display(Name = "Nome Completo")]
    public string? FullName { get; set; }

    public bool MustChangePassword { get; set; }

    public ICollection<ScoreSheet> ScoreSheets { get; set; }
        = new List<ScoreSheet>();

    public ICollection<Participation> Participations { get; set; }
        = new List<Participation>();

    public ICollection<CupAssignment> CupAssignments { get; set; }
        = new List<CupAssignment>();
}