using LibraryManagemenytSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagemenytSystem.Controllers
{
    [Authorize]
    public class BorrowingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BorrowingController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Borrowing/Index — Admin view all transactions
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();
            return View(transactions);
        }

        // GET: Borrowing/MyBorrowings — Member view their borrowings
        public async Task<IActionResult> MyBorrowings()
        {
            var user = await _userManager.GetUserAsync(User);
            var transactions = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Where(t => t.UserId == user!.Id)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();
            return View(transactions);
        }

        // POST: Borrowing/Borrow/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            var book = await _context.Books.FindAsync(bookId);
            var config = await _context.BorrowingConfigs.FirstOrDefaultAsync()
                ?? new BorrowingConfig();

            if (book == null) return NotFound();

            // Check if available copies exist
            if (book.AvailableCopies <= 0)
            {
                TempData["Error"] = "Sorry, no copies of this book are currently available.";
                return RedirectToAction("Browse", "Books");
            }

            // Check max borrowable items
            var activeBorrowings = await _context.BorrowingTransactions
                .Where(t => t.UserId == user!.Id && t.Status != "Returned")
                .CountAsync();

            if (activeBorrowings >= config.MaxBorrowableItems)
            {
                TempData["Error"] = $"You have reached the maximum limit of {config.MaxBorrowableItems} borrowed books.";
                return RedirectToAction("Browse", "Books");
            }

            // Create transaction
            var transaction = new BorrowingTransaction
            {
                UserId = user!.Id,
                BookId = bookId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(config.LoanDurationDays),
                Status = "Borrowed"
            };

            // Decrease available copies
            book.AvailableCopies--;

            // Mark unavailable only when all copies are borrowed
            if (book.AvailableCopies == 0)
                book.IsAvailable = false;

            _context.BorrowingTransactions.Add(transaction);
            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"You have successfully borrowed '{book.Title}'. Due date: {transaction.DueDate:dd/MM/yyyy}. Copies remaining: {book.AvailableCopies}/{book.TotalCopies}";
            return RedirectToAction("MyBorrowings");
        }

        // POST: Borrowing/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int transactionId)
        {
            var transaction = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null) return NotFound();

            var config = await _context.BorrowingConfigs.FirstOrDefaultAsync()
                ?? new BorrowingConfig();

            // Calculate fine if overdue
            if (DateTime.Now > transaction.DueDate)
            {
                var daysOverdue = (DateTime.Now - transaction.DueDate).Days;
                transaction.FineAmount = daysOverdue * config.OverduePenaltyPerDay;
            }

            // Update transaction
            transaction.ReturnDate = DateTime.Now;
            transaction.Status = "Returned";

            // Increase available copies
            if (transaction.Book!.AvailableCopies < transaction.Book.TotalCopies)
                transaction.Book.AvailableCopies++;

            // Mark available again if at least 1 copy is available
            if (transaction.Book.AvailableCopies > 0)
                transaction.Book.IsAvailable = true;

            _context.BorrowingTransactions.Update(transaction);
            _context.Books.Update(transaction.Book);
            await _context.SaveChangesAsync();

            TempData["Message"] = transaction.FineAmount > 0
                ? $"Book returned. Fine: ${transaction.FineAmount:F2}"
                : "Book returned successfully!";

            return RedirectToAction("MyBorrowings");
        }

        // POST: Borrowing/Renew/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int transactionId)
        {
            var transaction = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null) return NotFound();

            var config = await _context.BorrowingConfigs.FirstOrDefaultAsync()
                ?? new BorrowingConfig();

            // Check renewal limit
            if (transaction.RenewedCount >= config.RenewalLimit)
            {
                TempData["Error"] = $"You have reached the maximum renewal limit of {config.RenewalLimit}.";
                return RedirectToAction("MyBorrowings");
            }

            // Extend due date
            transaction.DueDate = transaction.DueDate.AddDays(config.LoanDurationDays);
            transaction.RenewedCount++;
            transaction.IsRenewed = true;
            transaction.Status = "Renewed";

            _context.BorrowingTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Loan renewed successfully! New due date: {transaction.DueDate:dd/MM/yyyy}";
            return RedirectToAction("MyBorrowings");
        }

        // GET: Borrowing/History
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            var history = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Where(t => t.UserId == user!.Id && t.Status == "Returned")
                .OrderByDescending(t => t.ReturnDate)
                .ToListAsync();
            return View(history);
        }

        // GET: Borrowing/Config — Librarian only
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Config()
        {
            var config = await _context.BorrowingConfigs.FirstOrDefaultAsync()
                ?? new BorrowingConfig();
            return View(config);
        }

        // POST: Borrowing/Config
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Config(BorrowingConfig config)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.BorrowingConfigs.FirstOrDefaultAsync();
                if (existing == null)
                {
                    _context.BorrowingConfigs.Add(config);
                }
                else
                {
                    existing.LoanDurationDays = config.LoanDurationDays;
                    existing.RenewalLimit = config.RenewalLimit;
                    existing.OverduePenaltyPerDay = config.OverduePenaltyPerDay;
                    existing.MaxBorrowableItems = config.MaxBorrowableItems;
                    _context.BorrowingConfigs.Update(existing);
                }
                await _context.SaveChangesAsync();
                TempData["Message"] = "Borrowing configuration updated successfully!";
                return RedirectToAction("Config");
            }
            return View(config);
        }
    }
}