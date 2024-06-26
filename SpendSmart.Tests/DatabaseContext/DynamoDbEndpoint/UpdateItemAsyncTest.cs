using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Moq;
using SpendSmart.DatabaseContext;
using Amazon.DynamoDBv2.Model;

namespace SpendSmart.Tests.DatabaseContext.DynamoDbEndpoint;

public class UpdateItemAsyncTest
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDbContext;
    private readonly Mock<DynamoDBContext> _context;
    private readonly DynamoDbEndpoint<SampleItem> _dynamoDbEndpoint;

    public UpdateItemAsyncTest()
    {
        _mockDynamoDbContext = new Mock<IAmazonDynamoDB>();
        _context = new Mock<DynamoDBContext>(_mockDynamoDbContext.Object);
        _dynamoDbEndpoint = new DynamoDbEndpoint<SampleItem>(_mockDynamoDbContext.Object, _context.Object);
    }

    [Fact]
    public async Task UpdateItemAsync_Success()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user1";
        var itemId = "item1";

        var sampleItem = new SampleItem
        {
            Id = itemId,
            UserId = userId,
            Balance = 100.0m
        };

        _mockDynamoDbContext.Setup(client => client.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default))
                            .ReturnsAsync(new UpdateItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        await _dynamoDbEndpoint.UpdateItemAsync(tableName, sampleItem, userId, itemId);

        // Assert
        _mockDynamoDbContext.Verify(
            client => client.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateItemAsync_WithNonExistentItems()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user1";
        var itemId = "item1";

        var sampleItem = new SampleItem
        {
            Id = itemId,
            UserId = userId,
            Balance = 100.0m
        };

        _mockDynamoDbContext.Setup(client => client.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default))
                            .ReturnsAsync(new UpdateItemResponse { HttpStatusCode = System.Net.HttpStatusCode.BadRequest });

        // Act
        await _dynamoDbEndpoint.UpdateItemAsync(tableName, sampleItem, userId, itemId);

        // Assert
        _mockDynamoDbContext.Verify(
            client => client.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default),
            Times.Once
        );
    }

	[Fact]
	public async Task UpdateItemAsync_WithDynamoDbException()
	{
		// Arrange
		var tableName = "TestTable";
		var userId = "user1";
		var itemId = "item1";

		var sampleItem = new SampleItem
		{
			Id = itemId,
			UserId = userId,
			Balance = 100.0m
		};

		_mockDynamoDbContext.Setup(client => client.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default))
						   .ThrowsAsync(new AmazonDynamoDBException("An error occurred"));

		// Act
		var exception = await Xunit.Record.ExceptionAsync(() => _dynamoDbEndpoint.UpdateItemAsync(tableName, sampleItem, userId, itemId));

		// Assert
		Assert.NotNull(exception);
		Assert.IsType<AmazonDynamoDBException>(exception);
		_mockDynamoDbContext.Verify(
			client => client.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default),
			Times.Once
		);
	}
}
