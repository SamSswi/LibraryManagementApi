using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Models;

public class Book
{
    public int Id {get; private set;}

    [Required]
    [StringLength(200)]
    public string Title {get; set;} = "";
    
    [Required]
    [Range(1454, 2050)]
    public int PublicationYear {get; set;}
    public string Genre {get; set;} = "";

    [Required]
    public int TotalCopies {get; set;}
    
    [Required]
    public  int AvailableCopies{get; set;}

    public List<AuthorBook> AuthorBooks {get; set;} = [];
    public ICollection<LoanItem> LoanItems {get; set;} = [];
}