using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Moq;
using SpendSmart.DatabaseContext;
using System.Transactions;

namespace SpendSmart.Tests.DatabaseContext.DynamoDbEndpoint;

public class DeleteTransactionItemsTest
{
	private readonly Mock<IAmazonDynamoDB> _dynamoDbMock;
	private readonly DynamoDBContext _context; // Assuming you have a way to mock or instantiate this for tests
	private readonly DynamoDbEndpoint<Transaction> _dynamoDbEndpoint;

	public DeleteTransactionItemsTest()
	{
		_dynamoDbMock = new Mock<IAmazonDynamoDB>();
		_context = new DynamoDBContext(_dynamoDbMock.Object); // Simplified for example purposes
		_dynamoDbEndpoint = new DynamoDbEndpoint<Transaction>(_dynamoDbMock.Object, _context);
	}

	[Fact]
	public async void DeleteTransactionItems_DeletesMatchingTransactions()
	{
		// Arrange
		var userId = "testUser";
		var accountName = "testAccount";
		var transactions = new List<Dictionary<string, AttributeValue>>
		{
			new() {
				{ "UserId", new AttributeValue { S = userId } },
				{ "Id", new AttributeValue { S = "1" } },
				{ "Account", new AttributeValue { S = accountName } }
			}
		};

		_dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new QueryResponse { Items = transactions });

		_dynamoDbMock.Setup(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new DeleteItemResponse());

		// Act
		await _dynamoDbEndpoint.DeleteTransactionItemsAsync(userId, accountName);

		// Assert
		_dynamoDbMock.Verify(x => x.DeleteItemAsync(It.Is<DeleteItemRequest>(r => r.TableName == "Transactions" && r.Key["UserId"].S == userId && r.Key["Id"].S == "1"), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async void DeleteTransactionItems_DoesNotDeleteNonMatchingTransactions()
	{
		// Arrange
		var userId = "testUser";
		var accountName = "testAccount";
		var transactions = new List<Dictionary<string, AttributeValue>>
		{
			new() {
				{ "UserId", new AttributeValue { S = userId } },
				{ "Id", new AttributeValue { S = "1" } },
				{ "Account", new AttributeValue { S = "differentAccount" } }
			}
		};

		_dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new QueryResponse { Items = transactions });

		// Act
		await _dynamoDbEndpoint.DeleteTransactionItemsAsync(userId, accountName);

		// Assert
		_dynamoDbMock.Verify(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}
