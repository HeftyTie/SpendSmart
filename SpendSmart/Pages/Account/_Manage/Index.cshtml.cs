using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
#nullable disable

namespace SpendSmart.Pages.Account._Manage;

[Authorize]
public class IndexModel : PageModel
{
	[BindProperty]
	public string Username { get; set; }

	public void OnGetAsync()
    {
        Username = User.Identity.Name;
    }
}
