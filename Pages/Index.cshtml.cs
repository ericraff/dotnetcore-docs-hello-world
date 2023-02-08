using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace dotnetcoresample.Pages;

public class IndexModel : PageModel
{

    private readonly ILogger<IndexModel> _logger;
    public List<Claim>? Claims;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }


    public void OnGet()
    {

        try
        {
            Claims = HttpContext.User.Claims.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not decode token", ex.Message);

        }

    }

}
