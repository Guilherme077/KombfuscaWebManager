using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using KombfuscaWebManager.Services;
using KombfuscaWebManager.Services.dto;
using KombfuscaWebManager.Models;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ScoreController(
            ScoreService _scoreService,
            ApplicationDbContext context,
            HttpClient httpClient,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            scoreService = _scoreService;
            _context = context;
            _httpClient = httpClient;
            _userManager = userManager;
            _configuration = configuration;
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

            var scoreCounterBaseUrl = _configuration["ScoreCounter:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ScoreCounter:BaseUrl não configurada.");
            var response = await _httpClient.PostAsync($"{scoreCounterBaseUrl}/scorecounter", content);

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

        [HttpGet]
        public async Task<IActionResult> MyScores()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            var cupResults = await _context.CupResults.Where(c => c.UserId == userId).ToListAsync();
            var certificates = await _context.Certificates
                .Where(c => c.UserId == userId)
                .ToDictionaryAsync(c => c.CupId);

            var myScores = new List<MyScoreViewModel>();

            foreach (var cupResult in cupResults)
            {
                var cup = await _context.Cups.FindAsync(cupResult.CupId);
                if (cup == null) continue;
                certificates.TryGetValue(cupResult.CupId, out var certificate);
                myScores.Add(new MyScoreViewModel
                {
                    UserName = User.Identity?.Name ?? "",
                    FullName = user?.FullName,
                    TeamName = cupResult.TeamName,
                    TotalScore = cupResult.TotalScore,
                    Kombi = cupResult.QtdKombi,
                    Fusca = cupResult.QtdFusca,
                    NewBeetle = cupResult.QtdNewBeetle,
                    Position = cupResult.Position,
                    CupId = cup.Id,
                    CupName = cup.Name,
                    CupYear = cup.StartDate.Year,
                    GeneratedAt = cupResult.GeneratedAt,
                    UserId = cupResult.UserId,
                    CertificateId = certificate?.Id,
                    CertificateStatus = certificate?.Status
                });
            }

            return View(myScores);
        }

        [HttpGet]
        public async Task<IActionResult> CupScores(int cupId)
        {
            var cupExists = await _context.Cups.Where(c => c.Id == cupId).FirstOrDefaultAsync();
            if (cupExists == null) return NotFound();
            var cupResults = await _context.CupResults.Where(c => c.CupId == cupId).ToListAsync();
            var certificates = await _context.Certificates
                .Where(c => c.CupId == cupId)
                .ToDictionaryAsync(c => c.UserId);

            var scores = new List<MyScoreViewModel>();

            foreach (var cupResult in cupResults)
            {
                var cup = await _context.Cups.FindAsync(cupResult.CupId);
                var user = await _context.Users.FindAsync(cupResult.UserId);
                if (cup == null || user == null) return NotFound();
                certificates.TryGetValue(cupResult.UserId, out var certificate);
                scores.Add(new MyScoreViewModel
                {
                    UserName = user.UserName ?? "",
                    FullName = user.FullName,
                    TeamName = cupResult.TeamName,
                    TotalScore = cupResult.TotalScore,
                    Kombi = cupResult.QtdKombi,
                    Fusca = cupResult.QtdFusca,
                    NewBeetle = cupResult.QtdNewBeetle,
                    Position = cupResult.Position,
                    CupId = cup.Id,
                    CupName = cup.Name,
                    CupYear = cup.StartDate.Year,
                    GeneratedAt = cupResult.GeneratedAt,
                    UserId = cupResult.UserId,
                    CertificateId = certificate?.Id,
                    CertificateStatus = certificate?.Status
                });
            }

            return View(scores);
        }

        [HttpGet]
        public async Task<IActionResult> CupStatistics(int cupId)
        {
            var cup = await _context.Cups
                .AsNoTracking()
                .Include(c => c.Periods)
                .FirstOrDefaultAsync(c => c.Id == cupId);

            if (cup == null) return NotFound();

            var results = await _context.CupResults
                .AsNoTracking()
                .Where(r => r.CupId == cupId)
                .OrderBy(r => r.Position)
                .ToListAsync();

            var userIds = results.Select(r => r.UserId).Distinct().ToList();
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            string CompetitorName(string userId)
            {
                if (!users.TryGetValue(userId, out var user)) return "Competidor";
                return !string.IsNullOrWhiteSpace(user.FullName)
                    ? user.FullName
                    : user.UserName ?? "Competidor";
            }

            var model = new CupStatisticsViewModel
            {
                CupId = cup.Id,
                CupName = cup.Name,
                CupYear = cup.StartDate.Year,
                TotalParticipants = results.Count,
                TotalKombi = results.Sum(r => r.QtdKombi),
                TotalFusca = results.Sum(r => r.QtdFusca),
                TotalNewBeetle = results.Sum(r => r.QtdNewBeetle),
                TotalVehicles = results.Sum(r => r.QtdKombi) + results.Sum(r => r.QtdFusca) + results.Sum(r => r.QtdNewBeetle),
                Competitors = results.Select(r => new CompetitorStatisticsViewModel
                {
                    UserId = r.UserId,
                    Name = CompetitorName(r.UserId),
                    Kombi = r.QtdKombi,
                    Fusca = r.QtdFusca,
                    NewBeetle = r.QtdNewBeetle,
                    TotalScore = r.TotalScore
                }).ToList()
            };

            var periods = cup.Periods.OrderBy(p => p.PaperNumber).ThenBy(p => p.Id).ToList();
            model.PeriodLabels = periods
                .Select(p => string.IsNullOrWhiteSpace(p.Description)
                    ? $"Papel {p.PaperNumber}"
                    : $"{p.Description}")
                .ToList();

            if (userIds.Count > 0 && periods.Count > 0)
            {
                var periodIds = periods.Select(p => p.Id).ToList();
                var scoreSheets = await _context.ScoreSheets
                    .AsNoTracking()
                    .Where(s => periodIds.Contains(s.PeriodId) && userIds.Contains(s.UserId))
                    .ToListAsync();

                foreach (var result in results)
                {
                    var cumulative = 0;
                    var points = new List<int?>();

                    for (var periodIndex = 0; periodIndex < periods.Count; periodIndex++)
                    {
                        var period = periods[periodIndex];

                        // CupResult is the official consolidated result. Always use it as
                        // the final point of the evolution, regardless of sheet divergences.
                        if (periodIndex == periods.Count - 1)
                        {
                            points.Add(result.TotalScore);
                            continue;
                        }

                        var readings = scoreSheets
                            .Where(s => s.UserId == result.UserId && s.PeriodId == period.Id)
                            .OrderBy(s => s.Id)
                            .ToList();

                        if (readings.Count == 0)
                        {
                            points.Add(null);
                            continue;
                        }

                        var firstReading = readings[0];
                        cumulative += firstReading.Kombi
                            + (firstReading.Fusca * 2)
                            + (firstReading.NewBeetle * 3);
                        points.Add(cumulative);
                    }

                    model.ScoreEvolution.Add(new ScoreEvolutionViewModel
                    {
                        Name = CompetitorName(result.UserId),
                        CumulativeScores = points
                    });
                }
            }

            return View(model);
        }
    }
}
