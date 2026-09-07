using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SQLitePCL;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController(IAuthService authService) : BaseController
{

    [HttpPost ("register_user")]
    public async Task<ActionResult<User>> RegisterUser([FromForm] CreateEditUserDTO dto)
    {
        var user = await authService.RegisterAsync(dto);
        
        if(user is null)
            return BadRequest("Username already exists");
        return Ok(user);
    }
    
    [HttpPost ("login")]
    public async Task<ActionResult<TokenResponesDTO>> Login(CreateEditUserDTO dto)
    {
        var response = await authService.LoginAsync(dto);
        if(response is null)
            return BadRequest("Invalid username or password");
        return Ok(response);
    }

    [HttpPost ("refresh-token")] //automatic
    public async Task<ActionResult<TokenResponesDTO>> RefreshToken(RefreshTokenRequestDTO request)
    {
        var response = await authService.RefreshTokensAsync(request);
        if(response is null || response.AccessToken is null || response.RefreshToken is null) 
            return Unauthorized("Invalid refresh token");

        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        return Ok("You are authenticated");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet ("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are an admin");
    }
}

