using System.Collections.Immutable;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Api.Filter;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BookController(AppDbContext context) : BaseController(context)
{
    private async Task<int> CountLoanedCopies(Book book)
    {
        var loanedCopies = await _context.LoanItem.Where(i => i.BookId == book.Id).ToListAsync();
        return loanedCopies.Count;
    }
    
    [HttpGet("books")]
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
            Author = b.AuthorBooks.Select(ab => ab.Author.Name).ToList(),
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
    [HttpGet("books/{id}")]
    public async Task<ActionResult<ReadBooksDTO>> GetBookById(int id)
    {
        var currBook = await FindBookById(id);
        var book = new ReadBooksDTO
        {
            Id = currBook.Id,
            Title = currBook.Title,
            Author = currBook.AuthorBooks.Select(ab => ab.Author.Name).ToList(),
            PublicationYear = currBook.PublicationYear,
            Genre = currBook.Genre,
            TotalCopies = currBook.TotalCopies,
            AvailableCopies = currBook.AvailableCopies
        };
        return book;

    }
    
    [Authorize (Roles = "Admin")]
    [HttpPost ("books")]
    public async Task<ActionResult<ReadBooksDTO>> AddBook([FromForm] CreateBookDTO dto)
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
        var newBook = await FindBookById(book.Id);
        return Ok(new ReadBooksDTO
        {
            Id = newBook.Id,
            Title = newBook.Title,
            Author = newBook.AuthorBooks.Select(ab => ab.Author.Name).ToList(),
            PublicationYear = newBook.PublicationYear,
            Genre = newBook.Genre,
            TotalCopies = newBook.TotalCopies,
            AvailableCopies = newBook.AvailableCopies
        });
    }

    [Authorize (Roles = "Admin")]
    [HttpPut ("books/{id}")]
    public async Task<ActionResult<ReadBooksDTO>> EditBook(int id, EditBookDTO dto)
    {
        var currBook = await FindBookById(id);
        currBook.Title = dto.Title;
        currBook.PublicationYear = dto.PublicationYear;
        currBook.TotalCopies = dto.TotalCopies;
        // Recalculate available copies when the total changes,
        // accounting for copies currently on loan.
        currBook.AvailableCopies = dto.TotalCopies - await CountLoanedCopies(currBook);

        await _context.SaveChangesAsync();
        return Ok(new ReadBooksDTO
        {
            Id = currBook.Id,
            Title = currBook.Title,
            Author = currBook.AuthorBooks.Select(ab => ab.Author.Name).ToList(),
            PublicationYear = currBook.PublicationYear,
            Genre = currBook.Genre,
            TotalCopies = currBook.TotalCopies,
            AvailableCopies = currBook.AvailableCopies
        });
    }
    
    [Authorize (Roles = "Admin")]
    [HttpDelete ("books/{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var currBook = await FindBookById(id);
        _context.Remove(currBook);
        await _context.SaveChangesAsync();
        return Ok();
    }
}