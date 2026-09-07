using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController] // admin only
[Route("api/[controller]")]

public class ReturnEndpoint(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [Authorize (Roles = "Admin")]
    [HttpPut ("return/loan-item-{id}")]
    public async Task<ActionResult> ReturnItem(int id)
    {
        var loanItem = await _context.LoanItem.FirstOrDefaultAsync(li => li.Id == id)
            ?? throw new KeyNotFoundException("LoanItem not found");

        if(loanItem.ReturnedAt != null) return Conflict($"loan already returned on {loanItem.ReturnedAt}");
        loanItem.ReturnedAt = DateOnly.FromDateTime(DateTime.Today);
        await _context.SaveChangesAsync();
        return Ok();
    }
}