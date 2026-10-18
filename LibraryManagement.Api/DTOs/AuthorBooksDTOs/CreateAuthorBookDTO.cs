namespace LibraryManagement.Api.DTO;
using System.ComponentModel.DataAnnotations;

public class CreateAuthorBooksDTO
{
    [Required]
    public int AuthorId {get; set;}

    [Required]
    public int BookId {get; set;}
}