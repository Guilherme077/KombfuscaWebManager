using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.AdModels;
using KombfuscaWebManager.Models.AdModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KombfuscaWebManager.Controllers
{
    public class AdsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("AdsCentral");
        }

        [Authorize]
        [HttpGet]
        public IActionResult AdsCentral()
        {
            return View();
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public async Task<IActionResult> ManageAds()
        {
            var adPeriods = await _context.AdSubscriptionPeriods.Include(p => p.Cup).Include(p => p.Categories).ToListAsync();
            return View(adPeriods);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateAdPeriod(AdSubscriptionPeriod adPeriod)
        {
            var cup = await _context.Cups.Where(c => c.StartDate >= DateTime.Now).OrderBy(c => c.StartDate).FirstOrDefaultAsync();
            if (cup == null) return BadRequest("No upcoming cups found.");
            var cupId = cup.Id;

            var newPeriod = new AdSubscriptionPeriod
            {
                CupId = cupId,
                Cup = cup,
                StartSubscription = adPeriod.StartSubscription,
                EndSubscription = adPeriod.EndSubscription,
                StartAuction = adPeriod.StartAuction,
                EndAuction = adPeriod.EndAuction,
                SituationReviewDate = adPeriod.SituationReviewDate
            };

            _context.AdSubscriptionPeriods.Add(newPeriod);
            _context.SaveChanges();

            return RedirectToAction("ManageAds");
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreatePeriodCategory(AdCategoryViewModel adCategory)
        {
            var adPeriod = await _context.AdSubscriptionPeriods.Where(p => p.Id == adCategory.AdPeriodId).FirstOrDefaultAsync();
            if (adPeriod == null) return BadRequest("Ad period selected not found.");
            adPeriod.Categories.Add(new AdCategory
            {
                Name = adCategory.Name,
                Description = adCategory.Description,
                MaxAds = adCategory.MaxAds,
                MinValue = adCategory.MinValue
            });
            
            _context.AdSubscriptionPeriods.Update(adPeriod);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageAds");
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public IActionResult EditAdPeriod(int id)
        {
            return View();
        }
    }
}
