using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace SpendSmart.Pages.Account._Manage;

[Authorize]
public class ChangePasswordModel(IAmazonDynamoDB dynamoDbClient, ILogger<ChangePasswordModel> logger) : ToasterNotification
{
    private readonly IAmazonDynamoDB _dynamoDbClient = dynamoDbClient;
    private readonly ILogger<ChangePasswordModel> _logger = logger;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("Input", "Invalid input.");
            GenerateToaster(400);
            return Page();
        }

        string userId = User?.Identity?.Name ?? "";
        var userResponse = await _dynamoDbClient.GetItemAsync("Users", new Dictionary<string, AttributeValue>
        {
            { "UserId", new AttributeValue { S = userId } }
        });

        byte[] salt = userResponse.Item["Salt"].B.ToArray();

        // Generate hashed password using PBKDF2
        string hashedPassword = PasswordHasher.HashPassword(Input.CurrentPassword, salt);

        if (hashedPassword != userResponse.Item["Password"].S)
        {
            ModelState.AddModelError("Input.CurrentPassword", "Incorrect current password.");
            GenerateToaster(400);
            return Page();
        }

        byte[] newSalt = PasswordHasher.Salter();

        // Generate new hashed password using PBKDF2
        string newHashedPassword = PasswordHasher.HashPassword(Input.NewPassword, newSalt);

        var putItemRequest = new PutItemRequest
        {
            TableName = "Users",
            Item = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId } },
                { "Password", new AttributeValue { S = newHashedPassword } },
                { "Salt", new AttributeValue { B = new MemoryStream(newSalt) } }
            }
        };

        var response = await _dynamoDbClient.PutItemAsync(putItemRequest);
        if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Failed to update password.");
            GenerateToaster(500);
            return Page();
        }

        GenerateToaster(202);
        return RedirectToPage();
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        public string UserId { get; set; } 

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
