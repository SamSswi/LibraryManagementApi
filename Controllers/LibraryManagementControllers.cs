using System.Collections.Immutable;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class LibraryManagementController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;
    
    [HttpGet("all_books")]
    public List<Book> GetBooks()
    {
        var books = _context.Book.ToList();
        return books;
    }

    [HttpPost ("add_book")]
    public IActionResult AddBook([FromForm] Book book)
    {
        _context.Add(book);
        _context.SaveChanges();
        return Ok(book);
    }


}