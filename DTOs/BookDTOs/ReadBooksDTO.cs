using System.ComponentModel.DataAnnotations;
namespace LibraryManagement.Api.DTO;

public class ReadBooksDTO
{
    public int Id {get; set;}
    public string Title {get; set;} = "";
    public string ISBN {get; set;} = "";
    public int PublicationYear {get; set;}
    public string Genre {get; set;} = "";
    public int TotalCopies {get; set;}
    public  int AvailableCopies{get; set;}
    // might have to separate book and book copy model where will be available copies
}