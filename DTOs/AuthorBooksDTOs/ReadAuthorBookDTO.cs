namespace LibraryManagement.Api.DTO;
using System.ComponentModel.DataAnnotations;

public class AuthorBookReadDTO
{
    public int AuthorId {get; set;}
    public string AuthorName {get; set;} = "";
    public int BookId {get; set;}
    public string BookTitle {get; set;} = "";
}

public class AuthorBookReadResponseDTO
{
    public int AuthorId {get; set;}
    public int BookId {get; set;}
}