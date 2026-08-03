using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using KombfuscaWebManager.Models.SystemModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace KombfuscaWebManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> PlayerArea()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            var cupsToSub = await _context.Cups.Where(c => c.cupStatus == CupStatus.openSubscriptions).ToListAsync();
            var cupsParticipated = await _context.CupResults.Where(c => c.UserId == userId).ToListAsync();

            var userCups = new List<MyScoreViewModel>();

            foreach (var cupResult in cupsParticipated) {
                var cup = await _context.Cups.FirstOrDefaultAsync(c => c.Id == cupResult.CupId);
                userCups.Add(new MyScoreViewModel
                {
                    TeamName = cupResult.TeamName,
                    TotalScore = cupResult.TotalScore,
                    Position = cupResult.Position,
                    CupId = cup.Id,
                    CupName = cup.Name,
                    CupYear = cup.StartDate.Year,
                });
            }

            var numberCups = await _context.CupResults.Where(c => c.UserId == userId).CountAsync();
            var numberVictory = await _context.CupResults.Where(c => c.UserId == userId && c.Position == 1).CountAsync();

            var vm = new PlayerAreaViewModel()
            {
                Victory = numberVictory,
                CupNumbers = numberCups,
                UserCups = userCups,
                CupsToSubscription = cupsToSub
            };

            return View(vm);
        }
        public IActionResult ScoreCounterArea()
        {
            return View();
        }
        [Authorize(Roles = Roles.Admin)]
        public IActionResult AdminArea()
        {
            return View();
        }
    }
}
