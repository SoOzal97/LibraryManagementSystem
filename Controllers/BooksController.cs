using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagemenytSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagemenytSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Librarian")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Create([Bind("BookId,Title,Author,Genre,ISBN,IsAvailable,Description,CoverImagePath,DateAdded,TotalCopies")] Book book, IFormFile? coverImage)
        {
            book.DateAdded = DateTime.Now;
            book.AvailableCopies = book.TotalCopies;

            if (coverImage != null && coverImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(stream);
                }

                book.CoverImagePath = "/uploads/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Title,Author,Genre,ISBN,IsAvailable,Description,CoverImagePath,DateAdded,TotalCopies,AvailableCopies")] Book book, IFormFile? coverImage)
        {
            if (id != book.BookId) return NotFound();

            if (coverImage != null && coverImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(stream);
                }
                book.CoverImagePath = "/uploads/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing book to compare copies
                    var existingBook = await _context.Books.AsNoTracking()
                        .FirstOrDefaultAsync(b => b.BookId == id);

                    if (existingBook != null)
                    {
                        // Calculate difference in total copies
                        int copyDifference = book.TotalCopies - existingBook.TotalCopies;

                        // Adjust available copies accordingly
                        book.AvailableCopies = existingBook.AvailableCopies + copyDifference;

                        // Make sure AvailableCopies doesn't go below 0 or above TotalCopies
                        book.AvailableCopies = Math.Max(0, book.AvailableCopies);
                        book.AvailableCopies = Math.Min(book.TotalCopies, book.AvailableCopies);

                        // Update availability status
                        book.IsAvailable = book.AvailableCopies > 0;
                    }

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }

        // GET: Books/Search
        public async Task<IActionResult> Search(string query)
        {
            ViewData["SearchQuery"] = query;

            if (string.IsNullOrEmpty(query))
                return View(new List<Book>());

            var results = await _context.Books
                .Where(b => (b.Title ?? "").ToLower().Contains(query.ToLower()) ||
                            (b.Author ?? "").ToLower().Contains(query.ToLower()) ||
                            (b.Genre ?? "").ToLower().Contains(query.ToLower()))
                .ToListAsync();

            return View(results);
        }

        // GET: Books/Browse
        public async Task<IActionResult> Browse(string genre)
        {
            ViewData["SelectedGenre"] = genre;

            var genres = await _context.Books
                .Select(b => b.Genre ?? "")
                .Distinct()
                .ToListAsync();
            ViewData["Genres"] = genres;

            var books = string.IsNullOrEmpty(genre)
                ? await _context.Books.ToListAsync()
                : await _context.Books
                    .Where(b => (b.Genre ?? "") == genre)
                    .ToListAsync();

            return View(books);
        }
    }
}