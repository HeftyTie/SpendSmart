using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Moq;
using SpendSmart.DatabaseContext;
using Amazon.DynamoDBv2.Model;
using System.Net;

namespace SpendSmart.Tests.DatabaseContext.DynamoDbEndpoint;

public class AddItemAsyncTest
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDbContext;
    private readonly Mock<DynamoDBContext> _context;
    private readonly DynamoDbEndpoint<SampleItem> _dynamoDbEndpoint;


    public AddItemAsyncTest()
    {
        _mockDynamoDbContext = new Mock<IAmazonDynamoDB>();
        _context = new Mock<DynamoDBContext>(_mockDynamoDbContext.Object);
        _dynamoDbEndpoint = new DynamoDbEndpoint<SampleItem>(_mockDynamoDbContext.Object, _context.Object);
    }

    [Fact]
    public async Task AddItemAsync_Success()
    {
        // Arrange
        var sampleItem = new SampleItem 
        {
            Id = "sampleId",
            UserId = "Sample Name",
            Balance = 100.0m
        };
        var tableName = "TestTable";

        _mockDynamoDbContext.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                            .ReturnsAsync(new PutItemResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _dynamoDbEndpoint.AddItemAsync(tableName, sampleItem);

        // Assert
        _mockDynamoDbContext.Verify(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_WithNonExistentItems()
    {
        // Arrange
        var sampleItem = new SampleItem();
        var tableName = "TestTable";

        _mockDynamoDbContext.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                            .ReturnsAsync(new PutItemResponse { HttpStatusCode = HttpStatusCode.BadRequest });

        // Act
        await _dynamoDbEndpoint.AddItemAsync(tableName, sampleItem);

        // Assert
        _mockDynamoDbContext.Verify(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_WithDynamoDbException()
    {
        // Arrange
        var sampleItem = new SampleItem
        {
            Id = "sampleId",
            UserId = "Sample Name",
            Balance = 100.0m
        };
        var tableName = "TestTable";

        _mockDynamoDbContext.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                            .ThrowsAsync(new AmazonDynamoDBException("Error"));

        // Act
        var exception = await Xunit.Record.ExceptionAsync(() => _dynamoDbEndpoint.AddItemAsync(tableName, sampleItem));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<AmazonDynamoDBException>(exception);
        _mockDynamoDbContext.Verify(client => client.PutItemAsync(It.Is<PutItemRequest>(request =>
              request.TableName == tableName &&
              request.Item["Id"].S == sampleItem.Id &&
              request.Item["UserId"].S == sampleItem.UserId &&
              request.Item["Balance"].N == sampleItem.Balance.ToString()
          ), default), Times.Once);
    }
}
