using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController(IAuthService authService) : ControllerBase
{

    [HttpPost ("register_user")]
    [ProducesResponseType(typeof(CreateEditUserDTO), 201)]
    [ProducesResponseType(400)]

    public async Task<ActionResult<User>> RegisterUser([FromForm] CreateEditUserDTO dto)
    {
        var user = await authService.RegisterAsync(dto);
        
        if(user is null)
            return BadRequest("Username already exists");
        return StatusCode(201, user);
    }
    
    [HttpPost ("login")]
    [ProducesResponseType(typeof(CreateEditUserDTO), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TokenResponesDTO>> Login(CreateEditUserDTO dto)
    {
        var response = await authService.LoginAsync(dto);
        if(response is null)
            return Unauthorized("Invalid username or password");
        return Ok(response);
    }

    [HttpPost ("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenRequestDTO), 200)]
    [ProducesResponseType(401)]
    
    public async Task<ActionResult<TokenResponesDTO>> RefreshToken(RefreshTokenRequestDTO request)
    {
        var response = await authService.RefreshTokensAsync(request);
        if(response is null || response.AccessToken is null || response.RefreshToken is null) 
            return Unauthorized("Invalid refresh token");

        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        return Ok("You are authenticated");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet ("admin-only")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are an admin");
    }
}

