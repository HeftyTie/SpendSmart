using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
#nullable disable

namespace SpendSmart.Pages.Account._Manage;

public class _ManageNav : PageModel
{
	public static string Index => "Index";
	public static string ChangePassword => "ChangePassword";
	public static string GenerateReport => "GenerateReport";

	public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
	public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);
	public static string GenerateReportNavClass(ViewContext viewContext) => PageNavClass(viewContext, GenerateReport);

	public static string PageNavClass(ViewContext viewContext, string page)
	{
		var activePage = viewContext.ViewData["ActivePage"] as string
			?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
		return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
	}
}
