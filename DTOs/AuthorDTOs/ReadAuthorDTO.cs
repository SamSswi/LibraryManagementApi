using System.ComponentModel.DataAnnotations;
namespace LibraryManagement.Api.DTO;

public class ReadAuthorDTO
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
}