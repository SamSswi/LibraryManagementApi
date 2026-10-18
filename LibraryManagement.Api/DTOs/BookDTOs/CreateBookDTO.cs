namespace LibraryManagement.Api.DTO;
using System.ComponentModel.DataAnnotations;

public class CreateBookDTO
{
    [Required]
    [StringLength(200)]
    public string Title {get; set;} = "";


    // create a new folder validation with checksum.cs to validate last digit of isbn
    
    [Required]
    [Range(1454, 2050)]
    public int PublicationYear {get; set;}

    public string Genre {get; set;} = "";

    [Required]
    public int TotalCopies {get; set;}
    // might have to separate book and book copy model where will be available copies
}