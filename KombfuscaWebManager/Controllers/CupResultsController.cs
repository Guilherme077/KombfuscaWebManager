using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KombfuscaWebManager.Controllers
{
    public class CupResultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CupResultsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Register(int cupId)
        {
            if (await _context.CupResults.AnyAsync(c => c.CupId == cupId))
            {
                return BadRequest("The results for this cup has already been registered.");
            }

            var cup = await _context.Cups
                .Include(c => c.Periods)
                .FirstAsync(c => c.Id == cupId);

            var scores = await _context.ScoreSheets
                .Include(s => s.User)
                .Include(s => s.Period)
                .Where(s => s.Period.CopaId == cupId)
                .ToListAsync();

            var model = new RegisterCupResultViewModel
            {
                CupId = cupId,
                CupName = cup.Name
            };

            var players = scores
                .GroupBy(s => new
                {
                    s.UserId,
                    s.User.UserName
                });

            foreach (var player in players)
            {
                var vm = new PlayerCupResultViewModel
                {
                    UserId = player.Key.UserId,
                    UserName = player.Key.UserName!
                };

                var periods = player.GroupBy(x => new { x.PeriodId, x.Period.Description});

                foreach (var period in periods)
                {
                    var list = period.ToList();

                    var fuscas = list.Select(x => x.Fusca).Distinct().ToList();
                    var kombis = list.Select(x => x.Kombi).Distinct().ToList();
                    var beetles = list.Select(x => x.NewBeetle).Distinct().ToList();

                    bool div =
                        fuscas.Count > 1 ||
                        kombis.Count > 1 ||
                        beetles.Count > 1;

                    vm.HasDivergence |= div;

                    vm.Divergences.Add(
                        new PeriodDivergenceViewModel
                        {
                            PeriodId = period.Key.PeriodId,

                            PeriodDesc = period.Key.Description!,

                            FuscaValues = fuscas,

                            KombiValues = kombis,

                            NewBeetleValues = beetles,

                            FuscaFinal = fuscas.First(),

                            KombiFinal = kombis.First(),

                            NewBeetleFinal = beetles.First(),

                            FuscaDivergence = fuscas.Count > 1,

                            KombiDivergence = kombis.Count > 1,

                            NewBeetleDivergence = beetles.Count > 1
                        });

                    vm.Fusca += list.First().Fusca;
                    vm.Kombi += list.First().Kombi;
                    vm.NewBeetle += list.First().NewBeetle;
                }

                model.Players.Add(vm);
            }

            return View(model);
        }
    }
}
