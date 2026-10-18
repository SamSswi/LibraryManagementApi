namespace LibraryManagement.Api.DTO;

public class TokenResponesDTO
{
    public required string AccessToken {get; set;}
    public required string RefreshToken {get; set;}
}