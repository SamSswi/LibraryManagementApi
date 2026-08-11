using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class CreateEditLoanItemDTO
{
    [Required]
    public int BookId {get; set;}
}