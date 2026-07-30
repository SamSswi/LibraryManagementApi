using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class LibraryManagementController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;
}