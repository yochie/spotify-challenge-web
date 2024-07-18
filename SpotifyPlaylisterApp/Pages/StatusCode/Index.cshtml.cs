using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyPlaylisterApp.Pages;

public class StatusCodeModel : PageModel
{

    private readonly ILogger<ErrorModel> _logger;
    public int? ErrorStatusCode {get; set;}
    public StatusCodeModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet(int statusCode)
    {
        this.ErrorStatusCode = statusCode;
    }
}

