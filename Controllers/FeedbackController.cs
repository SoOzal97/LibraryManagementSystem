using LibraryManagemenytSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagemenytSystem.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Feedback/Index — Librarian sees all feedback
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Index()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Book)
                .Include(f => f.User)
                .OrderByDescending(f => f.DateSubmitted)
                .ToListAsync();
            return View(feedbacks);
        }

        // GET: Feedback/MyFeedback — Member sees their feedback
        public async Task<IActionResult> MyFeedback()
        {
            var user = await _userManager.GetUserAsync(User);
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Book)
                .Where(f => f.UserId == user!.Id)
                .OrderByDescending(f => f.DateSubmitted)
                .ToListAsync();
            return View(feedbacks);
        }

        // GET: Feedback/Create/5
        public async Task<IActionResult> Create(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // Check if user already submitted feedback for this book
            var existing = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.UserId == user!.Id && f.BookId == bookId);

            if (existing != null)
            {
                TempData["Error"] = "You have already submitted feedback for this book.";
                return RedirectToAction("MyFeedback");
            }

            ViewData["BookTitle"] = book.Title;
            ViewData["BookId"] = bookId;
            return View();
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            var book = await _context.Books.FindAsync(bookId);

            if (book == null) return NotFound();

            var feedback = new Feedback
            {
                UserId = user!.Id,
                BookId = bookId,
                Rating = rating,
                Comment = comment,
                DateSubmitted = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Thank you for your feedback!";
            return RedirectToAction("MyFeedback");
        }

        // GET: Feedback/BookFeedback/5
        public async Task<IActionResult> BookFeedback(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return NotFound();

            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Where(f => f.BookId == bookId)
                .OrderByDescending(f => f.DateSubmitted)
                .ToListAsync();

            ViewData["BookTitle"] = book.Title;
            ViewData["BookId"] = bookId;
            ViewData["AverageRating"] = feedbacks.Any()
                ? feedbacks.Average(f => f.Rating).ToString("F1")
                : "No ratings yet";

            return View(feedbacks);
        }

        // POST: Feedback/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Delete(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Feedback deleted successfully.";
            }
            return RedirectToAction("Index");
        }
    }
}