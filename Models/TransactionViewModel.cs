namespace ExpenseTracker.Models
{
    public class TransactionViewModel
    {
        public List<Transaction> Transactions { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
