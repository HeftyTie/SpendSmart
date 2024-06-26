using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace SpendSmart.Pages.Dashboard.Content;

public class BaseEntity
{
    [DynamoDBHashKey]
    public string UserId { get; set; }
    [DynamoDBRangeKey]
    public string Id { get; set; }
	[Required]
	public string Name { get; set; }
}

[DynamoDBTable("Accounts")]
public class AccountsEntity : BaseEntity
{
	public decimal Balance { get; set; }
	public string Type { get; set; }
    public string TypeName { get; set; }
    public decimal Goal { get; set; }
}

[DynamoDBTable("Transactions")]
public class TransactionsEntity : BaseEntity
{
	public DateTime Date { get; set; }
	public string Account { get; set; }
	public decimal Amount { get; set; }
}