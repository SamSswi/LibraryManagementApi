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

public class BookController(AppDbContext context) : BaseController
{
    private readonly AppDbContext _context = context;

    private Book FindBookById(int id)
    {
        var currBook = _context.Book.FirstOrDefault(m => m.Id == id) ?? throw new KeyNotFoundException("Book Not Found");
        return currBook;
    }

    // add book not found functionality
    
    [HttpGet("all_books")]
    public async Task<ActionResult<IEnumerable<ReadBooksDTO>>> GetBooks(int page = 1, int pageSize = 10)
    {
        var query = _context.Book.Select(b => new ReadBooksDTO
        {
            Id = b.Id,
            Title = b.Title,
            ISBN = b.ISBN,
            PublicationYear = b.PublicationYear,
            Genre = b.Genre,
            TotalCopies = b.TotalCopies,
            AvailableCopies = b.AvailableCopies
        });
        var totalBooks = await query.CountAsync();

        Dictionary<string, int> normData = NormalizePageData(page, pageSize, totalBooks);

        var booksPerPage = await query
            .OrderBy(b => b.Id)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();

        return Ok(booksPerPage);
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

    // [HttpPost ("seed-books")]
    // public async Task<IActionResult> SeedBooks()
    // {
    //     for(int i = 0; i < 100; i++)
    //     {
    //         _context.Book.Add(new Book
    //         {
    //             Title = $"Book{i}",
    //             PublicationYear = 1460 + i,
    //             TotalCopies = 3,
    //             AvailableCopies = 0
    //         });
    //     }
    //     await _context.SaveChangesAsync();
    //     return Ok();
    // }
}