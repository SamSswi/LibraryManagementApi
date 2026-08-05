using System.Collections.Immutable;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BookController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private Book FindBookById(int id)
    {
        var currBook = _context.Book.FirstOrDefault(m => m.Id == id) ?? throw new KeyNotFoundException("Book Not Found");
        return currBook;
    }

    // add book not found functionality
    
    [HttpGet("all_books")]
    public async Task<ActionResult<ReadBooksDTO>> GetBooks()
    {
        var books = await _context.Book.Select(b => new ReadBooksDTO
        {
            Id = b.Id,
            Title = b.Title,
            ISBN = b.ISBN,
            PublicationYear = b.PublicationYear,
            Genre = b.Genre,
            TotalCopies = b.TotalCopies,
            AvailableCopies = b.AvailableCopies
        }).ToListAsync();
        return Ok(books);
    }

    [HttpGet("get_book_by_id")]
    public ActionResult<ReadBooksDTO> GetBookById(int id)
    {
        var currBook = FindBookById(id);
        var book = new ReadBooksDTO
        {
            Id = currBook.Id,
            Title = currBook.Title,
            ISBN = currBook.ISBN,
            PublicationYear = currBook.PublicationYear,
            Genre = currBook.Genre,
            TotalCopies = currBook.TotalCopies,
            AvailableCopies = currBook.AvailableCopies
        };
        return book;

    }

    [HttpPost ("add_book")]
    public async Task<IActionResult> AddBook([FromForm] CreateEditBookDTO dto)
    {
        var book = new Book
        {
            Title = dto.Title,
            PublicationYear = dto.PublicationYear,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.AvailableCopies

        };
        _context.Book.Add(book);
        await _context.SaveChangesAsync();
        return Ok(book);
    }


    [HttpPut ("edit_book")]
    public async Task<IActionResult> EditBook(int id, CreateEditBookDTO dto)
    {
        var currBook = FindBookById(id);
        currBook.Title = dto.Title;
        currBook.PublicationYear = dto.PublicationYear;
        currBook.TotalCopies = dto.TotalCopies;
        currBook.AvailableCopies = dto.AvailableCopies;

        await _context.SaveChangesAsync();
        return Ok(currBook);
    }

    [HttpDelete ("delete_book")]
    public IActionResult DeleteBook(int id)
    {
        var currBook = FindBookById(id);
        _context.Remove(currBook);
        _context.SaveChanges();
        return Ok();
    }
}