using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.DatabaseContext;
using SpendSmart.Pages.Dashboard.Content;
using Microsoft.AspNetCore.Authorization;
#nullable disable

namespace SpendSmart.Pages.Account._Manage;

[Authorize]
public class GenerateReportModel(IAmazonDynamoDB dynamoDbClient, DynamoDBContext context) : DynamoDbEndpoint<TransactionsEntity>(dynamoDbClient, context)
{
    public async Task<IActionResult> OnPost()
    {
        if (User.Identity.IsAuthenticated)
        {
            TempData["Report"] = true;
            var transactions = await GetItemsAsync("Transactions", User.Identity.Name);
            var previousMonthTransactions = transactions.Where(t => t.Date.Month == Month.AddMonths(-1).Month).ToList();
            Transactions = transactions.Where(t => t.Date.Month == Month.Month).ToList();

            Debit = Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount) + previousMonthTransactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
            Credit = Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount) + previousMonthTransactions.Where(t => t.Amount > 0).Sum(t => t.Amount);

            Accounts = await GetAccountsAsync(User.Identity.Name);
            Savings = Accounts.Where(a => a.Type == "Savings").ToList();
            Budgets = Accounts.Where(a => a.Type == "Budget").ToList();
        }
        return Page();
    }

    public DateTime Month { get; set; } = DateTime.Now;

    public List<AccountsEntity> Accounts { get; set; }
    public List<AccountsEntity> Savings { get; set; }
    public List<AccountsEntity> Budgets { get; set; }
    public List<TransactionsEntity> Transactions { get; set; }
    public decimal Debit { get; set; } = 0;
    public decimal Credit { get; set; } = 0;
}
