namespace ExpenseTracker.Dto
{
    public class TransactionDto
    {
        public DateOnly Date { get; set; }
        public int Income { get; set; }    
        public int Expense { get; set; }
        
    }
}
