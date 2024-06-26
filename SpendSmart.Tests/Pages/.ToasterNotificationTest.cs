using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SpendSmart.Pages;

namespace SpendSmart.Tests.Pages;

public class ToasterNotificationTest
{
    private readonly Mock<ITempDataDictionary> _tempDataMock;
    private readonly ToasterNotification _toasterNotification;

    public ToasterNotificationTest()
    {
        _tempDataMock = new Mock<ITempDataDictionary>();
        _toasterNotification = new ToasterNotification
        {
            TempData = _tempDataMock.Object
        };
    }

    [Theory]
    [InlineData(200, "bg-success", "bi-check-circle", "Item saved successfully")]
    [InlineData(201, "bg-success", "bi-check-circle", "Sucessfully logged in")]
    [InlineData(2015, "bg-success", "bi-check-circle", "Sucessfully Registered")]
    [InlineData(202, "bg-success", "bi-check-circle", "Item updated successfully")]
    [InlineData(204, "bg-success", "bi-check-circle", "Item deleted successfully")]
    [InlineData(400, "bg-danger", "bi-exclamation-circle", "Invalid input")]
    [InlineData(401, "bg-danger", "bi-exclamation-circle", "Unauthorized")]
    [InlineData(404, "bg-warning", "bi-exclamation-triangle", "Item not found")]
    [InlineData(500, "bg-danger", "bi-exclamation-circle", "Internal server error")]
    [InlineData(501, "bg-danger", "bi-exclamation-circle", "No user found")]
    [InlineData(502, "bg-danger", "bi-exclamation-circle", "Username already exists")]
    public void GenerateToaster_SetsCorrectTempData_ForStatusCode(int statusCode, string expectedMessageClass, string expectedIconClass, string expectedMessage)
    {
        // Act
        _toasterNotification.GenerateToaster(statusCode);

        // Assert
        _tempDataMock.VerifySet(x => x["ToasterClass"] = expectedMessageClass, Times.Once);
        _tempDataMock.VerifySet(x => x["ToasterIconClass"] = expectedIconClass, Times.Once);
        _tempDataMock.VerifySet(x => x["ToasterMessage"] = expectedMessage, Times.Once);
    }
}
