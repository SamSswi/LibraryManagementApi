using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Models;

public class Loan
{
    public int Id {get; private set;}
    [Required]
    public int UserId{get; set;} // Foreign Key
    [Required]
    public User User {get; set;} = null!;
    [Required]
    public DateOnly BorrowedAt {get; set;}
    [Required]
    public ICollection<LoanItem> LoanItems {get; set;} = [];
}