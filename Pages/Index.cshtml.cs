using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.InteropServices;
using System.Security.Claims;

 

namespace dotnetcoresample.Pages;

 

public class IndexModel : PageModel
{
    public string Message { get; set; } = "Initial value";
    public string OSVersion { get { return RuntimeInformation.OSDescription; }  }

    private readonly ILogger<IndexModel> _logger;

 

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

 

    public void OnGet()
    {        
    ClaimsPrincipal cp = ClaimsPrincipal.Current;
    string userName = cp?.FindFirst(ClaimTypes.WindowsAccountName).Value;
    Message = String.Format("Hello {0}!", userName);

    }
}
