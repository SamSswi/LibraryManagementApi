using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class CreateAuthorDTO
{
    [Required]
    public string Name {get; set;} = "";
}