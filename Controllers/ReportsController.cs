using LibraryManagemenytSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagemenytSystem.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Reports/Index
        public async Task<IActionResult> Index()
        {
            // Total books
            var totalBooks = await _context.Books.CountAsync();

            // Available books
            var availableBooks = await _context.Books
                .CountAsync(b => b.IsAvailable);

            // Total members
            var totalTransactions = await _context.BorrowingTransactions.CountAsync();

            // Overdue books
            var overdueBooks = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .Where(t => t.Status != "Returned" && t.DueDate < DateTime.Now)
                .ToListAsync();

            // Most popular books
            var popularBooks = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .GroupBy(t => t.Book!.Title)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            // Most active members
            var activeMembers = await _context.BorrowingTransactions
                .Include(t => t.User)
                .GroupBy(t => t.User!.FullName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            // Borrowing trends last 7 days
            var last7Days = await _context.BorrowingTransactions
                .Where(t => t.BorrowDate >= DateTime.Now.AddDays(-7))
                .GroupBy(t => t.BorrowDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            // Total fines collected
            var totalFines = await _context.BorrowingTransactions
                .SumAsync(t => t.FineAmount);

            // Total feedback
            var totalFeedback = await _context.Feedbacks.CountAsync();

            ViewData["TotalBooks"] = totalBooks;
            ViewData["AvailableBooks"] = availableBooks;
            ViewData["TotalTransactions"] = totalTransactions;
            ViewData["TotalFines"] = totalFines.ToString("F2");
            ViewData["TotalFeedback"] = totalFeedback;
            ViewData["OverdueBooks"] = overdueBooks;
            ViewData["PopularBooks"] = popularBooks;
            ViewData["ActiveMembers"] = activeMembers;
            ViewData["Last7Days"] = last7Days;

            return View();
        }
    }
}