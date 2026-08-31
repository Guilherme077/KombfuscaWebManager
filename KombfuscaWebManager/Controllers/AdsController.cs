using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.AdModels;
using KombfuscaWebManager.Models.AdModels.ViewModels;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KombfuscaWebManager.Controllers
{
    public class AdsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AdsService _adsService;
        public AdsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, AdsService adsService)
        {
            _context = context;
            _userManager = userManager;
            _adsService = adsService;
        }

        public IActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                if(User.IsInRole(Roles.Admin))
                {
                    return RedirectToAction("ManageAds");
                }
                return RedirectToAction("AdsCentral");
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AdsCentral()
        {
            AdPeriodStatus status = AdPeriodStatus.NoCupOrPeriodFound;
            AdSubscriptionPeriod? adPeriod = null;
            var cup = await _context.Cups.Where(c => c.StartDate >= DateTime.Now).OrderBy(c => c.StartDate).FirstOrDefaultAsync();

            if(cup != null)
            {
                adPeriod = await _context.AdSubscriptionPeriods
                .Include(p => p.Cup)
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.CupId == cup.Id);

                if (adPeriod != null)
                {
                    CheckAdPeriodStatus(adPeriod, out status);
                }
            }

            var viewModel = new AdCentralViewModel
            {
                AdPeriodStatus = status,
                CurrentAdPeriod = adPeriod
            };

            if (adPeriod != null)
            {
                var userId = _userManager.GetUserId(User);
                if(status == AdPeriodStatus.AuctionOpen || status == AdPeriodStatus.AuctionEnded)
                {
                    viewModel.UserAdRequest = await _context.AdRequests
                        .Where(request => (request.UserId == userId && request.SubscriptionPeriod!.Id == adPeriod.Id) && request.Status == RequestStatus.Approved)
                        .ToListAsync();
                }
                else
                {
                    viewModel.UserAdRequest = await _context.AdRequests
                        .Where(request => request.UserId == userId && request.SubscriptionPeriod!.Id == adPeriod.Id)
                        .ToListAsync();
                }
                

                if (status == AdPeriodStatus.AuctionOpen)
                {
                    var validAuctionBids = await _context.AuctionBids
                        .AsNoTracking()
                        .Include(bid => bid.Request)
                        .Where(bid => bid.Valid && bid.Request.SubscriptionPeriod!.Id == adPeriod.Id)
                        .OrderByDescending(bid => bid.Value)
                        .ThenBy(bid => bid.Id)
                        .ToListAsync();

                    foreach (var bid in validAuctionBids)
                    {
                        if(bid.Request.UserId != userId)
                        {
                            bid.Request.OficialName = "Anonimo";
                            bid.Request.BrandName = "Anonimo";
                            bid.Request.Slogan = "Anonimo";
                            bid.Request.StatusMessage = "Anonimo";
                            bid.Request.UserId = "";
                            bid.Request.User = null;
                        }
                    }

                    viewModel.AllValidAuctionBids = validAuctionBids;

                    viewModel.UserValidAuctionBids = viewModel.AllValidAuctionBids
                        .Where(bid => bid.Request.UserId == userId)
                        .ToList();
                    viewModel.BidCategories = _adsService.RankBidsByCategory(
                        viewModel.AllValidAuctionBids,
                        adPeriod.Categories);
                }

                if (status == AdPeriodStatus.AuctionEnded)
                {
                    var validAuctionBids = await _context.AuctionBids
                        .AsNoTracking()
                        .Include(bid => bid.Request)
                        .Where(bid => bid.Valid && bid.Request.SubscriptionPeriod!.Id == adPeriod.Id)
                        .OrderByDescending(bid => bid.Value)
                        .ThenBy(bid => bid.Id)
                        .ToListAsync();

                    viewModel.AllValidAuctionBids = validAuctionBids;

                    viewModel.UserValidAuctionBids = viewModel.AllValidAuctionBids
                        .Where(bid => bid.Request.UserId == userId)
                        .ToList();
                    viewModel.BidCategories = _adsService.RankBidsByCategory(
                        viewModel.AllValidAuctionBids,
                        adPeriod.Categories);
                }
            }

            return View(viewModel);
        }

        private void CheckAdPeriodStatus(AdSubscriptionPeriod adPeriod, out AdPeriodStatus status)
        {
            var now = DateTime.Now.Date;
            if (now < adPeriod.StartSubscription)
            {
                status = AdPeriodStatus.WaitingSubscription;
            }
            else if (now >= adPeriod.StartSubscription && now <= adPeriod.EndSubscription)
            {
                status = AdPeriodStatus.SubscriptionOpen;
            }
            else if (now > adPeriod.EndSubscription && now < adPeriod.SituationReviewDate)
            {
                status = AdPeriodStatus.WaitingResults;
            }
            else if (now >= adPeriod.SituationReviewDate && now < adPeriod.StartAuction)
            {
                status = AdPeriodStatus.ResultsAvailable;
            }
            else if (now >= adPeriod.StartAuction && now <= adPeriod.EndAuction)
            {
                status = AdPeriodStatus.AuctionOpen;
            }
            else
            {
                status = AdPeriodStatus.AuctionEnded;
            }
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
            var hasInvalidDate = false;

            if (adPeriod.EndSubscription <= adPeriod.StartSubscription)
            {
                ModelState.AddModelError(string.Empty,
                    "A data final das inscrições deve ser posterior à data inicial.");
                hasInvalidDate = true;
            }

            if (adPeriod.SituationReviewDate <= adPeriod.EndSubscription)
            {
                ModelState.AddModelError(string.Empty,
                    "A data de liberação dos resultados de inscrições deve ser posterior ao final das inscrições.");
                hasInvalidDate = true;
            }

            if (adPeriod.StartAuction <= adPeriod.SituationReviewDate)
            {
                ModelState.AddModelError(string.Empty,
                    "O período de leilão deve ser posterior à data de liberação de resultados.");
                hasInvalidDate = true;
            }

            if (adPeriod.EndAuction <= adPeriod.StartAuction)
            {
                ModelState.AddModelError(string.Empty,
                    "A data final do leilão deve ser posterior à data inicial.");
                hasInvalidDate = true;
            }

            var cup = await _context.Cups.Where(c => c.StartDate >= DateTime.Now).OrderBy(c => c.StartDate).FirstOrDefaultAsync();
            if (cup == null) return BadRequest("No upcoming cups found.");
            var cupId = cup.Id;

            var existingPeriodsConflict = await _context.AdSubscriptionPeriods.Where(p => p.CupId == cupId).ToListAsync();

            if(existingPeriodsConflict.Count > 0)
            {
                ModelState.AddModelError(string.Empty, "A copa mais próxima já possui um período de anúncios configurado.");
                hasInvalidDate = true;
            }

            if (hasInvalidDate)
            {
                var adPeriods = await _context.AdSubscriptionPeriods
                    .Include(p => p.Cup)
                    .Include(p => p.Categories)
                    .ToListAsync();

                ViewData["StartSubscription"] = adPeriod.StartSubscription.ToString("yyyy-MM-dd");
                ViewData["EndSubscription"] = adPeriod.EndSubscription.ToString("yyyy-MM-dd");
                ViewData["SituationReviewDate"] = adPeriod.SituationReviewDate.ToString("yyyy-MM-dd");
                ViewData["StartAuction"] = adPeriod.StartAuction.ToString("yyyy-MM-dd");
                ViewData["EndAuction"] = adPeriod.EndAuction.ToString("yyyy-MM-dd");

                return View(nameof(ManageAds), adPeriods);
            }

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
        public async Task<IActionResult> EditAdPeriod(int id)
        {
            var adPeriod = await _context.AdSubscriptionPeriods
                .Include(p => p.Cup)
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (adPeriod == null) return NotFound();

            return View(adPeriod);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdPeriod(int id, AdSubscriptionPeriod updatedPeriod)
        {
            var adPeriod = await _context.AdSubscriptionPeriods
                .Include(p => p.Cup)
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (adPeriod == null) return NotFound();

            var hasInvalidDate = false;

            if (updatedPeriod.EndSubscription <= updatedPeriod.StartSubscription)
            {
                ModelState.AddModelError(string.Empty,
                    "A data final das inscrições deve ser posterior à data inicial.");
                hasInvalidDate = true;
            }

            if (updatedPeriod.SituationReviewDate <= updatedPeriod.EndSubscription)
            {
                ModelState.AddModelError(string.Empty,
                    "A data de liberação dos resultados de inscrições deve ser posterior ao final das inscrições.");
                hasInvalidDate = true;
            }

            if (updatedPeriod.StartAuction <= updatedPeriod.SituationReviewDate)
            {
                ModelState.AddModelError(string.Empty,
                    "O período de leilão deve ser posterior à data de liberação de resultados.");
                hasInvalidDate = true;
            }

            if (updatedPeriod.EndAuction <= updatedPeriod.StartAuction)
            {
                ModelState.AddModelError(string.Empty,
                    "A data final do leilão deve ser posterior à data inicial.");
                hasInvalidDate = true;
            }

            if (hasInvalidDate)
            {
                updatedPeriod.Id = adPeriod.Id;
                updatedPeriod.Cup = adPeriod.Cup;
                return View(updatedPeriod);
            }

            adPeriod.StartSubscription = updatedPeriod.StartSubscription;
            adPeriod.EndSubscription = updatedPeriod.EndSubscription;
            adPeriod.SituationReviewDate = updatedPeriod.SituationReviewDate;
            adPeriod.StartAuction = updatedPeriod.StartAuction;
            adPeriod.EndAuction = updatedPeriod.EndAuction;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageAds));
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPeriodCategory(int periodId, int categoryId, AdCategory updatedCategory)
        {
            var adPeriod = await _context.AdSubscriptionPeriods
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == periodId);

            if (adPeriod == null) return NotFound();

            var category = adPeriod.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return NotFound();

            category.Name = updatedCategory.Name;
            category.Description = updatedCategory.Description;
            category.MaxAds = updatedCategory.MaxAds;
            category.MinValue = updatedCategory.MinValue;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EditAdPeriod), new { id = periodId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdRequest(AdRequest request)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            var cup = await _context.Cups.Where(c => c.StartDate >= DateTime.Now).OrderBy(c => c.StartDate).FirstOrDefaultAsync();

            if(cup == null) return BadRequest("Houve um erro ao tentar registrar sua inscrição: Copa não encontrada.");

            var adPeriod = await _context.AdSubscriptionPeriods
                .Include(p => p.Cup)
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.CupId == cup.Id);

            if (adPeriod == null) return BadRequest("Houve um erro ao tentar registrar sua inscrição: Nenhum período de anúncios encontrado.");

            AdPeriodStatus status = AdPeriodStatus.NoCupOrPeriodFound;
            CheckAdPeriodStatus(adPeriod, out status);

            if (status != AdPeriodStatus.SubscriptionOpen) return BadRequest("Houve um erro ao tentar registrar sua inscrição: Período de inscrição não está aberto.");

            var newRequest = new AdRequest
            {
                UserId = userId,
                RequestedAt = DateTime.Now,
                SubscriptionPeriod = adPeriod,
                BrandName = request.BrandName,
                OficialName = request.OficialName,
                Slogan = request.Slogan,
                Status = RequestStatus.Pending,
                StatusMessage = String.Empty
            };
            _context.AdRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction("AdsCentral");

        }

    }

    public enum AdPeriodStatus
    {
        NoCupOrPeriodFound,
        WaitingSubscription,
        SubscriptionOpen,
        WaitingResults,
        ResultsAvailable,
        AuctionOpen,
        AuctionEnded
    }
}
