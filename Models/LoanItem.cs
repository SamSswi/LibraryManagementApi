using System.ComponentModel.DataAnnotations;
namespace LibraryManagement.Api.Models;

public class LoanItem
{
    public int Id {get; private set;}
    [Required]
    public DateOnly DueDate {get; set;}
    public DateOnly? ReturnedAt {get; set;}

    [Required]
    public int LoanId {get; set;} // Foreign Key
    public Loan? Loan {get; set;} // navigation
    [Required]
    public int BookId {get; set;} // Foreign Key
    [Required]
    public Book Book {get; set;} = null!; // navigation


}