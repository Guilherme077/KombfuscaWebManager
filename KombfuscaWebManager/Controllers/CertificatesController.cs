using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.CertificateModels;
using KombfuscaWebManager.Models.CertificateModels.ViewModels;
using KombfuscaWebManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace KombfuscaWebManager.Controllers;

public class CertificatesController : Controller
{
    private const string DefaultTemplate = "Model1";
    private static readonly HashSet<string> AllowedTemplates = new(StringComparer.OrdinalIgnoreCase) { DefaultTemplate };
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RazorViewRenderer _renderer;
    private readonly CertificateService _pdfService;

    public CertificatesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RazorViewRenderer renderer, CertificateService pdfService)
    {
        _context = context;
        _userManager = userManager;
        _renderer = renderer;
        _pdfService = pdfService;
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Index(int? cupId)
    {
        var query = _context.Certificates.AsNoTracking().Include(c => c.Cup).Include(c => c.User).AsQueryable();
        if (cupId.HasValue) query = query.Where(c => c.CupId == cupId.Value);
        return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create(int cupId, string userId)
    {
        if (await _context.Certificates.AnyAsync(c => c.CupId == cupId && c.UserId == userId))
        {
            TempData["CertificateMessage"] = "Já existe um certificado para este participante nesta copa.";
            return RedirectToAction(nameof(Index), new { cupId });
        }

        var model = await BuildCreateModelAsync(cupId, userId);
        return model is null ? NotFound() : View(model);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCertificateViewModel model)
    {
        var trustedModel = await BuildCreateModelAsync(model.CupId, model.UserId);
        if (trustedModel is null) return NotFound();

        trustedModel.Title = model.Title?.Trim() ?? string.Empty;
        trustedModel.Description1 = model.Description1?.Trim() ?? string.Empty;
        trustedModel.Description2 = model.Description2?.Trim() ?? string.Empty;
        trustedModel.CertificateFileModel = AllowedTemplates.Contains(model.CertificateFileModel) ? model.CertificateFileModel : DefaultTemplate;

        ModelState.Clear();
        if (!TryValidateModel(trustedModel)) return View(trustedModel);

        if (await _context.Certificates.AnyAsync(c => c.CupId == trustedModel.CupId && c.UserId == trustedModel.UserId))
        {
            ModelState.AddModelError(string.Empty, "Já existe um certificado para este participante nesta copa.");
            return View(trustedModel);
        }

        var certificate = new Certificate
        {
            UserId = trustedModel.UserId,
            Title = trustedModel.Title,
            CupId = trustedModel.CupId,
            CupName = trustedModel.CupName,
            Description1 = trustedModel.Description1,
            PlayerName = trustedModel.PlayerName,
            Description2 = trustedModel.Description2,
            StartDate = trustedModel.StartDate,
            EndDate = trustedModel.EndDate,
            CertificateFileModel = trustedModel.CertificateFileModel,
            CreatedAt = DateTime.UtcNow,
            ValidationCode = await GenerateUniqueCodeAsync()
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();
        TempData["CertificateMessage"] = "Certificado emitido com sucesso.";
        return RedirectToAction(nameof(Index), new { cupId = certificate.CupId });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var certificate = await _context.Certificates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (certificate is null) return NotFound();
        if (certificate.UserId != _userManager.GetUserId(User) && !User.IsInRole(Roles.Admin)) return Forbid();
        if (certificate.Status != CertificateStatus.Active) return NotFound();
        if (!AllowedTemplates.Contains(certificate.CertificateFileModel)) return Problem("O modelo deste certificado não está disponível. Consulte o Administrador.");

        var html = await _renderer.RenderAsync($"~/Views/Certificates/Pdf/{certificate.CertificateFileModel}.cshtml", certificate, ControllerContext);
        var pdf = await _pdfService.GenerateAsync(html);
        return File(pdf, "application/pdf", BuildFileName(certificate));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Validate(string? code)
    {
        Certificate? certificate = null;
        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            certificate = await _context.Certificates.AsNoTracking().FirstOrDefaultAsync(c => c.ValidationCode == normalizedCode);
        }
        ViewBag.Searched = !string.IsNullOrWhiteSpace(code);
        return View(certificate);
    }

    private async Task<CreateCertificateViewModel?> BuildCreateModelAsync(int cupId, string userId)
    {
        var result = await _context.CupResults.AsNoTracking().FirstOrDefaultAsync(c => c.CupId == cupId && c.UserId == userId);
        var cup = await _context.Cups.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cupId);
        var user = await _userManager.FindByIdAsync(userId);
        if (result is null || cup is null || user is null) return null;

        return new CreateCertificateViewModel
        {
            CupId = cup.Id, CupName = cup.Name, UserId = user.Id,
            PlayerName = user.FullName ?? user.UserName ?? string.Empty,
            cupResult = result, StartDate = cup.StartDate, EndDate = cup.EndDate ?? cup.StartDate,
            Title = $"{cup.Name} - {result.Position}º lugar", Description1 = "A CAK certifica que",
            Description2 = $"participou da {cup.Name} representando a equipe {result.TeamName} e conquistou o {result.Position}º lugar com {result.TotalScore} pontos no total.", CertificateFileModel = DefaultTemplate
        };
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        string code;
        do
        {
            var bytes = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
            code = $"CAK-{bytes[..8]}-{bytes[8..]}";
        } while (await _context.Certificates.AnyAsync(c => c.ValidationCode == code));
        return code;
    }

    private static string BuildFileName(Certificate certificate)
    {
        var safeName = Regex.Replace($"certificado-{certificate.PlayerName}-{certificate.CupName}", @"[^\p{L}\p{N}._-]+", "-").Trim('-');
        return $"{safeName}.pdf";
    }
}
