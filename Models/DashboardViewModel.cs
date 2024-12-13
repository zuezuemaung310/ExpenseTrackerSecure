using ExpenseTracker.Dto;

namespace ExpenseTracker.Models
{
    public class DashboardViewModel
    {
        public int TotalBalance { get; set; }
        public int TotalIncome { get; set; }
        public int TotalExpense { get; set; }
        public List<CategoryExpenseDto> CategoryExpenses { get; set; }
        public List<TransactionDto> Transactions { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public List<string> XAxisLabels { get; set; }
    }

   
}
