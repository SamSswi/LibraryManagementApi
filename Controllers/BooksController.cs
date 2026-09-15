using System.Collections.Immutable;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Api.Filter;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagement.Api.Controllers;

[ApiController] // only view for user
[Route("api/[controller]")]

public class BookController(AppDbContext context) : BaseController(context)
{

    private async Task<Book> FindBookById(int id)
    {
        var currBook = await _context.Book.FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException("Book Not Found");
        return currBook;
    }

    // count how many times book was loaned out
    private async Task<int> CountLoanedCopies(Book book)
    {
        var loanedCopies = await _context.LoanItem.Where(i => i.BookId == book.Id).ToListAsync();
        return loanedCopies.Count;
    }

    // add book not found functionality
    
    [HttpGet("all_books")]
    public async Task<ActionResult<IEnumerable<ReadBooksDTO>>> GetBooks([FromQuery] BookFilter filter)
    {
        var query = _context.Book.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(b => b.Title.ToLower().Contains(filter.Search.ToLower()));
        }

        if (filter.PublicationYear.HasValue)
        {
            query = query.Where(b => b.PublicationYear == filter.PublicationYear.Value);
        }

        if (filter.AvailableOnly == true)
        {
            query = query.Where(b => b.AvailableCopies > 0);
        }

        var books = query.Select(b => new ReadBooksDTO
        {
            Id = b.Id,
            Title = b.Title,
            PublicationYear = b.PublicationYear,
            Genre = b.Genre,
            TotalCopies = b.TotalCopies,
            AvailableCopies = b.AvailableCopies
        });
        var totalBooks = await books.CountAsync();

        Dictionary<string, int> normData = NormalizePageData(filter.Page, filter.PageSize, totalBooks);

        var booksPerPage = await books
            .OrderBy(b => b.Id)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();

        return Ok(booksPerPage);
    }

    [Authorize (Roles = "Admin")]
    [HttpGet("get_book_by_id")]
    public async Task<ActionResult<ReadBooksDTO>> GetBookById(int id)
    {
        var currBook = await FindBookById(id);
        var book = new ReadBooksDTO
        {
            Id = currBook.Id,
            Title = currBook.Title,
            PublicationYear = currBook.PublicationYear,
            Genre = currBook.Genre,
            TotalCopies = currBook.TotalCopies,
            AvailableCopies = currBook.AvailableCopies
        };
        return book;

    }
    
    [Authorize (Roles = "Admin")]
    [HttpPost ("add_book")]
    public async Task<IActionResult> AddBook([FromForm] CreateBookDTO dto)
    {
        var book = new Book
        {
            Title = dto.Title,
            PublicationYear = dto.PublicationYear,
            TotalCopies = dto.TotalCopies,
            Genre = dto.Genre,
            AvailableCopies = dto.TotalCopies,

        };
        _context.Book.Add(book);
        await _context.SaveChangesAsync();
        return Ok(book);
    }

    [Authorize (Roles = "Admin")]
    [HttpPut ("edit_book")]
    public async Task<IActionResult> EditBook(int id, EditBookDTO dto)
    {
        var currBook = await FindBookById(id);
        currBook.Title = dto.Title;
        currBook.PublicationYear = dto.PublicationYear;
        currBook.TotalCopies = dto.TotalCopies;
        currBook.AvailableCopies = dto.TotalCopies - await CountLoanedCopies(currBook);

        await _context.SaveChangesAsync();
        return Ok(currBook);
    }
    
    [Authorize (Roles = "Admin")]
    [HttpDelete ("delete_book")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var currBook = await FindBookById(id);
        _context.Remove(currBook);
        await _context.SaveChangesAsync();
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