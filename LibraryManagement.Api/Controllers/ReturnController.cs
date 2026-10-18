using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.GlobalExceptionHandler;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController] // admin only
[Route("api/[controller]")]

public class ReturnController(AppDbContext context) : BaseController(context)
{

    [Authorize (Roles = "Admin")]
    [HttpPut ("loanItems/{id}")]
    public async Task<ActionResult> ReturnItem(int id)
    {
        var loanItem = await _context.LoanItem.FirstOrDefaultAsync(li => li.Id == id)
            ?? throw new NotFoundException("LoanItem not found");

        if(loanItem.ReturnedAt != null) throw new ConflictException($"loan already returned on {loanItem.ReturnedAt}");
        loanItem.ReturnedAt = DateOnly.FromDateTime(DateTime.Today);
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == loanItem.BookId) ?? throw new NotFoundException("Book not Found");
        await AvailableCopiesCalc(book.Id, Operands.Plus);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpPut ("me/loanItems/{id}")]
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
        return Ok();
    }

}