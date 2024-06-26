using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
#nullable disable

namespace SpendSmart.Pages.Account;

public class LogoutModel(ILogger<LogoutModel> logger) : ToasterNotification
{
    private readonly ILogger<LogoutModel> _logger = logger;

    public async Task<IActionResult> OnGet(string returnUrl = null)
    {
        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

        if (User.Identity.IsAuthenticated)
        {   
            _logger.LogInformation("User logged off.");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            GenerateToaster(2016);
        }

        return LocalRedirect(returnUrl);
    }
}
