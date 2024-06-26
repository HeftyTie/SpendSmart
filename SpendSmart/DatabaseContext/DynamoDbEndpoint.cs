using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using SpendSmart.Pages;
using SpendSmart.Pages.Dashboard.Content;

namespace SpendSmart.DatabaseContext;

public class DynamoDbEndpoint<T>(IAmazonDynamoDB dynamoDbClient, DynamoDBContext context) : ToasterNotification where T : class
{
    private readonly IAmazonDynamoDB _dynamoDbClient = dynamoDbClient;
    protected readonly DynamoDBContext _context = context;

	public async Task<GetItemResponse> GetItemAsync(string tableName, string userId, string itemId) =>
		   await _dynamoDbClient.GetItemAsync(tableName, new Dictionary<string, AttributeValue>
		   {
				{ "UserId", new AttributeValue { S = userId } },
				{ "Id", new AttributeValue { S = itemId } }
		   });

	public async Task<List<T>> GetItemsAsync(string tableName, string userId)
    {
        var request = new QueryRequest
        {
			TableName = tableName,
			ExpressionAttributeValues = new Dictionary<string, AttributeValue>
		    {
			    { ":userId", new AttributeValue { S = userId } }
		    },
			KeyConditionExpression = "UserId = :userId"
		};

        var response = await _dynamoDbClient.QueryAsync(request);

        var items = response.Items;
        var results = new List<T>();

        foreach (var item in items)
        {
            var document = Document.FromAttributeMap(item);
            var result = _context.FromDocument<T>(document);
            results.Add(result);
        }

        return results;
    }

	public async Task<List<AccountsEntity>> GetAccountsAsync(string userId)
	{
		var request = new QueryRequest
		{
			TableName = "Accounts",
			ExpressionAttributeValues = new Dictionary<string, AttributeValue>
			{
				{ ":userId", new AttributeValue { S = userId } }
			},
			KeyConditionExpression = "UserId = :userId"
		};

		var response = await _dynamoDbClient.QueryAsync(request);

		var items = response.Items;
		var results = new List<AccountsEntity>();

		foreach (var item in items)
		{
			var document = Document.FromAttributeMap(item);
			var result = _context.FromDocument<AccountsEntity>(document);
			results.Add(result);
		}

		return results;
	}

	public async Task AddItemAsync(string tableName, T item) {
        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = _context.ToDocument(item).ToAttributeMap(),
            ConditionExpression = "attribute_not_exists(Id)"
        };

        await _dynamoDbClient.PutItemAsync(request);
    }

#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
	public async Task UpdateItemAsync<T>(string tableName, T item, string userId, string id)
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type
	{
		// Convert item to attribute map and remove UserId and Id
		var attributeValues = _context.ToDocument(item).ToAttributeMap();
		attributeValues.Remove("UserId");
		attributeValues.Remove("Id");

		// Use aliases for keys that might be reserved keywords
		var expressionAttributeNames = new Dictionary<string, string>();
		var expressionAttributeValues = new Dictionary<string, AttributeValue>();
		var updateExpressions = new List<string>();

		foreach (var kvp in attributeValues)
		{
			string key = kvp.Key;
			string aliasKey = "#" + key;
			string valueKey = ":" + key;

			// Add to ExpressionAttributeNames
			expressionAttributeNames[aliasKey] = key;

			// Add to ExpressionAttributeValues
			expressionAttributeValues[valueKey] = kvp.Value;

			// Add to update expressions
			updateExpressions.Add($"{aliasKey} = {valueKey}");
		}

		var updateExpression = "SET " + string.Join(", ", updateExpressions);

		var request = new UpdateItemRequest
		{
			TableName = tableName,
			Key = new Dictionary<string, AttributeValue>
				{
					{ "UserId", new AttributeValue { S = userId } },
					{ "Id", new AttributeValue { S = id } }
				},
			ExpressionAttributeNames = expressionAttributeNames,
			ExpressionAttributeValues = expressionAttributeValues,
			ConditionExpression = "attribute_exists(Id)",
			UpdateExpression = updateExpression
		};

		await _dynamoDbClient.UpdateItemAsync(request);
	}

    public async Task DeleteItemAsync(string tableName, string userId, string id)
    {
		var request = new DeleteItemRequest
		{
			TableName = tableName,
			Key = new Dictionary<string, AttributeValue>
			{
				{ "UserId", new AttributeValue { S = userId } },
				{ "Id", new AttributeValue { S = id } }
			}
		};

		await _dynamoDbClient.DeleteItemAsync(request);
    }
	
    public async Task DeleteTransactionItemsAsync(string userId, string acccountName)
	{
		var request = new QueryRequest
		{
			TableName = "Transactions",
			ExpressionAttributeValues = new Dictionary<string, AttributeValue>
			{
				{ ":userId", new AttributeValue { S = userId } }
			},
			KeyConditionExpression = "UserId = :userId"
		};

        var response = await _dynamoDbClient.QueryAsync(request);
        var transactions = response.Items;

        foreach (var transaction in transactions)
        {
			if (transaction["Account"].S == acccountName)
			{
				var deleteRequest = new DeleteItemRequest
				{
					TableName = "Transactions",
					Key = new Dictionary<string, AttributeValue>
					{
						{ "UserId", new AttributeValue { S = userId } },
						{ "Id", transaction["Id"] }
					}
				};

				await _dynamoDbClient.DeleteItemAsync(deleteRequest);
			}
		}
	}
	public async Task<AccountsEntity> ClearExistingAccountAsync(string userId, string id)
	{
		var accountResponse = await GetItemAsync("Accounts", userId, id);
		var document = Document.FromAttributeMap(accountResponse.Item);
		var account = _context.FromDocument<AccountsEntity>(document);

		account.Type = "None";
		account.TypeName = "None";
		account.Goal = 0;

		return account;
	}

	public AccountsEntity ClearExistingAccount(AccountsEntity account)
	{
		account.Type = "None";
		account.TypeName = "None";
		account.Goal = 0;

		return account;
	}
}
