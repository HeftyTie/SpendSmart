using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.DatabaseContext;
using System.Security.Principal;

namespace SpendSmart.Pages.Dashboard.Content.Accounts;

public class AccountsModel(IAmazonDynamoDB dynamoDbClient, DynamoDBContext context) : DynamoDbEndpoint<AccountsEntity>(dynamoDbClient, context)
{
	public IActionResult OnGetAsync()
    {
		try
		{
            if (User.Identity.IsAuthenticated)
                Accounts = GetItemsAsync("Accounts", User.Identity.Name).Result;

            if (Accounts.Count != 0)
                ViewData["SearchIsEnabled"] = true;

			ViewData["DeleteText"] = "WARNING: Deleting an account will also delete budget/savings and all transactions associated with it. This action cannot be undone.";
		}
		catch (Exception)
		{
            GenerateToaster(500);
		}
        return Page();
    }

	public async Task<IActionResult> OnPostAddAsync(AccountsEntity newAccount)
	{
		try
		{
			if (!ModelState.IsValid)
            {
                GenerateToaster(400);
                return RedirectToPage();
            }
            else if (User.Identity.IsAuthenticated)
            {
                newAccount.UserId = User.Identity.Name;
                newAccount.Id = Guid.NewGuid().ToString();
				newAccount.TypeName = !string.IsNullOrEmpty(newAccount.TypeName) ? newAccount.TypeName : "None";
				if (newAccount.TypeName != "None" && newAccount.Goal == 0)
				{
					GenerateToaster(400);
					return RedirectToPage();
				}
				newAccount.Goal = FormatGoal(newAccount.Goal, newAccount.TypeName);

				await AddItemAsync("Accounts", newAccount);
				GenerateToaster(200);
			}
		}
        catch (Exception)
		{
			GenerateToaster(500);
		}

        return RedirectToPage();
    }

	public async Task<IActionResult> OnGetUpdateAsync(string id)
	{
        AccountsEntity updateAccount;
        if (User.Identity.IsAuthenticated)
        {
            var response = await GetItemAsync("Accounts", User.Identity.Name, id);
			var document = Document.FromAttributeMap(response.Item);
			updateAccount = _context.FromDocument<AccountsEntity>(document);
		}
        else
            updateAccount = new();

        return Partial("EditForm", updateAccount);
    }

	public async Task<IActionResult> OnPostUpdateAsync(AccountsEntity account)
	{
		try
		{
            if (User.Identity.IsAuthenticated)
            {
				account.Goal = account.Type == "Budget" || account.Type == "Savings" ? account.Goal : 0;
				await UpdateItemAsync("Accounts", account, account.UserId, account.Id);
				GenerateToaster(202);
			}
		}
		catch (Exception)
		{
			GenerateToaster(500);
		}

        return RedirectToPage();    
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
	{
		try
		{
            if (User.Identity.IsAuthenticated)
            {
                var accountResponse = await GetItemAsync("Accounts", User.Identity.Name, id);
				var document = Document.FromAttributeMap(accountResponse.Item);
				var account = _context.FromDocument<AccountsEntity>(document);
                await DeleteItemAsync("Accounts", User.Identity.Name, id);
				await DeleteTransactionItemsAsync(User.Identity.Name, account.Name);
                GenerateToaster(204);
			}
		}
        catch (Exception)
		{
            GenerateToaster(500);
        }

        return RedirectToPage();
    }

    [BindProperty]
	public List<AccountsEntity> Accounts { get; set; } = MockData.Accounts;

    [BindProperty]
	public AccountsEntity NewAccount { get; set; } = new AccountsEntity();

	private decimal FormatGoal(decimal goal, string typeName)
	{
		goal = Math.Abs(goal);
		if (typeName == "Savings")
			return goal;
		else
			return goal * -1;
	}
}
