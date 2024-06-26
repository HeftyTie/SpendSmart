using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
#nullable disable

namespace SpendSmart.Pages.Account;

public class LoginModel(IAmazonDynamoDB dynamoDbClient, ILogger<LoginModel> logger) : ToasterNotification
{
    private readonly IAmazonDynamoDB _dynamoDbClient = dynamoDbClient;
    private readonly ILogger<LoginModel> _logger = logger;

    public IActionResult OnGet(string returnUrl = null)
    {
        if (User.Identity.IsAuthenticated)
        {
            _logger.LogInformation("User is already authenticated.");
            return LocalRedirect(returnUrl ?? "/");
        }

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("Input", "Invalid input.");
            return Page();
        }

        var userResponse = await _dynamoDbClient.GetItemAsync("Users", new Dictionary<string, AttributeValue>
        {
            { "UserId", new AttributeValue { S = Input.Username } }
        });

        if (userResponse != null && userResponse.Item.Count < 1)
        {
            ModelState.AddModelError("Input.Username", "Username not registered.");
            GenerateToaster(501);
            return Page();
        }

		byte[] salt = userResponse.Item["Salt"].B.ToArray();

		string hashedPassword = PasswordHasher.HashPassword(Input.Password, salt);

		if (hashedPassword != userResponse.Item["Password"].S)
        {
            ModelState.AddModelError("Input.Password", "Incorrect password.");
            GenerateToaster(400);
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, Input.Username)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            RedirectUri = ReturnUrl
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        _logger.LogInformation("User logged in.");
        GenerateToaster(201);
        return LocalRedirect(ReturnUrl ?? "/");
    }

    [BindProperty]
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }

    public class InputModel()
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
