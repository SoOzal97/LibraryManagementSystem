using LibraryManagemenytSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagemenytSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Total books
            ViewData["TotalBooks"] = await _context.Books.CountAsync();

            // Total transactions
            ViewData["TotalTransactions"] = await _context.BorrowingTransactions.CountAsync();

            // Total feedback
            ViewData["TotalFeedback"] = await _context.Feedbacks.CountAsync();

            // New arrivals — last 4 added books
            ViewData["NewArrivals"] = await _context.Books
                .OrderByDescending(b => b.DateAdded)
                .Take(4)
                .ToListAsync();

            // Most borrowed books
            var mostBorrowedIds = await _context.BorrowingTransactions
                .GroupBy(t => t.BookId)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();

            var mostBorrowed = await _context.Books
                .Where(b => mostBorrowedIds.Contains(b.BookId))
                .ToListAsync();

            // If no borrowings yet show newest books
            if (!mostBorrowed.Any())
            {
                mostBorrowed = await _context.Books
                    .OrderByDescending(b => b.DateAdded)
                    .Take(4)
                    .ToListAsync();
            }

            ViewData["MostBorrowed"] = mostBorrowed;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}