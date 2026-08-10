using System.ComponentModel.DataAnnotations;
namespace LibraryManagement.Api.Models;

public class LoanItem
{
    public int Id {get; private set;}
    public DateOnly DueDate {get; set;}
    public DateOnly? ReturnedAt {get; set;}

    public int LoanId {get; set;} // Foreign Key
    public Loan? Loan {get; set;} // navigation
    public int BookId {get; set;} // Foreign Key
    public Book? Book {get; set;} // navigation


}