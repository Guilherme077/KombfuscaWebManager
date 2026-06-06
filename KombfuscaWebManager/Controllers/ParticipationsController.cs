using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models.CupModels;

namespace KombfuscaWebManager.Controllers
{
    public class ParticipationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParticipationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Participations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Participations.Include(p => p.Cup).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Participations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations
                .Include(p => p.Cup)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participation == null)
            {
                return NotFound();
            }

            return View(participation);
        }

        // GET: Participations/Create
        public IActionResult Create()
        {
            ViewData["CupId"] = new SelectList(_context.Cups, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Participations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CupId,UserId,TeamName")] Participation participation)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(participation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " + ex.InnerException?.Message);
                }
            }

            ViewData["CupId"] = new SelectList(_context.Cups, "Id", "Name", participation.CupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", participation.UserId);
            return View(participation);
        }

        // GET: Participations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations.FindAsync(id);
            if (participation == null)
            {
                return NotFound();
            }
            ViewData["CupId"] = new SelectList(_context.Cups, "Id", "Name", participation.CupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", participation.UserId);
            return View(participation);
        }

        // POST: Participations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CupId,UserId,TeamName")] Participation participation)
        {
            if (id != participation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(participation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParticipationExists(participation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " + ex.InnerException?.Message);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CupId"] = new SelectList(_context.Cups, "Id", "Name", participation.CupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", participation.UserId);
            return View(participation);
        }

        // GET: Participations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations
                .Include(p => p.Cup)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participation == null)
            {
                return NotFound();
            }

            return View(participation);
        }

        // POST: Participations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participation = await _context.Participations.FindAsync(id);
            if (participation != null)
            {
                _context.Participations.Remove(participation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParticipationExists(int id)
        {
            return _context.Participations.Any(e => e.Id == id);
        }
    }
}
