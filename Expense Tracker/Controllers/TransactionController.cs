using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    public class TransactionController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Transaction/Create
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                PopulateCategories();
                return View(new Transaction());
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            PopulateCategories();
            return View(transaction);
        }

        // POST: Transaction/AddOrEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategories();
                return View(transaction);
            }

            if (transaction.TransactionId == 0)
            {
                _context.Add(transaction);
            }
            else
            {
                if (!TransactionExists(transaction.TransactionId))
                {
                    return NotFound();
                }

                _context.Update(transaction);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                // Puedes registrar el error o manejarlo de otra forma
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar la transacción. Por favor, inténtalo de nuevo.");
            }

            PopulateCategories();
            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }

        [NonAction]
        public void PopulateCategories()
        {
            var categoryCollection = _context.Categories.Select(c => new
            {
                c.CategoryId,
                TitleWithIcon = c.TitleWithIcon ?? $"{c.Icon} {c.Title}"
            }).ToList();
            
            categoryCollection.Insert(0, new { CategoryId = 0, TitleWithIcon = "Choose a Category" });
            ViewBag.Categories = categoryCollection;
        }
    }
}
