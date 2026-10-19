using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using LibraryManagement.Api.GlobalExceptionHandler;
using System.Text.Json.Serialization;
namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class LoanController(AppDbContext context) : BaseController (context)
{

    private async Task CheckBookAvailability(int bookId)
    {
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == bookId) ?? throw new NotFoundException("Book not found");
        if(book.AvailableCopies == 0) throw new ConflictException($"The book {book.Title} is not available for loan");

    }

    private async Task CheckIfUserHasTheCopy(int bookId, int userId)
    {
        var query = _context.Loan.AsQueryable();
        query = query.Where(u => u.UserId == userId);
        var duplicateLoans = await query.AnyAsync(l=>l.LoanItems.Any(li => li.BookId == bookId));
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == bookId);
        string bookTitle = book != null ? book.Title : throw new NotFoundException("The book title is missing");
        if(duplicateLoans) throw new ConflictException($"The book {bookTitle} - id:{bookId} is already loaned by user {userId}");
    }

    private DateOnly today = DateOnly.FromDateTime(DateAndTime.Now);

    private string CalcStatus(DateOnly dueDate, DateOnly? dateReturned)
    {
        string status =  dateReturned != null ? LoanItemStatus.Returned.ToString() 
                    : dueDate >= today ? $"{LoanItemStatus.Current} Loan. Return Deadline: {dueDate}" 
                    : LoanItemStatus.Overdue.ToString();
        return status;
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    private enum LoanItemStatus
    {
        Returned,
        Current,
        Overdue
    }
    
    [Authorize (Roles = "Admin")]
    [HttpGet ("loans")]
    public async Task<ActionResult<IEnumerable<ReadLoanDTO>>> GetAllLoans([FromQuery] LoanFilter filter)
    {
        var query = _context.Loan.AsQueryable();

        if (filter.UserId.HasValue)
        {
            query = query.Where(u => u.UserId == filter.UserId.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            query = query.Where(u => u.User != null && u.User.Name.ToLower().Contains(filter.UserName.ToLower()));
        }
        if (filter.BorrowedAt.HasValue)
        {
            query = query.Where(b => b.BorrowedAt == filter.BorrowedAt.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.BookTitle))
        {
            query = query.Where(bT => bT.LoanItems.Any(item => item.Book != null && 
                        item.Book.Title.ToLower().Contains(filter.BookTitle.ToLower())));
        }
        if(filter.Category == LoanItemCategory.All)
        {
            query = query.Where(l => l.LoanItems.Any());
        }
        
        if(filter.Category == LoanItemCategory.PastLoans)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt != null));
        }
        if(filter.Category == LoanItemCategory.Current)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt == null && today <= li.DueDate));
        }
        if(filter.Category == LoanItemCategory.Overdue)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt == null && today > li.DueDate));
        }



        var loan = query.Select(l => new ReadLoanDTO
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.User.Name,
            BorrowedAt = l.BorrowedAt,
            LoanItems = l.LoanItems.Select(li => new ReadLoanItemDTO
            {
                Id = li.Id,
                DueDate = li.DueDate,
                ReturnedAt = li.ReturnedAt,
                BookId = li.BookId,
                BookTitle = li.Book.Title,
                Status = li.ReturnedAt != null ? LoanItemStatus.Returned.ToString() 
                    : li.DueDate >= today ? $"{LoanItemStatus.Current} Loan. Return Deadline: {li.DueDate}" 
                    : LoanItemStatus.Overdue.ToString()
            }).ToList()
        });

        var totalLoans = await loan.CountAsync();

        Dictionary<string, int> normData = NormalizePageData(filter.Page, filter.PageSize, totalLoans);

        var loansPerPage = await loan
            .OrderBy(u => u.Id)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();
            
        return Ok(loansPerPage);        
    }


    [Authorize]
    [HttpGet ("me/loans")]
    public async Task<ActionResult<IEnumerable<ReadLoanDTO>>> GetOwnLoans([FromQuery] LoanItemFilter filter)
    {
        var query = _context.Loan.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.BookTitle))
        {
            query = query.Where(l => l.LoanItems.Any(b => b.Book != null && b.Book.Title.ToLower().Contains(filter.BookTitle.ToLower())));
        }
        if(filter.Category == LoanItemCategory.All)
        {
            query = query.Where(l => l.LoanItems.Any());
        }
        
        if(filter.Category == LoanItemCategory.PastLoans)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt != null));
        }
        if(filter.Category == LoanItemCategory.Current)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt == null && today <= li.DueDate));
        }
        if(filter.Category == LoanItemCategory.Overdue)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt == null && today > li.DueDate));
        }

        int uId = ExtractId();

        var loans = query.Where(l => l.UserId == uId).Select(l => new ReadLoanDTO
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.User.Name,
            BorrowedAt = l.BorrowedAt,
            LoanItems = l.LoanItems.Select(li => new ReadLoanItemDTO
            {
                Id = li.Id,
                Status = li.ReturnedAt != null ? LoanItemStatus.Returned.ToString() 
                    : li.DueDate >= today ? $"{LoanItemStatus.Current} Loan. Return Deadline: {li.DueDate}" 
                    : LoanItemStatus.Overdue.ToString(),
                DueDate = li.DueDate,
                BookId = li.BookId,
                BookTitle = li.Book.Title
            }).ToList()
        });
            
        return Ok(loans);        
    }



    [Authorize (Roles = "Admin")]  
    [HttpGet("loans/{id}")]
    public async Task<ActionResult<ReadLoanDTO>> GetLoanById(int id)
    {
        var currLoan = await FindLoanById(id);
        var loan = new ReadLoanDTO
        {
          Id = currLoan.Id,
          UserId = currLoan.UserId,
          UserName = currLoan.User.Name,
          BorrowedAt = currLoan.BorrowedAt,
          LoanItems = [.. currLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title,
              Status = li.ReturnedAt != null ? LoanItemStatus.Returned.ToString() 
                    : li.DueDate >= today ? $"{LoanItemStatus.Current} Loan. Return Deadline: {li.DueDate}" 
                    : LoanItemStatus.Overdue.ToString()
          })]
        };
        return loan;
    }

    [Authorize (Roles = "Admin")]
    [HttpPost ("loans")]
    public async Task<IActionResult> AddLoan([FromBody] CreateLoanDTO dto)
    {
        await FindUserById(dto.UserId);
        var loan = new Loan
        {
            UserId = dto.UserId,
            BorrowedAt = DateOnly.FromDateTime(DateTime.Today)
        };
        foreach(var item in dto.LoanItems)
        {
            loan.LoanItems.Add(new LoanItem
            {
                BookId = item.BookId,
                DueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14)
            });
            await CheckBookAvailability(item.BookId);
            await AvailableCopiesCalc(item.BookId, Operands.Minus);
            await CheckIfUserHasTheCopy(item.BookId, dto.UserId);
            
        }
        _context.Loan.Add(loan);
        await _context.SaveChangesAsync();
        var createdLoan = await FindLoanById(loan.Id);
        return Ok(new ReadLoanResponseDTO
        {
            Id = createdLoan.Id,
            UserId = createdLoan.UserId,
            UserName = createdLoan.User.Name,
            BorrowedAt = createdLoan.BorrowedAt,
            LoanItems = [.. createdLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title,
              Status = li.ReturnedAt != null ? LoanItemStatus.Returned.ToString() 
                    : li.DueDate >= today ? $"{LoanItemStatus.Current} Loan. Return Deadline: {li.DueDate}" 
                    : "Overdue"
          })]
        });
    }

    [Authorize]
    [HttpPost ("me/loans")]
    public async Task<IActionResult> Borrow([FromBody] BorrowDTO dto)
    {
        int id = ExtractId();
        var loan = new Loan
        {
            UserId = id,
            BorrowedAt = DateOnly.FromDateTime(DateTime.Today)
        };
        foreach(var item in dto.LoanItems)
        {
            loan.LoanItems.Add(new LoanItem
            {
                BookId = item.BookId,
                DueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14)
            });
            await CheckBookAvailability(item.BookId);
            await AvailableCopiesCalc(item.BookId, Operands.Minus);
            await CheckIfUserHasTheCopy(item.BookId, id);
        }
        _context.Loan.Add(loan);
        await _context.SaveChangesAsync();
        var createdLoan = await FindLoanById(loan.Id);
        return Ok(new ReadLoanResponseDTO
        {
            Id = createdLoan.Id,
            UserId = createdLoan.UserId,
            UserName = createdLoan.User.Name,
            BorrowedAt = createdLoan.BorrowedAt,
            LoanItems = [.. createdLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title,
              Status = li.ReturnedAt != null ? LoanItemStatus.Returned.ToString()
                    : li.DueDate >= today ? $"{LoanItemStatus.Current} Loan. Return Deadline: {li.DueDate}" 
                    : LoanItemStatus.Overdue.ToString()
          })]
        });
    }


    
    [Authorize (Roles = "Admin")]
    [HttpPut ("loans/{id}")]
    public async Task<IActionResult> EditLoan(int id, EditLoanDTO dto)
    {
        await FindUserById(dto.UserId);
        var loan = await FindLoanById(id);
        foreach(var item in loan.LoanItems)
        {
            await AvailableCopiesCalc(item.BookId, Operands.Plus);
        }
        loan.UserId = dto.UserId;
        foreach(var itemDTO in dto.LoanItems)
        {
            var item = loan.LoanItems.First(li => li.Id == itemDTO.Id);
            item.DueDate = itemDTO.DueDate;
            item.ReturnedAt = itemDTO.ReturnedAt;
            item.BookId = itemDTO.BookId;
        }
        foreach(var item in loan.LoanItems)
        {
            await CheckBookAvailability(item.BookId);
            await AvailableCopiesCalc(item.BookId, Operands.Minus);
            await CheckIfUserHasTheCopy(item.BookId, dto.UserId);
        }
        await _context.SaveChangesAsync();
        var updatedLoan = await FindLoanById(loan.Id);
        return Ok(new ReadLoanResponseDTO
        {
            Id = updatedLoan.Id,
            UserId = updatedLoan.UserId,
            UserName = updatedLoan.User.Name,
            BorrowedAt = updatedLoan.BorrowedAt,
            LoanItems = [.. updatedLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title,
              Status = li.ReturnedAt != null ? LoanItemStatus.Returned.ToString() 
                    : li.DueDate >= today ? $"{LoanItemStatus.Current} Loan. Return Deadline: {li.DueDate}" 
                    : LoanItemStatus.Overdue.ToString()
          })]
        });
    }
    
    [Authorize (Roles = "Admin")]
    [HttpDelete ("loans/{id}")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var loan = await FindLoanById(id);
        foreach(var item in loan.LoanItems)
        {
            await AvailableCopiesCalc(item.BookId, Operands.Plus);
        }
        _context.Remove(loan);
        await _context.SaveChangesAsync();
        return Ok();
    }
}