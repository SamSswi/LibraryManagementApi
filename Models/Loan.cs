namespace LibraryManagement.Api.Models;

public class Loan
{
    public int Id {get; private set;}
    public int UserId{get; set;} // Foreign Key
    public User? User {get; set;}
    public DateOnly BorrowedAt {get; set;}
    public ICollection<LoanItem> LoanItems {get; set;} = [];
}