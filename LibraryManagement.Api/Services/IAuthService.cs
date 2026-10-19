using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;

namespace LibraryManagement.Api.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(CreateEditUserDTO request);
    Task<TokenResponesDTO?> LoginAsync(CreateEditUserDTO request);
    Task<TokenResponesDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request);
}