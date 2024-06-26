using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SpendSmart.Pages.Account;
using System.Security.Claims;

namespace SpendSmart.Tests.Pages.Account;

public class LogoutModelTest
{
    [Fact]
    public async Task OnGet_RedirectsToReferer_IfExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LogoutModel>>();
        var httpContext = new DefaultHttpContext();
        var mockTempData = new Mock<ITempDataDictionary>();
        httpContext.Request.Headers.Referer = "http://localhost:433/previous-page";
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthentication"));

        // Set up authentication services
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        httpContext.RequestServices = serviceProvider;

        var logoutModel = new LogoutModel(mockLogger.Object)
        {
            PageContext = new PageContext(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor())),
            Url = new UrlHelper(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor())),
            TempData = mockTempData.Object
        };

        // Act
        var result = await logoutModel.OnGet();

        // Assert
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("~/", redirectResult.Url);
    }

    [Fact]
    public async Task OnGet_RedirectsToRoot_IfRefererIsMissing()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LogoutModel>>();
        var mockTempData = new Mock<ITempDataDictionary>();

        Claim[] claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"))
        };

        // Set up authentication services
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        httpContext.RequestServices = serviceProvider;

        var logoutModel = new LogoutModel(mockLogger.Object)
        {
            PageContext = new PageContext(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor())),
            Url = new UrlHelper(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor())),
            TempData = mockTempData.Object
        };

        // Act
        var result = await logoutModel.OnGet();

        // Assert
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("~/", redirectResult.Url);
    }
}