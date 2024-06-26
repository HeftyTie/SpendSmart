using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SpendSmart.Pages.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable disable

namespace SpendSmart.Tests.Pages.Account;

public class LoginModelTest
{
    [Fact]
    public void OnGet_OnGet_UnauthenticatedUser_ReturnsPage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginModel>>();
        var httpContext = new DefaultHttpContext();

        var loginModel = new LoginModel(null, mockLogger.Object)
        {
            PageContext = new PageContext(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor())),
            Url = new UrlHelper(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor()))
        };

        // Act
        var result = loginModel.OnGet();

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_InvalidModelState_ReturnsPage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginModel>>();
        var loginModel = new LoginModel(null, mockLogger.Object);
        loginModel.ModelState.AddModelError("Test", "TestError");

        // Act
        var result = await loginModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_InvalidPassword_AddsModelError()
    {
        // Arrange
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<LoginModel>>();
        var mockTempData = new Mock<ITempDataDictionary>();

        var inputModel = new LoginModel.InputModel { Username = "validUser", Password = "incorrectPassword" };
        var base64Salt = "base64Salt=="; // Example Base64 salt
        byte[] byteArray = Convert.FromBase64String(base64Salt);
        // Simulate a different password stored in the database
        var hashedPassword = PasswordHasher.HashPassword("correctPassword", byteArray);

        var getItemResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = inputModel.Username } },
                { "Salt", new AttributeValue { B = new MemoryStream(byteArray) } },
                { "Password", new AttributeValue { S = hashedPassword } }
            }
        };

        mockDynamoDbClient.Setup(client => client.GetItemAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, AttributeValue>>(), default))
                          .ReturnsAsync(getItemResponse);

        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = routeData,
            ActionDescriptor = actionDescriptor
        };

        var loginModel = new LoginModel(mockDynamoDbClient.Object, mockLogger.Object)
        {
            Input = inputModel,
            PageContext = new PageContext(actionContext),
            TempData = mockTempData.Object
        };

        // Act
        var result = await loginModel.OnPostAsync();

        // Assert
        var pageResult = Assert.IsType<PageResult>(result);
        var modelState = loginModel.ModelState;
        Assert.True(modelState.ContainsKey("Input.Password"));
        Assert.Equal("Incorrect password.", modelState["Input.Password"].Errors.First().ErrorMessage);
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Invalid input", Times.Once);
    }

    [Fact]
	public async Task OnPostAsync_UnregisteredUsername_AddsModelError()
	{
		// Arrange
		var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
		var mockLogger = new Mock<ILogger<LoginModel>>();
		var inputModel = new LoginModel.InputModel { Username = "unregisteredUser", Password = "anyPassword" };
        var mockTempData = new Mock<ITempDataDictionary>();

        var getItemResponse = new GetItemResponse
		{
			Item = [] // Empty dictionary to simulate user not found
		};

		mockDynamoDbClient.Setup(client => client.GetItemAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, AttributeValue>>(), default))
						  .ReturnsAsync(getItemResponse);

		var loginModel = new LoginModel(mockDynamoDbClient.Object, mockLogger.Object)
		{
			Input = inputModel,
            TempData = mockTempData.Object
		};

		// Act
		var result = await loginModel.OnPostAsync();

		// Assert
		Assert.IsType<PageResult>(result);
		Assert.True(loginModel.ModelState.ContainsKey("Input.Username"));
		var modelStateEntry = loginModel.ModelState["Input.Username"];
		Assert.Single(modelStateEntry.Errors);
		Assert.Equal("Username not registered.", modelStateEntry.Errors[0].ErrorMessage);
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "No user found", Times.Once);
	}
}
