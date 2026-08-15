using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    protected Dictionary<string, int>  NormalizePageData(int page, int pageSize, int totalInstances)
    {
        if(pageSize < 1) pageSize = 1;
        var totalPages = (int) Math.Ceiling((decimal) totalInstances / pageSize);
        if(pageSize > totalInstances) pageSize = totalInstances;
        if(page < 1) page = 1;
        else if(page > totalPages) page = totalPages;
        
        var pageData = new Dictionary<string, int> 
        {
            {"page", page},
            {"pageSize", pageSize}
        };
        return pageData;
    }
}
