using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class CreateEditUserDTO
{
    [Required]
    public string Name{get; set;} = "";
    [Required]
    public string Password {get; set;} = "";
}