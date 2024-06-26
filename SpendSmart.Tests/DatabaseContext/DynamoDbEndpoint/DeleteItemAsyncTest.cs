using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Moq;
using SpendSmart.DatabaseContext;
using Amazon.DynamoDBv2.Model;

namespace SpendSmart.Tests.DatabaseContext.DynamoDbEndpoint;

public class DeleteItemAsyncTest
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDbContext;
    private readonly Mock<DynamoDBContext> _context;
    private readonly DynamoDbEndpoint<SampleItem> _dynamoDbEndpoint;

    public DeleteItemAsyncTest()
    {
        _mockDynamoDbContext = new Mock<IAmazonDynamoDB>();
        _context = new Mock<DynamoDBContext>(_mockDynamoDbContext.Object);
        _dynamoDbEndpoint = new DynamoDbEndpoint<SampleItem>(_mockDynamoDbContext.Object, _context.Object);
    }

    [Fact]
    public async Task DeleteItemAsync_Success()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user1";
        var itemId = "item1";

        _mockDynamoDbContext.Setup(client => client.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), default))
                            .ReturnsAsync(new DeleteItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        await _dynamoDbEndpoint.DeleteItemAsync(tableName, userId, itemId);

        // Assert
        _mockDynamoDbContext.Verify(
            client => client.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteItemAsync_WithNonExistentItems()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user1";
        var itemId = "item1";

        _mockDynamoDbContext.Setup(client => client.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), default))
                            .ReturnsAsync(new DeleteItemResponse { HttpStatusCode = System.Net.HttpStatusCode.NotFound });

        // Act
        await _dynamoDbEndpoint.DeleteItemAsync(tableName, userId, itemId);

        // Assert
        _mockDynamoDbContext.Verify(
            client => client.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), default),
            Times.Once
        );

    }

    [Fact]
    public async Task DeleteItemAsync_WithDynamoDbException()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "user1";
        var itemId = "item1";

        _mockDynamoDbContext.Setup(client => client.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), default))
                            .ThrowsAsync(new AmazonDynamoDBException("An error occurred"));

        // Act
        await Assert.ThrowsAsync<AmazonDynamoDBException>(() => _dynamoDbEndpoint.DeleteItemAsync(tableName, userId, itemId));

        // Assert
        _mockDynamoDbContext.Verify(
            client => client.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), default),
            Times.Once
        );
    }
}
