using System.ComponentModel.DataAnnotations;
namespace LibraryManagement.Api.DTO;

public class EditAuthorDTO
{
    [Required]
    public string Name {get; set;} = "";
}
