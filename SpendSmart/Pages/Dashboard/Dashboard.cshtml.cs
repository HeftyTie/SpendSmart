using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using SpendSmart.DatabaseContext;
using SpendSmart.Pages.Dashboard.Content;
using Microsoft.AspNetCore.Mvc;
#nullable disable

namespace SpendSmart.Pages.Dashboard;

public class DashboardModel(IAmazonDynamoDB dynamoDbClient, DynamoDBContext context) : DynamoDbEndpoint<TransactionsEntity>(dynamoDbClient, context)
{
	public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadUserFinancialDataAsync(DateTime.Now.ToString(), true);
        }
        catch (Exception)
        {
            GenerateToaster(500);
        }

        return Page();
	}

    public async Task<IActionResult> OnPost(string month)
    {
        try
        {
            await LoadUserFinancialDataAsync(month, false);
        }
        catch (Exception)
        {
            GenerateToaster(500);
        }

        return Page();
    }

    public List<TransactionsEntity> Transactions { get; set; } = MockData.Transactions;
    public List<AccountsEntity> Savings { get; set; } = MockData.Accounts.Where(a => a.Type == "Savings").ToList();
	public List<AccountsEntity> Budgets { get; set; } = MockData.Accounts.Where(a => a.Type == "Budget").ToList();

    public DateTime Month { get; set; } = DateTime.Now;
    public decimal Saving { get; set; } 
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance => Debit + Credit;
    public decimal IncomeChange { get; set; }
    public decimal ExpensesChange { get; set; }
    public decimal BalanceChange => IncomeChange + ExpensesChange;

    public List<FinancialMetric> FinancialMetrics =
    [
        new("Balance", "bank2 text-sexy-blue", 0m, "Increase since"),
        new("Credit", "graph-up-arrow text-success", 0m, "Increase since"),
        new("Debit", "graph-down-arrow text-danger", 0m, "Increase since")
    ];

    public List<FinancialCategories> FinancialCategories =
    [
        new("Savings", "piggy-bank-fill text-success"),
        new("Budgets", "wallet-fill text-warning")
    ];

    private async Task LoadUserFinancialDataAsync(string month, bool currentMonth)
    {
        if (User.Identity.IsAuthenticated)
        {
            var userId = User.Identity.Name;

            var accounts = await GetAccountsAsync(userId);
            Savings = currentMonth ? accounts.Where(a => a.Type == "Savings").ToList() : [];
            Budgets = currentMonth ? accounts.Where(a => a.Type == "Budget").ToList() : [];

            Month = DateTime.Parse(month);
            var transactions = await GetItemsAsync("Transactions", userId);
            Transactions = transactions.Where(t => t.Date.Month == Month.Month).ToList();
        }

        Debit = Transactions
            .Where(t => t.Amount < 0)
            .Sum(t => t.Amount);

        Credit = Transactions
            .Where(t => t.Amount > 0)
            .Sum(t => t.Amount);

        Saving = Savings.Sum(a => a.Balance);

        FinancialMetrics[0].Amount = Balance + Saving;
        FinancialMetrics[1].Amount = Credit;
        FinancialMetrics[2].Amount = Debit;
    }
}

public class FinancialMetric(string name, string cssClass, decimal amount, string description)
{
    public string Name { get; set; } = name;
    public string CssClass { get; set; } = cssClass;
    public decimal Amount { get; set; } = amount;
    public string Description { get; set; } = description;
}

public class FinancialCategories(string name, string cssClass)
{
    public string Name { get; set; } = name;
    public string CssClass { get; set; } = cssClass;
}