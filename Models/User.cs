using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Models;

public class User
{
    public int Id {get; private set;}
    [Required]
    public string Name {get; set;} = "";
    public ICollection<Loan>? Loans {get; set;}
    public string PasswordHash {get; set;} = "";
    public string Role {get; set;} = "";
    public string? RefreshToken {get; set;}
    public DateTime? RefreshTokenExpiryTime {get; set;}
}