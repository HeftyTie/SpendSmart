using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using SpendSmart.Pages.Account._Manage;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SpendSmart.Pages.Account;
using Microsoft.AspNetCore.Mvc;
#nullable disable

namespace SpendSmart.Tests.Pages.Account._Manage;

public class ChangePasswordTest
{
    [Fact]
    public async Task OnPostAsync_InvalidModelState_ReturnsPage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChangePasswordModel>>();
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockTempData = new Mock<ITempDataDictionary>();

        var inputModel = new ChangePasswordModel.InputModel
        {
            CurrentPassword = "incorrectPassword",
            NewPassword = "newPassword",
            ConfirmPassword = "newPassword"
        };

        var changePasswordModel = new ChangePasswordModel(mockDynamoDbClient.Object, mockLogger.Object)
        {
            Input = inputModel,
            TempData = mockTempData.Object 
        };

        changePasswordModel.ModelState.AddModelError("Test", "TestError");

        // Act
        var result = await changePasswordModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Invalid input", Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_InvalidCurrentPassword_AddsModelError()
    {
        // Arrange
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<ChangePasswordModel>>();
        var mockTempData = new Mock<ITempDataDictionary>();

        var userId = "validUser";
        var base64Salt = "base64Salt=="; // Example Base64 salt
        var hashedPassword = PasswordHasher.HashPassword("correctPassword", Convert.FromBase64String(base64Salt)); // Example hashed password

        var inputModel = new ChangePasswordModel.InputModel
        {
            CurrentPassword = "incorrectPassword",
            NewPassword = "newPassword",
            ConfirmPassword = "newPassword"
        };

        var getItemResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId } },
                { "Salt", new AttributeValue { B = new MemoryStream(Encoding.UTF8.GetBytes(base64Salt)) } },
                { "Password", new AttributeValue { S = hashedPassword } }
            }
        };

        mockDynamoDbClient.Setup(client => client.GetItemAsync("Users", It.IsAny<Dictionary<string, AttributeValue>>(), default))
                          .ReturnsAsync(getItemResponse);

        var changePasswordModel = new ChangePasswordModel(mockDynamoDbClient.Object, mockLogger.Object)
        {
            Input = inputModel,
            TempData = mockTempData.Object
        };

        // Act
        var result = await changePasswordModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.False(changePasswordModel.ModelState.IsValid); 
        Assert.True(changePasswordModel.ModelState.ContainsKey("Input.CurrentPassword")); 
        Assert.Equal("Incorrect current password.", changePasswordModel.ModelState["Input.CurrentPassword"].Errors.First().ErrorMessage);
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Invalid input", Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_Successfull_PasswordChange()
    {
        // Arrange
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<ChangePasswordModel>>();
        var mockTempData = new Mock<ITempDataDictionary>();

        var inputModel = new ChangePasswordModel.InputModel { CurrentPassword = "correctPassword", NewPassword = "newPassword", ConfirmPassword = "newPassword" };
        var userId = "validUser";
        var base64Salt = "base64Salt=="; // Example Base64 salt
        byte[] byteArray = Convert.FromBase64String(base64Salt);
        var hashedPassword = PasswordHasher.HashPassword("correctPassword", byteArray); 

        var getItemResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId } },
                { "Salt", new AttributeValue { B = new MemoryStream(byteArray) } },
                { "Password", new AttributeValue { S = hashedPassword } }
            }
        };

        mockDynamoDbClient.Setup(client => client.GetItemAsync("Users", It.IsAny<Dictionary<string, AttributeValue>>(), default)).ReturnsAsync(getItemResponse);

        var putItemResponse = new PutItemResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK
        };

        mockDynamoDbClient.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default)).ReturnsAsync(putItemResponse);

        var changePasswordModel = new ChangePasswordModel(mockDynamoDbClient.Object, mockLogger.Object)
        {
            Input = inputModel,
            TempData = mockTempData.Object
        };

        // Act
        var result = await changePasswordModel.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        Assert.True(changePasswordModel.ModelState.IsValid);
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Item updated successfully", Times.Once);
    }
}
