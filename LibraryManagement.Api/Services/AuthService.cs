using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagement.Api.Services;

public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
{
    public async Task<TokenResponesDTO?> LoginAsync(CreateEditUserDTO request)
    {
        var user = await context.User.FirstOrDefaultAsync(u => u.Name == request.Name);
        if (user is null)
            return null;
        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return null;
        return await CreateTokenResponse(user);
    }

    private async Task<TokenResponesDTO> CreateTokenResponse(User user)
    {
        return new TokenResponesDTO
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }

    public async Task<User?> RegisterAsync(CreateEditUserDTO request)
    {
        if(await context.User.AnyAsync(u => u.Name == request.Name)) 
            return null;

        var user = new User();

        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);        
        user.Name = request.Name;
        user.PasswordHash = hashedPassword;
        user.Role = "user";

        
        context.User.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<TokenResponesDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if(user is null) return null;
        
        return await CreateTokenResponse(user);
    }

    private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await context.User.FindAsync(userId);
        if(user is null || user.RefreshToken != refreshToken 
            || user.RefreshTokenExpiryTime <= DateTime.UtcNow){
                return null;}
        return user; 
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();
        return refreshToken;
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}