using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using Microsoft.AspNetCore.Identity;
using KombfuscaWebManager.Models;
using Microsoft.AspNetCore.Authorization;

namespace KombfuscaWebManager.Controllers
{
    public class CupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cups
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cups.ToListAsync());
        }

        // GET: Cups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cup = await _context.Cups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cup == null)
            {
                return NotFound();
            }

            return View(cup);
        }

        // GET: Cups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,StartDate,Year,Placename,EndDate,SubscriptionFee,cupStatus")] Cup cup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cup);
        }

        // GET: Cups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cup = await _context.Cups.FindAsync(id);
            if (cup == null)
            {
                return NotFound();
            }
            return View(cup);
        }

        // POST: Cups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,StartDate,Year,Placename,EndDate,SubscriptionFee,cupStatus")] Cup cup)
        {
            if (id != cup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CupExists(cup.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cup);
        }

        // GET: Cups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cup = await _context.Cups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cup == null)
            {
                return NotFound();
            }

            return View(cup);
        }

        // POST: Cups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cup = await _context.Cups.FindAsync(id);
            if (cup != null)
            {
                _context.Cups.Remove(cup);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CupExists(int id)
        {
            return _context.Cups.Any(e => e.Id == id);
        }

        // GET: Cups/ManageAssignments/5
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ManageAssignments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cup = await _context.Cups.FindAsync(id);
            if (cup == null)
            {
                return NotFound();
            }

            var assignedUserIds = await _context.CupAssignments
                .Where(ca => ca.CupId == id)
                .Select(ca => ca.UserId)
                .ToListAsync();

            var allUsers = await _userManager.Users.ToListAsync();

            var userAssignments = allUsers
                .Select(u => new UserAssignmentViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    UserEmail = u.Email ?? string.Empty,
                    IsAssigned = assignedUserIds.Contains(u.Id)
                })
                .OrderBy(u => u.UserName)
                .ToList();

            var model = new CupAssignmentViewModel
            {
                CupId = cup.Id,
                CupName = cup.Name,
                UserAssignments = userAssignments
            };

            return View(model);
        }

        // POST: Cups/ManageAssignments/5
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageAssignments(int id, CupAssignmentViewModel model)
        {
            if (id != model.CupId)
            {
                return NotFound();
            }

            var cup = await _context.Cups.FindAsync(id);
            if (cup == null)
            {
                return NotFound();
            }

            // Get current assignments
            var currentAssignments = await _context.CupAssignments
                .Where(ca => ca.CupId == id)
                .ToListAsync();

            // Remove all current assignments
            _context.CupAssignments.RemoveRange(currentAssignments);

            // Add new assignments based on form selection
            foreach (var userAssignment in model.UserAssignments.Where(ua => ua.IsAssigned))
            {
                var assignment = new CupAssignment
                {
                    CupId = id,
                    UserId = userAssignment.UserId,
                    AssignedAt = DateTime.UtcNow
                };
                _context.CupAssignments.Add(assignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
