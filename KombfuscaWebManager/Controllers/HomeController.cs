using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using KombfuscaWebManager.Models.SystemModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cups = _context.Cups.Where(c => (c.cupStatus == CupStatus.openSubscriptions || c.cupStatus == CupStatus.closedSubscriptions) && c.StartDate > DateTime.Now).OrderBy(c => c.StartDate).ToList();
            return View(cups);
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
        [Authorize]
        public async Task<IActionResult> PlayerArea()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

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
                UserFullName = user?.FullName ?? user?.UserName,
                Victory = numberVictory,
                CupNumbers = numberCups,
                UserCups = userCups,
                CupsToSubscription = cupsToSub
            };

            return View(vm);
        }
        [Authorize]
        public async Task<IActionResult> ScoreCounterArea()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserFullName = user?.FullName ?? user?.UserName;
            return View();
        }
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AdminArea()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserFullName = user?.FullName ?? user?.UserName;
            return View();
        }
    }
}
