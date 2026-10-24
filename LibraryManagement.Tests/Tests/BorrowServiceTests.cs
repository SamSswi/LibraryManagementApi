namespace LibraryManagement.Tests;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Api.GlobalExceptionHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

public class BorrowServiceTests
{
    private readonly AppDbContext _context;
    public BorrowServiceTests() {
        _context = TestDbContextFactory.Create();
    }

    [Fact]

    public async Task BorrowBook_CopyIsAvailableNotBorrowedBefore_LoanCreated()
    {
        var book = TestDataFactory.CreateTestBook();
        _context.Book.Add(book);
        var user = TestDataFactory.CreateTestUser();
        _context.User.Add(user);

        await _context.SaveChangesAsync();

        var dto = new BorrowDTO {LoanItems = [new(){BookId = book.Id}]};
        var controller = TestDataFactory.CreateAuthenticatedLoanController(_context, user.Id);
        
        var actionResult = await controller.Borrow(dto);
        var result = Assert.IsType<ObjectResult>(actionResult);

        Assert.Equal(201, result.StatusCode);

        Assert.IsType<ReadLoanResponseDTO>(result.Value);

        var loans = await _context.Loan.ToListAsync();
        Assert.NotEmpty(loans);
    }
    [Fact]
    public async Task BorrowBook_WhenNoCopiesAvailable_ShouldNotCreateLoan()
    {
        var book = TestDataFactory.CreateTestBook(availableCopies: 0);
        var user = TestDataFactory.CreateTestUser(Role: "user");

        _context.Book.Add(book);
        _context.User.Add(user);
        await _context.SaveChangesAsync();

        var dto = new BorrowDTO
        {
            LoanItems = new List<CreateLoanItemDTO>
            {
                new CreateLoanItemDTO
                {
                    BookId = book.Id
                }
            }
        };

        var controller = TestDataFactory.CreateAuthenticatedLoanController(_context, user.Id);

        
        await Assert.ThrowsAsync<ConflictException>(
            () => controller.Borrow(dto)
        );

        var loans = await _context.Loan.ToListAsync();
        Assert.Empty(loans);       
    }

    [Fact]
    public async Task BorrowBook_AlreadyBorrowed_ShouldNotCreateLoan()
    {
        var book = TestDataFactory.CreateTestBook();
        _context.Book.Add(book);
        var user = TestDataFactory.CreateTestUser();
        _context.User.Add(user);
        await _context.SaveChangesAsync();
        var loan = TestDataFactory.CreateTestLoan(user.Id, book.Id);
        _context.Loan.Add(loan);
        await _context.SaveChangesAsync();

        var dto = new BorrowDTO
        {
            LoanItems = new List<CreateLoanItemDTO>
            {
                new CreateLoanItemDTO
                {
                    BookId = book.Id
                }
            }
        };

        var controller = TestDataFactory.CreateAuthenticatedLoanController(_context, user.Id);

        await Assert.ThrowsAsync<ConflictException>(
            () => controller.Borrow(dto)
        );

        var loans = await _context.Loan.ToListAsync();
        var numOfLoans = loans.Count;

        Assert.Equal(1, numOfLoans);
    }

    [Fact]
    public async Task BorrowBook_MissingBook_ShouldNotCreateLoan()
    {
        var user = TestDataFactory.CreateTestUser();
        _context.User.Add(user);
        var bookId = 0;
        await _context.SaveChangesAsync();

        var dto = new BorrowDTO
        {
            LoanItems = new List<CreateLoanItemDTO>
            {
                new CreateLoanItemDTO
                {
                    BookId = bookId
                }
            }
        };

        var controller = TestDataFactory.CreateAuthenticatedLoanController(_context, user.Id);
        await Assert.ThrowsAsync<NotFoundException>(() => controller.Borrow(dto));

        var loans = await _context.Loan.ToListAsync();
        Assert.Empty(loans);
    }
        [Fact]
    public async Task CreateLoan_MissingUser_ShouldNotCreateLoan()
    {
        var book = TestDataFactory.CreateTestBook();
        _context.Book.Add(book);
        var admin = TestDataFactory.CreateTestUser();
        _context.User.Add(admin);
        await _context.SaveChangesAsync();

        var dto = new CreateLoanDTO
        {
            UserId = 0,
            LoanItems = new List<CreateLoanItemDTO>
            {
                new CreateLoanItemDTO
                {
                    BookId = book.Id
                }
            }
        };

        var controller = TestDataFactory.CreateAuthenticatedLoanController(_context, admin.Id);
        await Assert.ThrowsAsync<NotFoundException>(() => controller.AddLoan(dto));

        var loans = await _context.Loan.ToListAsync();
        Assert.Empty(loans);
    }
}
