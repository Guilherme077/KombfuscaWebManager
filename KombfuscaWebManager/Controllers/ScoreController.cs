using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using KombfuscaWebManager.Services;
using KombfuscaWebManager.Services.dto;
using KombfuscaWebManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace KombfuscaWebManager.Controllers
{
    public class ScoreController : Controller
    {
        public ScoreService scoreService;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public ScoreController(ScoreService _scoreService, ApplicationDbContext context, HttpClient httpClient)
        {
            scoreService = _scoreService;
            _context = context;
            _httpClient = httpClient;
        }
        
        public async Task<IActionResult> Index()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            // Check if user is admin
            bool isAdmin = User.IsInRole("Admin");

            // Get assigned cup IDs for this user
            var assignedCupIds = new List<int>();
            if (!isAdmin)
            {
                assignedCupIds = await _context.CupAssignments
                    .Where(ca => ca.UserId == userId)
                    .Select(ca => ca.CupId)
                    .ToListAsync();

                // If user has no assignments and is not admin, show empty list
                if (assignedCupIds.Count == 0)
                {
                    var emptyModel = new ScoreIndexViewModel();
                    return View(emptyModel);
                }
            }

            // Get cups - filter by assignment if user is not admin
            IQueryable<Cup> cupsQuery = _context.Cups.Include(c => c.Periods);
            if (!isAdmin)
            {
                cupsQuery = cupsQuery.Where(c => assignedCupIds.Contains(c.Id));
            }

            var cupsWithPeriods = await cupsQuery
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            // Get all score sheets created by this user
            var userScoreSheets = await _context.ScoreSheets
                .Where(s => s.CreatedByUserId == userId)
                .Select(s => s.PeriodId)
                .ToListAsync();

            var model = new ScoreIndexViewModel();

            foreach (var cup in cupsWithPeriods)
            {
                var cupGroup = new CupGroupViewModel
                {
                    CupId = cup.Id,
                    CupName = cup.Name
                };

                foreach (var period in cup.Periods.OrderBy(p => p.PaperNumber))
                {
                    var hasScore = userScoreSheets.Contains(period.Id);

                    var periodViewModel = new PeriodScoreViewModel
                    {
                        PeriodId = period.Id,
                        PaperNumber = period.PaperNumber,
                        CupId = cup.Id,
                        Description = period.Description,
                        HasScore = hasScore
                    };

                    if (hasScore)
                    {
                        var scoreSheet = await _context.ScoreSheets
                            .Where(s => s.PeriodId == period.Id && s.CreatedByUserId == userId)
                            .FirstOrDefaultAsync();

                        if (scoreSheet != null)
                        {
                            periodViewModel.ScoreCreatedAt = scoreSheet.CreatedAt;
                        }

                        cupGroup.CompletedPeriods.Add(periodViewModel);
                    }
                    else
                    {
                        cupGroup.PendingPeriods.Add(periodViewModel);
                    }
                }

                if (cupGroup.PendingPeriods.Any() || cupGroup.CompletedPeriods.Any())
                {
                    model.CupGroups.Add(cupGroup);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterScore(int periodId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Check authorization
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                // Get cup ID for this period
                var period = await _context.Periods.FindAsync(periodId);
                if (period == null) return NotFound();

                // Check if user has assignment to this cup
                var assignment = await _context.CupAssignments
                    .AnyAsync(ca => ca.CupId == period.CopaId && ca.UserId == userId);

                if (!assignment)
                {
                    return Forbid();
                }
            }

            ViewBag.PeriodId = periodId;
            return View();
        }

        [HttpGet]
        public IActionResult ConfirmScoreSheet()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadScoreSheet(IFormFile image, int periodId)
        {
            if (image == null) return BadRequest();

            string? uploadUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (uploadUserId == null) return Unauthorized();

            // Check authorization
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                // Get cup ID for this period
                var periodInfo = await _context.Periods.FindAsync(periodId);
                if (periodInfo == null) return NotFound();

                // Check if user has assignment to this cup
                var assignment = await _context.CupAssignments
                    .AnyAsync(ca => ca.CupId == periodInfo.CopaId && ca.UserId == uploadUserId);

                if (!assignment)
                {
                    return Forbid();
                }
            }


            //Connect to API
            using var content = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();

            content.Add(new StreamContent(stream), "picture", image.FileName);

            var response = await _httpClient.PostAsync("http://localhost:5000/scorecounter", content);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Status: {response.StatusCode}\n\nResponse:\n{body}");
            }

            string json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ScoreCounterResponse>(json);

            var model = new ScoreConfirmationViewModel
            {
                PeriodId = periodId,
                ProcessedImage = result.ProcessedImage
            };

            foreach (var item in result.Players)
            {
                model.Players.Add(
                    new PlayersScoreConfirmationViewModel
                    {
                        UserId = uploadUserId, // Change to player's user ID when available
                        Name = "Jogador Teste", // Change to player's name when available

                        Fusca = item.Value.Fusca,
                        Kombi = item.Value.Kombi,
                        NewBeetle = item.Value.NewBeetle
                    });
            }

            // Load participations for the cup of this period
            var period = await _context.Periods
                .Include(p => p.Copa)
                .FirstOrDefaultAsync(p => p.Id == periodId);

            if (period != null)
            {
                var participations = await _context.Participations
                    .Where(p => p.CupId == period.CopaId)
                    .Include(p => p.User)
                    .ToListAsync();

                foreach (var participation in participations)
                {
                    model.AvailableParticipations.Add(
                        new ParticipationDropdownViewModel
                        {
                            UserId = participation.UserId,
                            UserName = participation.User?.UserName ?? "",
                            TeamName = participation.TeamName
                        });
                }
            }

            return View("ConfirmScoreSheet", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveScoreSheet(ScoreConfirmationViewModel model)
        {

            string? uploadUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (uploadUserId == null) return Unauthorized();

            foreach (var player in model.Players)
            {
                var scoreSheet = new ScoreSheet
                {
                    UserId = player.UserId,
                    CreatedByUserId = uploadUserId,
                    PeriodId = model.PeriodId,
                    Fusca = player.Fusca,
                    Kombi = player.Kombi,
                    NewBeetle = player.NewBeetle
                };

                _context.ScoreSheets.Add(scoreSheet);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Periods", new { id = model.PeriodId });
        }
    }
}
