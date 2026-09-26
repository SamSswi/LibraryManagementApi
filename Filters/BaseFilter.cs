using System.Text.Json.Serialization;

namespace LibraryManagement.Api.Filter;

public abstract class BaseFilter
{
    public int Page {get; set;} = 1;
    public int PageSize{get; set;} = 10;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoanItemCategory
{
    PastLoans,
    Overdue,
    Current,
    All
}