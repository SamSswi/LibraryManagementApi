using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Api.Services;
using LibraryManagement.Api.Controllers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace LibraryManagement.Tests;
public class AuthenticationServiceTests
{
    private readonly AppDbContext _context;
    public AuthenticationServiceTests() {
        _context = TestDbContextFactory.Create();
    }

    [Fact]
    public async Task Authentication_CorrectPassword_Success()
    {
        var dto = new CreateEditUserDTO{
            Name = "User Name",
            Password = "password"            
        };

        var configuration = TestDataFactory.ConfigurationTest();

        var authService = new AuthService(_context, configuration);

        var user = await authService.RegisterAsync(dto);

        var controller = new AuthController(authService);

        var result = await controller.Login(dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    
    public async Task Authentication_IncorrectPassword_Fail()
    {
        string name = "Test Name";
        var dto = new CreateEditUserDTO
        {
            Name = name,
            Password = "password"
        };

        var configuration = TestDataFactory.ConfigurationTest();
        
        var authService = new AuthService(_context, configuration);
        
        var user = await authService.RegisterAsync(dto);

        var controller = new AuthController(authService);

        var newDTO = new CreateEditUserDTO
        {
            Name = name,
            Password = "wrongPassword"
        };

        var result = await controller.Login(newDTO);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task TokenIdentityClaims_IDNameRole_Pass()
    {
        var dto = new CreateEditUserDTO{
            Name = "User Name",
            Password = "password"
        };

        var configuration = TestDataFactory.ConfigurationTest();

        var authService = new AuthService(_context, configuration);

        var user = await authService.RegisterAsync(dto);
        if(user == null) Assert.Fail("Failed to register the user");

        var tokenResponse = await authService.LoginAsync(dto);
        if(tokenResponse == null) Assert.Fail("Failed to log in the user");
        var jwtString = tokenResponse.AccessToken;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(jwtString);

        var extrId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var extrName = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var extrRole = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        
        var id = user.Id;
        var name = user.Name;
        var role = user.Role;

        Assert.False(string.IsNullOrWhiteSpace(extrId));
        Assert.False(string.IsNullOrWhiteSpace(extrName));
        Assert.False(string.IsNullOrWhiteSpace(extrRole));

        Assert.Equal(id.ToString(), extrId);
        Assert.Equal(name, extrName);
        Assert.Equal(role, extrRole);
    }

    [Fact]
    public async Task RefreshToken_Valid_Pass()
    {
        var dto = new CreateEditUserDTO{
            Name = "Test User",
            Password = "password"            
        };

        var configuration = TestDataFactory.ConfigurationTest();

        var authService = new AuthService(_context, configuration);

        var user = await authService.RegisterAsync(dto);
        if(user == null) Assert.Fail("Failed to register the user");

        var tokenResponse = await authService.LoginAsync(dto);
        if(tokenResponse == null) Assert.Fail("Failed to log in the user");
        var refreshTknString = tokenResponse.RefreshToken;

        var refreshTknDTO = new RefreshTokenRequestDTO
        {
            UserId = user.Id,
            RefreshToken = refreshTknString,
        };

        var controller = new AuthController(authService);

        var result = await controller.RefreshToken(refreshTknDTO);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task RefreshToken_Invalid_FailToRefresh()
    {
        var dto = new CreateEditUserDTO{
            Name = "Test User",
            Password = "password"            
        };

        var configuration = TestDataFactory.ConfigurationTest();

        var authService = new AuthService(_context, configuration);

        var user = await authService.RegisterAsync(dto);
        if(user == null) Assert.Fail("Failed to register the user");

        var tokenResponse = await authService.LoginAsync(dto);
        if(tokenResponse == null) Assert.Fail("Failed to log in the user");

        // generate new refreshToken of a valid format
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshTknString = Convert.ToBase64String(randomNumber);
        
        var refreshTknDTO = new RefreshTokenRequestDTO
        {
            UserId = user.Id,
            RefreshToken = refreshTknString,
        };

        var controller = new AuthController(authService);

        var result = await controller.RefreshToken(refreshTknDTO);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}