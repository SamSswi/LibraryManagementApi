using System.Security.Claims;
using LibraryManagement.Api.Controllers;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public static class TestDataFactory
{
    public static Book CreateTestBook(int availableCopies = 1)
    {
        return new Book
        {
            Title = "Test Book",
            PublicationYear = 1900,
            TotalCopies = 1,
            AvailableCopies = availableCopies
        };

    }

    public static User CreateTestUser(string Role = "user")
    {
        return new User
        {
            Name = "Test User"            
        };
    }

    public static Loan CreateTestLoan(int userId, int bookId)
    {
        return new Loan
        {
            UserId = userId,
            BorrowedAt = DateOnly.FromDateTime(DateTime.Today),
            LoanItems = [new(){BookId = bookId}]
        };
    }

    public static LoanController CreateAuthenticatedLoanController(AppDbContext context, int userId)
    {
        var controller = new LoanController(context);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "TestAuth")
                )
            }
        };
        return controller;
    }

    public static ReturnController CreateAuthenticatedReturnController(AppDbContext context, int userId)
    {
        var controller = new ReturnController(context);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "TestAuth")
                )
            }
        };
        return controller;
    }

    public static IConfigurationRoot ConfigurationTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:Token"] = "TestSecretKey12345678901234567890123456789012345678901234567890q",
                ["AppSettings:Issuer"] = "LibraryManagementAPI",
                ["AppSettings:Audience"] = "LibraryUsers"
            }).Build();

        return configuration;
    }
}