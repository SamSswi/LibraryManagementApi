using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class CreateEditAuthorDTO
{
    [Required]
    public string Name {get; set;} = "";
}