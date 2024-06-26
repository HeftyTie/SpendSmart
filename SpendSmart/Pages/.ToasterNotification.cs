using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpendSmart.Pages;

public class ToasterNotification : PageModel
{
    public void GenerateToaster(int statusCode)
    {
        var message = Messages.FirstOrDefault(m => m.StatusCode == statusCode);

        TempData["ToasterClass"] = message.MessageClass;
        TempData["ToasterIconClass"] = message.IconClass;
        TempData["ToasterMessage"] = message.Message;
    }

    private static readonly List<(int StatusCode, string MessageClass, string IconClass, string Message)> Messages =
    [
        (200, "bg-success", "bi-check-circle", "Item saved successfully"),
        (201, "bg-success", "bi-check-circle", "Sucessfully logged in"),
        (2015, "bg-success", "bi-check-circle", "Sucessfully Registered"),
        (2016, "bg-success", "bi-check-circle", "Sucessfully logged out"),
        (202, "bg-success", "bi-check-circle", "Item updated successfully"),
        (204, "bg-success", "bi-check-circle", "Item deleted successfully"),
        (400, "bg-danger", "bi-exclamation-circle", "Invalid input"),
        (401, "bg-danger", "bi-exclamation-circle", "Unauthorized"),
        (404, "bg-warning", "bi-exclamation-triangle", "Item not found"),
        (500, "bg-danger", "bi-exclamation-circle", "Internal server error"),
        (501, "bg-danger", "bi-exclamation-circle", "No user found"),
        (502, "bg-danger", "bi-exclamation-circle", "Username already exists")
    ];
}
