using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.GlobalExceptionHandler;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthorBookController(AppDbContext context) : BaseController(context)
{
    [Authorize(Roles = "Admin")]
    [HttpGet ("author-books")]
    [ProducesResponseType(typeof(List<AuthorBookReadDTO>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<AuthorBookReadDTO>>> AllAuthorBooks(int page = 1, int pageSize = 10)
    {
        var query = _context.AuthorBooks.Select(ab => new AuthorBookReadDTO
        {
            AuthorId = ab.AuthorId,
            AuthorName = ab.Author.Name,
            BookId = ab.BookId,
            BookTitle = ab.Book.Title,
        });
        var totalAuthorBooks = await query.CountAsync();

        
        Dictionary<string, int> normData = NormalizePageData(page, pageSize, totalAuthorBooks);

        var abPerPage = await query
            .OrderBy(ab => ab.AuthorId).ThenBy(ab => ab.BookId)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();

        return Ok(abPerPage);
    }
    
    [Authorize (Roles = "Admin")]
    [HttpGet ("authors/{authorId}/books")]
    [ProducesResponseType(typeof(List<ReadBooksDTO>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<ReadBooksDTO>>> FindBooksByAnAuthor(int authorId)
    {
        await FindAuthorById(authorId);
        var books = await _context.AuthorBooks
            .Where(ab => ab.AuthorId == authorId)
            .Select(ab => new ReadBooksDTO
            {
                Id = ab.Book.Id,
                Title = ab.Book.Title,
                Author = new List<string> {ab.Author.Name},
                PublicationYear = ab.Book.PublicationYear,
                Genre = ab.Book.Genre,
                TotalCopies = ab.Book.TotalCopies,
                AvailableCopies = ab.Book.AvailableCopies                
            }) 
            .ToListAsync();
        return Ok(books);
    }

    [Authorize (Roles = "Admin")]
    [HttpGet ("books/{bookId}/authors")]
    [ProducesResponseType(typeof(List<ReadAuthorDTO>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<ReadAuthorDTO>>> FindAuthorsOfABook(int bookId)
    {
        await FindBookById(bookId);
        var books = await _context.AuthorBooks
            .Where(ab => ab.BookId == bookId)
            .Select(ab => new ReadAuthorDTO
            {
                Id = ab.Author.Id,
                Name = ab.Author.Name,
                Books = new List<string> {ab.Book.Title}
            })
            .ToListAsync();
        return Ok(books);
    }

    private async Task CheckRelationship(int authorId, int bookId)
    {
        var checkRel = await _context.AuthorBooks.FirstOrDefaultAsync(ab => ab.AuthorId == authorId && ab.BookId == bookId);
        if(checkRel != null) throw new ConflictException("The Author Book relationship exists or the book is duplicated in the memory");
    }

    [Authorize (Roles = "Admin")]
    [HttpPost ("author-books")]
    [ProducesResponseType(typeof(AuthorBookReadResponseDTO), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> AddAuthorBook([FromForm] CreateAuthorBooksDTO dto)
    {
        await FindAuthorById(dto.AuthorId);
        var book = await FindBookById(dto.BookId);
        await CheckRelationship(dto.AuthorId, dto.BookId);
        
        // Prevent an author from being associated with multiple
        // book records that share the same title.
        var booksSameTitle = await _context.Book.Where(b => b.Title.ToLower() == book.Title.ToLower()).ToListAsync();
        foreach(var bk in booksSameTitle)
        {
            await CheckRelationship(dto.AuthorId, bk.Id);
        }
        var ab = new AuthorBook
        {
            AuthorId = dto.AuthorId,
            BookId = dto.BookId
        };
        _context.Add(ab);
        await _context.SaveChangesAsync();
        return StatusCode(201, new AuthorBookReadResponseDTO
        {
            AuthorId = ab.AuthorId,
            BookId = ab.BookId
        });
    }

    [Authorize (Roles = "Admin")]
    [HttpDelete ("author/{authorId}/book/{bookId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]

    public async Task<IActionResult> DeleteAuthorBookRel(int authorId, int bookId)
    {
        await FindAuthorById(authorId);
        await FindBookById(bookId);
        var currAuthBook = await _context.AuthorBooks.FirstOrDefaultAsync(ab => ab.AuthorId == authorId && ab.BookId == bookId) 
            ?? throw new NotFoundException("Author - Book Relation not found");
        _context.AuthorBooks.Remove(currAuthBook);
        await _context.SaveChangesAsync();
        return StatusCode(204);
    }    
}