using KombfuscaWebManager.Models.CupModels;

namespace KombfuscaWebManager.Models.UsersModels.ViewModels;

public class UserProfileViewModel
{
    public string UserId { get; init; } = "";
    public string Name { get; init; } = "";
    public List<UserProfileCupViewModel> Cups { get; init; } = new();
    public int TotalCups { get; private set; }
    public int FirstPlaces { get; private set; }
    public int SecondPlaces { get; private set; }
    public int ThirdPlaces { get; private set; }
    public int CurrentWinStreak { get; private set; }
    public int BestWinStreak { get; private set; }
    public int VehicleCups { get; private set; }
    public int ExcludedVehicleCups { get; private set; }
    public long TotalKombi { get; private set; }
    public long TotalFusca { get; private set; }
    public long TotalNewBeetle { get; private set; }

    public static UserProfileViewModel Create(string userId, string? name, IEnumerable<UserProfileCupViewModel> cups)
    {
        var model = new UserProfileViewModel
        {
            UserId = userId,
            Name = string.IsNullOrWhiteSpace(name) ? "Competidor" : name,
            Cups = cups.OrderByDescending(c => c.StartDate).ThenByDescending(c => c.CupId).ToList()
        };

        foreach (var cup in model.Cups.AsEnumerable().Reverse())
        {
            if (cup.HasParticipated) model.TotalCups++;
            if (cup.Position == 1) model.FirstPlaces++;
            if (cup.Position == 2) model.SecondPlaces++;
            if (cup.Position == 3) model.ThirdPlaces++;

            // Ongoing cups do not alter the streak; an unpublished completed cup cannot establish a win.
            if (cup.Status is CupStatus.finished or CupStatus.finishedResultsAvailable)
            {
                model.CurrentWinStreak = cup.Position == 1 ? model.CurrentWinStreak + 1 : 0;
                model.BestWinStreak = Math.Max(model.BestWinStreak, model.CurrentWinStreak);
            }

            if (!cup.Position.HasValue) continue;
            if (!cup.HasPreciseVehicles)
            {
                model.ExcludedVehicleCups++;
                continue;
            }
            model.VehicleCups++;
            model.TotalKombi += cup.Kombi ?? 0;
            model.TotalFusca += cup.Fusca ?? 0;
            model.TotalNewBeetle += cup.NewBeetle ?? 0;
        }
        return model;
    }
}

public class UserProfileCupViewModel
{
    public int CupId { get; init; }
    public string Name { get; init; } = "";
    public DateTime StartDate { get; init; }
    public CupStatus Status { get; init; }
    public int? Position { get; init; }
    public int? TotalScore { get; init; }
    public int? Kombi { get; init; }
    public int? Fusca { get; init; }
    public int? NewBeetle { get; init; }
    public bool HasParticipated => Status is CupStatus.running or CupStatus.finished or CupStatus.finishedResultsAvailable;
    public bool HasPreciseVehicles => Position.HasValue && ((Fusca ?? 0) != 0 || (NewBeetle ?? 0) != 0);
    public string StatusLabel => Status switch
    {
        CupStatus.openSubscriptions or CupStatus.closedSubscriptions => "Inscrito",
        CupStatus.running => "Em andamento",
        _ => "Resultado ainda não publicado"
    };
}
