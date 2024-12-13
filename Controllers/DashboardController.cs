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

        public async Task<IActionResult> Index(string reportType = "Weekly")
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


            DateOnly startDate, endDate;
            List<string> xAxisLabels = new();

            switch (reportType)
            {
                case "Weekly":
                    startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                    endDate = DateOnly.FromDateTime(DateTime.Now);
                    xAxisLabels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4" };
                    break;

                case "Monthly":
                    startDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = DateOnly.FromDateTime(DateTime.Now);
                    xAxisLabels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4" };
                    break;

                case "Yearly":
                    startDate = new DateOnly(DateTime.Now.Year, 1, 1);
                    endDate = DateOnly.FromDateTime(DateTime.Now);
                    xAxisLabels = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                    break;

                default: // Summary Report 
                    startDate = DateOnly.MinValue;
                    endDate = DateOnly.FromDateTime(DateTime.Now);
                    break;
            }

            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Category.Type == "Income" && t.Date >= startDate && t.Date <= endDate)
                .SumAsync(t => t.Amount);

            var totalExpense = await _context.Transactions
                .Where(t => t.UserId == userId && t.Category.Type == "Expense" && t.Date >= startDate && t.Date <= endDate)
                .SumAsync(t => t.Amount);

            var totalBalance = totalIncome - totalExpense;

            // Get category-wise expenses for the selected period
            var categoryExpenses = await _context.Transactions
                .Where(t => t.UserId == userId && t.Category.Type == "Expense" && t.Date >= startDate && t.Date <= endDate)
                .GroupBy(t => new { t.Category.Title, t.Category.Icon })
                .Select(g => new CategoryExpenseDto
                {
                    Title = g.Key.Title,
                    Icon = g.Key.Icon,
                    Amount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            // Get income vs expense data for the selected period (for line chart)
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .GroupBy(t => t.Date)
                .Select(g => new TransactionDto
                {
                    Date = g.Key,
                    Income = g.Where(t => t.Category.Type == "Income").Sum(t => t.Amount),
                    Expense = g.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            var dashboardViewModel = new DashboardViewModel
            {
                TotalBalance = totalBalance,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                CategoryExpenses = categoryExpenses,
                Transactions = transactions
            };

            // Set the title for the selected report
            ViewData["Title"] = $"{reportType} Report Dashboard";

            return View(dashboardViewModel);
        }
    }
}
