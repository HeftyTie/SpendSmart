using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.DatabaseContext;
using System.Transactions;
using Amazon.DynamoDBv2.DocumentModel;
#nullable disable

namespace SpendSmart.Pages.Dashboard.Content.Transactions;

public class TransactionsModel(IAmazonDynamoDB dynamoDbClient, DynamoDBContext context) : DynamoDbEndpoint<TransactionsEntity>(dynamoDbClient, context)
{
	public IActionResult OnGetAsync()
	{
		try
		{
			if (!User.Identity.IsAuthenticated)
				TempData["Accounts"] = MockData.Accounts;
			else
				LoadTransactions();

			Transactions = [.. Transactions.OrderByDescending(t => t.Date)];
			if (Transactions.Count != 0)
				ViewData["SearchIsEnabled"] = true;
		}
		catch (Exception)
		{

			GenerateToaster(500);
		}

		return Page();
	}

	public IActionResult OnPost(string month)
    {
        try
        {
			Month = DateTime.Parse(month);
            if (User.Identity.IsAuthenticated)
            {
                LoadTransactions();
                Transactions = [.. Transactions.OrderByDescending(t => t.Date)];
                if (Transactions.Count != 0)
                    ViewData["SearchIsEnabled"] = true;
            }
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
			if (!ModelState.IsValid || NewTransaction.Amount == 0)
			{
				GenerateToaster(400);
				return RedirectToPage();
			}
			else if (User.Identity.IsAuthenticated)
			{
				var accounts = GetAccountsAsync(User.Identity.Name).Result;
				var accountToUpdate = accounts.Where(a => a.Name == NewTransaction.Account).FirstOrDefault();
				accountToUpdate.Balance += NewTransaction.Amount;
				await UpdateItemAsync("Accounts", accountToUpdate, accountToUpdate.UserId, accountToUpdate.Id);

				NewTransaction.Id = Guid.NewGuid().ToString();
				NewTransaction.UserId = User.Identity.Name;
				await AddItemAsync("Transactions", NewTransaction);
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
		TransactionsEntity updateTransaction;
		if (User.Identity.IsAuthenticated)
		{
			LoadTransactions();
			var response = await GetItemAsync("Transactions", User.Identity.Name, id);
			var document = Document.FromAttributeMap(response.Item);
			updateTransaction = _context.FromDocument<TransactionsEntity>(document);
		}
		else
		{
			TempData["Accounts"] = MockData.Accounts;
			updateTransaction = new TransactionsEntity();
		}

		return Partial("EditForm", updateTransaction);
	}

	public async Task<IActionResult> OnPostUpdateAsync(TransactionsEntity transaction)
	{
		try
		{
			if (User.Identity.IsAuthenticated)
			{
				var accounts = GetAccountsAsync(transaction.UserId).Result;
				var transactionResponse = await GetItemAsync("Transactions", transaction.UserId, transaction.Id);
				var document = Document.FromAttributeMap(transactionResponse.Item);
				var oldTransaction = _context.FromDocument<TransactionsEntity>(document);

				decimal oldAmount = oldTransaction.Amount; 
				decimal newAmount = transaction.Amount; 
				decimal change = newAmount - oldAmount;

				var account = accounts.FirstOrDefault(a => a.Name == transaction.Account);
				account.Balance += change;
				await UpdateItemAsync("Accounts", account, account.UserId, account.Id);

				await UpdateItemAsync("Transactions", transaction, transaction.UserId, transaction.Id);
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
				var transaction = await GetItemAsync("Transactions", userId, id);
				var document = Document.FromAttributeMap(transaction.Item);
				var transactionToDelete = _context.FromDocument<TransactionsEntity>(document);

				var accounts = GetAccountsAsync(userId).Result;
				var account = accounts.FirstOrDefault(a => a.Name == transactionToDelete.Account);
				account.Balance -= transactionToDelete.Amount;
				await UpdateItemAsync("Accounts", account, account.UserId, account.Id);

				await DeleteItemAsync("Transactions", userId, id);

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
	public List<TransactionsEntity> Transactions { get; set; } = MockData.Transactions;

	[BindProperty]
	public TransactionsEntity NewTransaction { get; set; } = new TransactionsEntity()
	{
		Date = DateTime.Now
	};

	public DateTime Month { get; set; } = DateTime.Now;

	private void LoadTransactions()
	{
		var transactions = GetItemsAsync("Transactions", User.Identity.Name).Result;
        var currentMonth = Month.Month;
		Transactions = transactions.Where(t => t.Date.Month == currentMonth).ToList();
		TempData["Accounts"] = GetAccountsAsync(User.Identity.Name).Result;
	}
}
