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
    public class PeriodsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PeriodsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Periods
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Periods.Include(p => p.Copa);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Periods/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var period = await _context.Periods
                .Include(p => p.Copa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (period == null)
            {
                return NotFound();
            }

            return View(period);
        }

        // GET: Periods/Create
        public IActionResult Create()
        {
            ViewData["CopaId"] = new SelectList(_context.Cups, "Id", "Name");
            return View();
        }

        // POST: Periods/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PaperNumber,CopaId")] Period period)
        {
            // Validação manual
            if (period.PaperNumber <= 0)
            {
                ModelState.AddModelError("PaperNumber", "Paper number must be greater than 0");
            }

            if (period.CopaId == 0)
            {
                ModelState.AddModelError("CopaId", "Please select a Cup");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(period);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " + ex.InnerException?.Message);
                }
            }

            ViewData["CopaId"] = new SelectList(_context.Cups, "Id", "Name", period.CopaId);
            return View(period);
        }

        // GET: Periods/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var period = await _context.Periods.FindAsync(id);
            if (period == null)
            {
                return NotFound();
            }
            ViewData["CopaId"] = new SelectList(_context.Cups, "Id", "Name", period.CopaId);
            return View(period);
        }

        // POST: Periods/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PaperNumber,CopaId")] Period period)
        {
            if (id != period.Id)
            {
                return NotFound();
            }

            // Validação manual
            if (period.PaperNumber <= 0)
            {
                ModelState.AddModelError("PaperNumber", "Paper number must be greater than 0");
            }

            if (period.CopaId == 0)
            {
                ModelState.AddModelError("CopaId", "Please select a Cup");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(period);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeriodExists(period.Id))
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

            ViewData["CopaId"] = new SelectList(_context.Cups, "Id", "Name", period.CopaId);
            return View(period);
        }

        // GET: Periods/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var period = await _context.Periods
                .Include(p => p.Copa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (period == null)
            {
                return NotFound();
            }

            return View(period);
        }

        // POST: Periods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var period = await _context.Periods.FindAsync(id);
            if (period != null)
            {
                _context.Periods.Remove(period);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PeriodExists(int id)
        {
            return _context.Periods.Any(e => e.Id == id);
        }
    }
}
