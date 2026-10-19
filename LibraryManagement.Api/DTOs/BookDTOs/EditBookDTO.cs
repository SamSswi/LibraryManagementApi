namespace LibraryManagement.Api.DTO;
using System.ComponentModel.DataAnnotations;

public class EditBookDTO
{
    [Required]
    [StringLength(200)]
    public string Title {get; set;} = "";

    [Required]
    [Range(1454, 2050)]
    public int PublicationYear {get; set;}

    public string Genre {get; set;} = "";

    [Required]
    public int TotalCopies {get; set;}
}