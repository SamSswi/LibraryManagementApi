using LibraryManagement.Api.Data;
using LibraryManagement.Api.GlobalExceptionHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ReturnController(AppDbContext context) : BaseController(context)
{

    [Authorize (Roles = "Admin")]
    [HttpPut ("loanItems/{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult> ReturnItem(int id)
    {
        var loanItem = await _context.LoanItem.FirstOrDefaultAsync(li => li.Id == id)
            ?? throw new NotFoundException("LoanItem not found");

        if(loanItem.ReturnedAt != null) throw new ConflictException($"loan already returned on {loanItem.ReturnedAt}");
        loanItem.ReturnedAt = DateOnly.FromDateTime(DateTime.Today);
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == loanItem.BookId) ?? throw new NotFoundException("Book not Found");
        await AvailableCopiesCalc(book.Id, Operands.Plus);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpPut ("me/loanItems/{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult> ReturnBook(int id)
    {
        int userId = ExtractId();
        var loanItem = await _context.LoanItem.FirstOrDefaultAsync(li => li.Id == id)
            ?? throw new NotFoundException("LoanItem not found");
        var loan = await FindLoanById(loanItem.LoanId);
        if(userId != loan.UserId) throw new ConflictException($"The book is not loaned to this user");
        if(loanItem.ReturnedAt != null) throw new ConflictException($"loan already returned on {loanItem.ReturnedAt}");
        loanItem.ReturnedAt = DateOnly.FromDateTime(DateTime.Today);
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == loanItem.BookId) ?? throw new NotFoundException("Book not Found");
        await AvailableCopiesCalc(book.Id, Operands.Plus);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}