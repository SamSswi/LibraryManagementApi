using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace LibraryManagement.Api.Models;

public class Book
{
    public int Id {get; private set;}

    [Required]
    [StringLength(200)]
    public string Title {get; set;} = "";


    // create a new folder validation with checksum.cs to validate last digit of isbn
    [Required]
    [RegularExpression(@"^(\d{10}|\d{13})$")]
    public string ISBN {get; set;} = "";
    
    [Range(1454, 2050)]
    public required int PublicationYear {get; set;}
    public string Genre {get; set;} = "";

    [Required]
    public string Author {get; set;} = "";

    [Required]
    public int TotalCopies {get; set;}
    
    [Required]
    public  int AvailableCopies{get; set;}
    // might have to separate book and book copy model where will be available copies
}