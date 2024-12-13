using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transaction/Index
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 8, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {

                ViewData["Username"] = user.Username;
                ViewData["UserImagePath"] = user.ImagePath;
            }

            ViewData["Username"] = username;

            var userId = await _context.Users
                                        .Where(u => u.Username == username)
                                        .Select(u => u.UserId)
                                        .FirstOrDefaultAsync();

         
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Transactions.AsQueryable();

            // Filter by date range if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value && t.Date <= endDate.Value);
            }

            query = query.Where(t => t.UserId == userId);

            var totalItems = await query.CountAsync();

            if (totalItems > pageSize)
            {
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                ViewBag.CurrentPage = pageNumber;
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

                // Apply pagination
                query = query.OrderBy(t => t.TransactionId)
                             .Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize);
            }
            else
            {
                // If there are fewer items than the page size, don't apply pagination
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = 1;
                ViewBag.CurrentPage = 1;
            }

            var transactions = await query
                .Include(t => t.Category)  // Include related category
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return View(transactions);
        }

        //Get Transactions with TransactionId
        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            // Return a partial view with transaction details
            return PartialView("_TransactionDetails", transaction);
        }

        //Download Excel File
        public async Task<IActionResult> DownloadExcel(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            // Retrieve username from session
            var username = HttpContext.Session.GetString("Username");

            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get UserId from the database
            var userId = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            if (userId == 0)
            {
                return NotFound();
            }

            var query = _context.Transactions.AsQueryable();

            // Filter by userId
            query = query.Where(t => t.UserId == userId);

            // Filter by date range if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value && t.Date <= endDate.Value);
            }

            var transactions = await query.Include(t => t.Category).ToListAsync();

            // Calculate totals
            var totalIncome = transactions.Where(t => t.Category.Type == "Income").Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount);
            var totalBalance = totalIncome - totalExpense;

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Generate Excel
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Transactions");

            // Set Headers
            worksheet.Cells[1, 1].Value = "Category";
            worksheet.Cells[1, 2].Value = "Amount";
            worksheet.Cells[1, 3].Value = "Date";
            worksheet.Cells[1, 4].Value = "Payment Method";
            worksheet.Cells[1, 5].Value = "Note";
            worksheet.Cells[1, 6].Value = "Type";

            // Style Headers
            using (var range = worksheet.Cells[1, 1, 1, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.Blue);
            }

            // Fill Data
            int row = 2;
            foreach (var transaction in transactions)
            {
                worksheet.Cells[row, 1].Value = transaction.Category.TitleWithIcon;
                worksheet.Cells[row, 2].Value = transaction.Amount;
                worksheet.Cells[row, 3].Value = transaction.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = transaction.PaymentMethod;
                worksheet.Cells[row, 5].Value = transaction.Note;
                worksheet.Cells[row, 6].Value = transaction.Category.Type;
                row++;
            }

            worksheet.Cells[row + 1, 1].Value = "Total Income";
            worksheet.Cells[row + 1, 2].Value = totalIncome;

            worksheet.Cells[row + 2, 1].Value = "Total Expense";
            worksheet.Cells[row + 2, 2].Value = totalExpense;

            worksheet.Cells[row + 3, 1].Value = "Total Balance";
            worksheet.Cells[row + 3, 2].Value = totalBalance;

            worksheet.Cells.AutoFitColumns();

            //string fileName = startDate.HasValue && endDate.HasValue ? $"Transactions_{startDate.Value.ToString("yyyy-MM-dd")}_to_{endDate.Value.ToString("yyyy-MM-dd")}.xlsx" : "Transactions.xlsx";

            string fileName;
            if (startDate.HasValue && endDate.HasValue)
            {
                var formattedStartDate = startDate.Value.ToString("yyyy-MM-dd");
                var formattedEndDate = endDate.Value.ToString("yyyy-MM-dd");
                fileName = $"Transactions_{formattedStartDate}_to_{formattedEndDate}.xlsx";
            }
            else
            {
                fileName = "Transactions.xlsx";
            }

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(package.GetAsByteArray(), contentType, fileName);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {

                ViewData["Username"] = user.Username;
                ViewData["UserImagePath"] = user.ImagePath;
            }
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "TitleWithIcon");
            return View();
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Amount, Date, ImagePath, PaymentMethod, Note, CategoryId")] Transaction transaction, IFormFile? ImageFile)
        {
            
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = await _context.Users
                                       .Where(u => u.Username == HttpContext.Session.GetString("Username"))
                                       .Select(u => u.UserId)
                                       .FirstOrDefaultAsync();

            transaction.UserId = userId;  // Set the UserId before saving

            if (ModelState.IsValid)
            {
                if (transaction.CategoryId == null || !_context.Categories.Any(c => c.CategoryId == transaction.CategoryId))
                {
                    ModelState.AddModelError("CategoryId", "The Category field is required.");
                }
                else
                {
                    if (ImageFile != null)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", ImageFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }
                        transaction.ImagePath = "/images/" + ImageFile.FileName;
                    }

                    _context.Add(transaction);
                    await _context.SaveChangesAsync();

                    TempData["CreateTransaction"] = "Transaction Created Successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "TitleWithIcon", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {

                ViewData["Username"] = user.Username;
                ViewData["UserImagePath"] = user.ImagePath;
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TransactionId == id);

            if (transaction == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "TitleWithIcon", transaction.CategoryId);
            return View(transaction);
        }


        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,CategoryId,Amount,Date,ImagePath,PaymentMethod,Note")] Transaction transaction, IFormFile? ImageFile)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = await _context.Users
                                       .Where(u => u.Username == HttpContext.Session.GetString("Username"))
                                       .Select(u => u.UserId)
                                       .FirstOrDefaultAsync();

            transaction.UserId = userId;
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null)
                    {
                        // If a new image is uploaded, delete the old image if it exists
                        if (!string.IsNullOrEmpty(transaction.ImagePath))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", transaction.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save the new image
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", ImageFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }
                        transaction.ImagePath = "/images/" + ImageFile.FileName;
                    }

                    // Update the transaction (this will save the ImagePath if it's unchanged or updated)
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["UpdateTransaction"] = "Transaction Updated Successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "TitleWithIcon", transaction.CategoryId);
            return View(transaction);
        }

        // Delete Transaction from checkbox
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(List<int> selectedTransactions)
        {
            // Ensure that selectedTransactions is not null
            if (selectedTransactions == null || selectedTransactions.Count == 0)
            {
                return RedirectToAction("Index");
            }

            // Fetch the transactions that are selected for deletion
            var transactionsToDelete = await _context.Transactions
                .Where(t => selectedTransactions.Contains(t.TransactionId))
                .ToListAsync();

            if (transactionsToDelete.Count > 0)
            {
                _context.Transactions.RemoveRange(transactionsToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {

                ViewData["Username"] = user.Username;
                ViewData["UserImagePath"] = user.ImagePath;
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = await _context.Users
                                       .Where(u => u.Username == HttpContext.Session.GetString("Username"))
                                       .Select(u => u.UserId)
                                       .FirstOrDefaultAsync();

           
            var transaction = await _context.Transactions.FindAsync(id);
            transaction.UserId = userId;
            if (transaction != null)
            {

                if (!string.IsNullOrEmpty(transaction.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", transaction.ImagePath.TrimStart('/'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();

            TempData["DeleteTransaction"] = "Transaction Deleted Successfully.";
            return RedirectToAction(nameof(Index));
        }


        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }


    }
}
