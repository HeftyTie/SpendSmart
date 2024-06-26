using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SpendSmart.Pages.Account;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace SpendSmart.Tests.Pages.Account;

public class RegisterModelTests
{
    [Fact]
    public void OnGet_UnauthenticatedUser_ReturnsPage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RegisterModel>>();
        var httpContext = new DefaultHttpContext(); 

        var registerModel = new RegisterModel(null, mockLogger.Object)
        {
            PageContext = new PageContext(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor())),
            Url = new UrlHelper(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor()))
        };

        // Act
        var result = registerModel.OnGet();

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task RegisterUser_Success()
    {
        // Arrange
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<RegisterModel>>();
        var mockTempData = new Mock<ITempDataDictionary>();

        var inputModel = new RegisterModel.InputModel { Username = "testUser", Password = "testPass", ConfirmPassword = "testPass" };
        mockDynamoDbClient.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                          .ReturnsAsync(new PutItemResponse { HttpStatusCode = HttpStatusCode.OK });

        var registerModel = new RegisterModel(mockDynamoDbClient.Object, mockLogger.Object) 
        {
            Input = inputModel,
            TempData = mockTempData.Object
        };

        // Act
        var result = await registerModel.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        mockDynamoDbClient.Verify(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default), Times.Once);
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Sucessfully Registered", Times.Once);
    }

    [Fact]
    public async Task RegisterUser_PasswordMismatch()
    {
        var mockTempData = new Mock<ITempDataDictionary>();

        // Arrange
        var registerModel = new RegisterModel(null, null)
        {
            Input = new RegisterModel.InputModel { Username = "testUser", Password = "testPass", ConfirmPassword = "mismatch" },
            TempData = mockTempData.Object
        };

        // Act
        var result = await registerModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(registerModel.ModelState.ErrorCount > 0);
        Assert.Contains(registerModel.ModelState.Keys, key => key == "Input.Password");
        Assert.Contains(registerModel.ModelState.Values.SelectMany(v => v.Errors), error => error.ErrorMessage == "Passwords do not match.");
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Invalid input", Times.Once);
    }

    [Fact]
    public async Task RegisterUser_Failed()
    {
        // Arrange
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<RegisterModel>>();
        var mockTempData = new Mock<ITempDataDictionary>();

        var inputModel = new RegisterModel.InputModel { Username = "testUser", Password = "testPass", ConfirmPassword = "testPass" };

        mockDynamoDbClient.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                          .ReturnsAsync(new PutItemResponse { HttpStatusCode = HttpStatusCode.InternalServerError });

        var registerModel = new RegisterModel(mockDynamoDbClient.Object, mockLogger.Object)
        {
            Input = inputModel,
            TempData = mockTempData.Object
        };

        // Act
        var result = await registerModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        mockDynamoDbClient.Verify(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default), Times.Once);
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Internal server error", Times.Once);
    }

    [Fact]
    public async Task RegisterUser_UsernameIsTaken()
    {
        // Arrange
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<RegisterModel>>();
        var mockTempData = new Mock<ITempDataDictionary>();

        var inputModel = new RegisterModel.InputModel { Username = "testUser", Password = "testPass", ConfirmPassword = "testPass" };

        var user = new Dictionary<string, AttributeValue>
        {
            { "UserId", new AttributeValue { S = "testUser" } }
        };

        mockDynamoDbClient.Setup(client => client.GetItemAsync("Users", It.IsAny<Dictionary<string, AttributeValue>>(), default))
                          .ReturnsAsync(new GetItemResponse { Item = user });

        var registerModel = new RegisterModel(mockDynamoDbClient.Object, mockLogger.Object)
        {
            Input = inputModel,
            TempData = mockTempData.Object
        };

        // Act
        var result = await registerModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(registerModel.ModelState.ErrorCount > 0);
        Assert.Contains(registerModel.ModelState.Keys, key => key == "Input.Username");
        Assert.Contains(registerModel.ModelState.Values.SelectMany(v => v.Errors), error => error.ErrorMessage == "Username already exists.");
        mockTempData.VerifySet(tempData => tempData["ToasterMessage"] = "Username already exists", Times.Once);
    }
}
