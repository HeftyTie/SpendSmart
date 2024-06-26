using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.DatabaseContext;

#nullable disable

namespace SpendSmart.Pages.Dashboard.Content.Budgets;

public class BudgetsModel(IAmazonDynamoDB dynamoDbClient, DynamoDBContext context) : DynamoDbEndpoint<AccountsEntity>(dynamoDbClient, context)
{
	public IActionResult OnGetAsync()
	{
		try
		{
			if (!User.Identity.IsAuthenticated)
				TempData["Accounts"] = MockData.Accounts.Where(a => a.Type == "Budget");
			else
				LoadBudgetAccounts();

			if (Budgets.Count > 0)
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
			if (!ModelState.IsValid || NewBudget.Goal == 0)
			{
				GenerateToaster(400);
				return RedirectToPage();
			}
			else if (User.Identity.IsAuthenticated)
			{
				var accounts = GetItemsAsync("Accounts", User.Identity.Name).Result;
				NewBudget.Id = accounts.Where(a => a.Name == NewBudget.Name).Select(a => a.Id).FirstOrDefault();
				NewBudget.UserId = User.Identity.Name;
				NewBudget.Type = "Budget";
				NewBudget.Balance = accounts.Where(a => a.Name == NewBudget.Name).Select(a => a.Balance).FirstOrDefault();
				NewBudget.Goal = NewBudget.Goal > 0 ? -NewBudget.Goal : NewBudget.Goal;
				await UpdateItemAsync("Accounts", NewBudget, NewBudget.UserId, NewBudget.Id);
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
			LoadBudgetAccounts();
			var response = await GetItemAsync("Accounts", User.Identity.Name, id);
			var document = Document.FromAttributeMap(response.Item);
			updateAccount = _context.FromDocument<AccountsEntity>(document);
		}
		else
		{
			TempData["Accounts"] = MockData.Accounts.Where(a => a.Type == "Budget");
			updateAccount = new();
		}

		return Partial("EditForm", updateAccount);
	}

	public async Task<IActionResult> OnPostUpdateAsync(AccountsEntity budget)
	{
		try
		{
			if (User.Identity.IsAuthenticated)
			{
				var accounts = GetItemsAsync("Accounts", budget.UserId).Result;
				var account = accounts.Where(a => a.Id == budget.Id).FirstOrDefault();
				var currentAccount = ClearExistingAccount(account);
				await UpdateItemAsync("Accounts", currentAccount, budget.UserId, currentAccount.Id);

				budget.Id = accounts.Where(a => a.Name == budget.Name).Select(a => a.Id).FirstOrDefault();	
				budget.Type = "Budget";

				await UpdateItemAsync("Accounts", budget, budget.UserId, budget.Id);
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
	public List<AccountsEntity> Budgets { get; set; } = MockData.Accounts.Where(a => a.Type == "Budget").ToList();

	[BindProperty]
	public AccountsEntity NewBudget { get; set; } = new AccountsEntity();

	private void LoadBudgetAccounts()
	{
		var accounts = GetItemsAsync("Accounts", User.Identity.Name).Result;
		Budgets = accounts.Where(a => a.Type == "Budget").ToList();
		var selectableAccounts = accounts.Where(a => a.Type == "None").ToList();
		TempData["Accounts"] = selectableAccounts.Select(a => a.Name).ToList();
	}
}
