using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Moq;
using SpendSmart.DatabaseContext;

namespace SpendSmart.Tests.DatabaseContext;

public class SpendSmartDynamoDbContextTest
{
    [Fact]
    public async Task TestCreateTableIfNotExistsAsync_TableDoesNotExist_CreatesTable()
    {
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<SpendSmartDynamoDbContext>>();
        var context = new SpendSmartDynamoDbContext(mockDynamoDbClient.Object, mockLogger.Object);
        string tableName = "Users";

        mockDynamoDbClient.SetupSequence(x => x.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeTableResponse { Table = new TableDescription { TableStatus = TableStatus.CREATING } })
            .ReturnsAsync(new DescribeTableResponse { Table = new TableDescription { TableStatus = TableStatus.ACTIVE } });

        await context.CreateTableIfNotExistsAsync(tableName);

        mockDynamoDbClient.Verify(x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        mockDynamoDbClient.Verify(x => x.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task TestCreateTableIfNotExistsAsync_TableDidNotBecomeActive()
    {
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<SpendSmartDynamoDbContext>>();
        var context = new SpendSmartDynamoDbContext(mockDynamoDbClient.Object, mockLogger.Object);
        string expectedMessage = "Table did not become active within the expected time.";
        string tableName = "Users";

        mockDynamoDbClient.Setup(x => x.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeTableResponse
            {
                Table = new TableDescription
                {
                    TableStatus = TableStatus.CREATING
                }
            });

        await context.CreateTableIfNotExistsAsync(tableName);

        mockDynamoDbClient.Verify(x => x.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(10));

        mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)), // Check if the logged message contains the expected message
            null, 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task TestCreateTableIfNotExistsAsync_TableAlreadyExists_LogsInformation()
    {
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<SpendSmartDynamoDbContext>>();
        var context = new SpendSmartDynamoDbContext(mockDynamoDbClient.Object, mockLogger.Object);
        string expectedMessage = "Table already exists.";
        string tableName = "Users";

        mockDynamoDbClient.Setup(x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceInUseException(expectedMessage));

        await context.CreateTableIfNotExistsAsync(tableName);

        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)), // Check if the logged message contains the expected message
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task TestCreateTableIfNotExistsAsync_UnexpectedException_LogsErrorWithSpecificMessage()
    {
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        var mockLogger = new Mock<ILogger<SpendSmartDynamoDbContext>>();
        var context = new SpendSmartDynamoDbContext(mockDynamoDbClient.Object, mockLogger.Object);
        string expectedLogMessage = "An unexpected exception occurred while creating the table."; // Adjusted to match the actual log message
        string tableName = "Users";

        mockDynamoDbClient.Setup(x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An unexpected exception occurred.")); 

        await context.CreateTableIfNotExistsAsync(tableName);

        mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedLogMessage)), // Check if the logged message contains the expected log message
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
