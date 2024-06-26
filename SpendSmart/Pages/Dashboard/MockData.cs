using SpendSmart.Pages.Dashboard.Content;

namespace SpendSmart.Pages.Dashboard;

public static class MockData
{
    static MockData()
    {
        Random random = new();
        foreach (var transaction in Transactions)
        {
            transaction.Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, random.Next(1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
        }

        foreach (var account in Accounts)
        {
            decimal totalAmount = Transactions.Where(t => t.Account == account.Name).Sum(t => t.Amount);
            account.Balance = totalAmount;
        }
    }

    public static List<TransactionsEntity> Transactions { get; set; } =
    [
            new TransactionsEntity { Name = "AWS Bill", Amount = -15.46m, Account = "Checking" },
            new TransactionsEntity { Name = "Phone Bill", Amount = -110.67m, Account = "Checking" },
            new TransactionsEntity { Name = "Spotify", Amount = -6.68m, Account = "Credit Card" },
            new TransactionsEntity { Name = "Limited Time Figure", Amount = -52.93m, Account = "Credit Card" },
            new TransactionsEntity { Name = "Food Stamps", Amount = 205.00m, Account = "EBT" },
            new TransactionsEntity { Name = "Weekly Deposit", Amount = 20m, Account = "Savings" },
            new TransactionsEntity { Name = "Groceries", Amount = -150.75m, Account = "EBT" },
            new TransactionsEntity { Name = "Restaurant", Amount = -45.67m, Account = "Checking" },
            new TransactionsEntity { Name = "Gasoline", Amount = -35.12m, Account = "Savings" },
            new TransactionsEntity { Name = "Coffee", Amount = -4.25m, Account = "Checking" },
            new TransactionsEntity { Name = "Book Purchase", Amount = -17.99m, Account = "Savings" },
            new TransactionsEntity { Name = "Movie Tickets", Amount = -30.00m, Account = "Checking" },
            new TransactionsEntity { Name = "Birthday Cards", Amount = 200.00m, Account = "Savings" },
            new TransactionsEntity { Name = "Weekly Deposit", Amount = 70m, Account = "Savings" },
        ];

    public static List<AccountsEntity> Accounts { get; set; } =
    [
        new AccountsEntity { Name = "Checking", Type = "Budget", TypeName = "BPPR", Goal = -500m },
        new AccountsEntity { Name = "Savings", Type = "Savings", TypeName = "HYSA", Goal = 2000m },
        new AccountsEntity { Name = "Credit Card", Type = "None" },
        new AccountsEntity { Name = "EBT", Type = "None" }
    ];
}
