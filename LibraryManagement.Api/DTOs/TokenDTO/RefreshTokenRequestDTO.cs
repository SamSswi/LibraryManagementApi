namespace LibraryManagement.Api.DTO;

public class RefreshTokenRequestDTO
{
    public int UserId {get; set;}
    public required string RefreshToken {get; set;}
}