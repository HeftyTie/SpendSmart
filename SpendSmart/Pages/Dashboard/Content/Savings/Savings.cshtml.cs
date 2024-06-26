using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.DatabaseContext;
using Amazon.DynamoDBv2.DocumentModel;
#nullable disable

namespace SpendSmart.Pages.Dashboard.Content.Savings;

public class SavingsModel(IAmazonDynamoDB dynamoDbClient, DynamoDBContext context) : DynamoDbEndpoint<AccountsEntity>(dynamoDbClient, context)
{
	public IActionResult OnGetAsync()
	{
		try
		{
			if (!User.Identity.IsAuthenticated)
				TempData["Accounts"] = MockData.Accounts.Where(a => a.Type == "Savings");
			else
				LoadSavingsAccounts();

			if (Savings.Count != 0)
				ViewData["SearchIsEnabled"] = true;
		}
		catch (Exception)
		{
			GenerateToaster(500);
		}

		return Page();
	}

	public async Task<IActionResult> OnPostAddAsync()
	{
		try
		{
			if (!ModelState.IsValid || NewSavings.Goal == 0)
			{
				GenerateToaster(400);
				return RedirectToPage();
			}
			else if (User.Identity.IsAuthenticated)
			{
				var accounts = GetItemsAsync("Accounts", User.Identity.Name).Result;
				NewSavings.Id = accounts.Where(a => a.Name == NewSavings.Name).Select(a => a.Id).FirstOrDefault();
				NewSavings.UserId = User.Identity.Name;
				NewSavings.Type = "Savings";
				NewSavings.Balance = accounts.Where(a => a.Name == NewSavings.Name).Select(a => a.Balance).FirstOrDefault();
				NewSavings.Goal = NewSavings.Goal > 0 ? NewSavings.Goal : -NewSavings.Goal;
				await UpdateItemAsync("Accounts", NewSavings, NewSavings.UserId, NewSavings.Id);
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
			LoadSavingsAccounts();
			var response = await GetItemAsync("Accounts", User.Identity.Name, id);
			var document = Document.FromAttributeMap(response.Item);
			updateAccount = _context.FromDocument<AccountsEntity>(document);
		}
		else
		{
			TempData["Accounts"] = MockData.Accounts.Where(a => a.Type == "Savings");
			updateAccount = new();
		}

		return Partial("EditForm", updateAccount);
	}

	public async Task<IActionResult> OnPostUpdateAsync(AccountsEntity savings)
	{
		try
		{
			if (User.Identity.IsAuthenticated)
			{
				var accounts = GetItemsAsync("Accounts", savings.UserId).Result;
				var account = accounts.Where(a => a.Id == savings.Id).FirstOrDefault();
				var currentAccount = ClearExistingAccount(account);
				await UpdateItemAsync("Accounts", currentAccount, savings.UserId, currentAccount.Id);

				savings.Id = accounts.Where(a => a.Name == savings.Name).Select(a => a.Id).FirstOrDefault();
				savings.Type = "Savings";

				await UpdateItemAsync("Accounts", savings, savings.UserId, savings.Id);
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
				var userId = User.Identity.Name;
				var account = await ClearExistingAccountAsync(userId, id);
				await UpdateItemAsync("Accounts", account, userId, account.Id);
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
	public List<AccountsEntity> Savings { get; set; } = MockData.Accounts.Where(a => a.Type == "Savings").ToList();

	[BindProperty]
	public AccountsEntity NewSavings { get; set; } = new AccountsEntity();

	private void LoadSavingsAccounts()
	{
		var accounts = GetItemsAsync("Accounts", User.Identity.Name).Result;
		Savings = accounts.Where(a => a.Type == "Savings").ToList();
		var selectableAccounts = accounts.Where(a => a.Type == "None").ToList();
		TempData["Accounts"] = selectableAccounts.Select(a => a.Name).ToList();
	}
}
