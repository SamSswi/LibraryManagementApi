using System.ComponentModel.DataAnnotations;
namespace LibraryManagement.Api.Models;

public class LoanItem
{
    public int Id {get; private set;}
    [Required]
    public DateOnly DueDate {get; set;} = DateOnly.FromDateTime(DateTime.Today).AddDays(14);
    public DateOnly? ReturnedAt {get; set;}

    [Required]
    public int LoanId {get; set;} // Foreign Key
    public Loan? Loan {get; set;} // navigation
    [Required]
    public int BookId {get; set;} // Foreign Key
    public Book? Book {get; set;} // navigation


}