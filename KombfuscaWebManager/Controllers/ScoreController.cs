using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using KombfuscaWebManager.Services;
using KombfuscaWebManager.Services.dto;
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

            // Get all cups with their periods
            var cupsWithPeriods = await _context.Cups
                .Include(c => c.Periods)
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
        public IActionResult RegisterScore(int periodId)
        {
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


            //Connect to API
            using var content = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();

            content.Add(new StreamContent(stream), "picture", image.FileName);

            var response = await _httpClient.PostAsync("http://localhost:5000/scorecounter", content);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Status: {response.StatusCode}\n\nResposta:\n{body}");
            }

            string json = await response.Content.ReadAsStringAsync();

            var resultado = JsonSerializer.Deserialize<Dictionary<string, ResultScoreCounterAPI>>(json);

            var model = new ScoreConfirmationViewModel
            {
                PeriodId = periodId
            };

            foreach (var item in resultado)
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
