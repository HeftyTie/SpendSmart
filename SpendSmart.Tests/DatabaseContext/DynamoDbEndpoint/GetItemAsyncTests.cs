using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Moq;
using SpendSmart.DatabaseContext;

namespace SpendSmart.Tests.DatabaseContext.DynamoDbEndpoint;

public class GetItemAsyncTests
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDbContext;
    private readonly Mock<DynamoDBContext> _context;
    private readonly DynamoDbEndpoint<SampleItem> _dynamoDbEndpoint;

    public GetItemAsyncTests()
    {
        _mockDynamoDbContext = new Mock<IAmazonDynamoDB>();
        _context = new Mock<DynamoDBContext>(_mockDynamoDbContext.Object);
        _dynamoDbEndpoint = new DynamoDbEndpoint<SampleItem>(_mockDynamoDbContext.Object, _context.Object);
    }

    [Fact]
    public async Task GetItemAsync_Success()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user1";
        var itemId = "item1";
        var mockResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId } },
                { "Id", new AttributeValue { S = itemId } }
            }
        };
        _mockDynamoDbContext.Setup(x => x.GetItemAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, AttributeValue>>(), default))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _dynamoDbEndpoint.GetItemAsync(tableName, userId, itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Item["UserId"].S);
        Assert.Equal(itemId, result.Item["Id"].S);
    }

    [Fact]
    public async Task GetItemAsync_WithNonExistentItem()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user2";
        var itemId = "item2";
        _mockDynamoDbContext.Setup(x => x.GetItemAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, AttributeValue>>(), default))
            .ReturnsAsync(new GetItemResponse());

        // Act
        var result = await _dynamoDbEndpoint.GetItemAsync(tableName, userId, itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Item);
    }

    [Fact]
    public async Task GetItemAsync_WithDynamoDbException()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user3";
        var itemId = "item3";
        _mockDynamoDbContext.Setup(x => x.GetItemAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, AttributeValue>>(), default))
            .ThrowsAsync(new AmazonDynamoDBException("Error fetching item"));

        // Act & Assert
        await Assert.ThrowsAsync<AmazonDynamoDBException>(() => _dynamoDbEndpoint.GetItemAsync(tableName, userId, itemId));
    }
}
