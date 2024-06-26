using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
#nullable disable

namespace SpendSmart.Pages.Account;

public class RegisterModel(IAmazonDynamoDB dynamoDbClient, ILogger<RegisterModel> logger) : ToasterNotification
{
    private readonly IAmazonDynamoDB _dynamoDbClient = dynamoDbClient;
    private readonly ILogger<RegisterModel> _logger = logger;

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

        if (Input.Password != Input.ConfirmPassword)
        {
            ModelState.AddModelError("Input.Password", "Passwords do not match.");
            GenerateToaster(400);
            return Page();
        }

        var userResponse = await _dynamoDbClient.GetItemAsync("Users", new Dictionary<string, AttributeValue>
        {
            { "UserId", new AttributeValue { S = Input.Username } }
        });

        if (userResponse != null && userResponse.Item != null && userResponse.Item.Count > 0)
        {
            ModelState.AddModelError("Input.Username", "Username already exists.");
            GenerateToaster(502);
            return Page();
        }

        byte[] salt = PasswordHasher.Salter();

        string hashedPassword = PasswordHasher.HashPassword(Input.Password, salt);

        var putItemRequest = new PutItemRequest
        {
            TableName = "Users",
            Item = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = Input.Username } },
                { "Password", new AttributeValue { S = hashedPassword } },
                { "Salt", new AttributeValue { B = new MemoryStream(salt) } }
            }
        };

        var response = await _dynamoDbClient.PutItemAsync(putItemRequest);

        if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Failed to register user.");
            GenerateToaster(500);
            return Page();
        }

        if (HttpContext != null)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, Input.Username) };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        _logger.LogInformation("User registered successfully.");   
        GenerateToaster(2015);
        return RedirectToPage(ReturnUrl);
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

        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
