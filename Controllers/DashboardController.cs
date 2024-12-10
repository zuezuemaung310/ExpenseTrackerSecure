using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                ViewData["Username"] = user.Username;
                ViewData["UserImagePath"] = user.ImagePath; 
            }

            var userId = await _context.Users
                                       .Where(u => u.Username == username)
                                       .Select(u => u.UserId)
                                       .FirstOrDefaultAsync();

          if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Calculate totals for the current user
            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Category.Type == "Income")
                .SumAsync(t => t.Amount);

            var totalExpense = await _context.Transactions
                .Where(t => t.UserId == userId && t.Category.Type == "Expense")
                .SumAsync(t => t.Amount);

            var totalBalance = totalIncome - totalExpense;

            // Fetch category-wise expenses for the current user
            var categoryExpenses = await _context.Transactions
                .Where(t => t.UserId == userId && t.Category.Type == "Expense")
                .GroupBy(t => new { t.Category.Title, t.Category.Icon })
                .Select(g => new CategoryExpenseDto
                {
                    Title = g.Key.Title,
                    Icon = g.Key.Icon,
                    Amount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            // Fetch transactions for spline chart (income and expense over time)
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.Date)
                .Select(g => new TransactionDto
                {
                    Date = g.Key,
                    Income = g.Where(t => t.Category.Type == "Income").Sum(t => t.Amount),
                    Expense = g.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            // Create the dashboard view model
            var dashboardViewModel = new DashboardViewModel
            {
                TotalBalance = totalBalance,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                CategoryExpenses = categoryExpenses,
                Transactions = transactions
            };

            return View(dashboardViewModel);
        }

    }
}
