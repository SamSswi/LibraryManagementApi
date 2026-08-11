using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UserController(AppDbContext context) : BaseController
{
    private readonly AppDbContext _context = context;
}