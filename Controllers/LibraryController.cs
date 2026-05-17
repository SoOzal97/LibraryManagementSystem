using LibraryManagemenytSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagemenytSystem.Controllers
{
    public class LibraryController : Controller
    {
        private readonly AppDbContext _context;

        public LibraryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Library
        public async Task<IActionResult> Index()
        {
            return View(await _context.Libraries.ToListAsync());
        }

        // GET: Library/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var library = await _context.Libraries.FirstOrDefaultAsync(m => m.LibraryId == id);
            if (library == null) return NotFound();
            return View(library);
        }

        // GET: Library/Create
        [Authorize(Roles = "Librarian")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Library/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Create([Bind("LibraryId,Name,Location,OperatingHours,ContactDetails,Email,PhoneNumber,Website,DateEstablished")] Library library)
        {
            if (ModelState.IsValid)
            {
                _context.Add(library);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(library);
        }

        // GET: Library/Edit/5
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var library = await _context.Libraries.FindAsync(id);
            if (library == null) return NotFound();
            return View(library);
        }

        // POST: Library/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int id, [Bind("LibraryId,Name,Location,OperatingHours,ContactDetails,Email,PhoneNumber,Website,DateEstablished")] Library library)
        {
            if (id != library.LibraryId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(library);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Libraries.Any(e => e.LibraryId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(library);
        }

        // GET: Library/Delete/5
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var library = await _context.Libraries.FirstOrDefaultAsync(m => m.LibraryId == id);
            if (library == null) return NotFound();
            return View(library);
        }

        // POST: Library/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var library = await _context.Libraries.FindAsync(id);
            if (library != null) _context.Libraries.Remove(library);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}