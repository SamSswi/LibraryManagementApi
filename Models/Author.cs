using System.ComponentModel.DataAnnotations;


namespace LibraryManagement.Api.Models;

public class Author
{
    public int Id {get; private set;}

    [Required]
    public string Name = "";

    public ICollection<AuthorBook> AuthorBooks {get; set;} = [];  
}