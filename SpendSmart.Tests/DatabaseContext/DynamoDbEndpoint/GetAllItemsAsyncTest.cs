﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Moq;
using SpendSmart.DatabaseContext;

namespace SpendSmart.Tests.DatabaseContext.DynamoDbEndpoint;

public class GetAllItemsAsyncTest
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDbContext;
    private readonly Mock<DynamoDBContext> _context;
    private readonly DynamoDbEndpoint<SampleItem> _dynamoDbEndpoint;

    public GetAllItemsAsyncTest()
    {
        _mockDynamoDbContext = new Mock<IAmazonDynamoDB>();
        _context = new Mock<DynamoDBContext>(_mockDynamoDbContext.Object);
        _dynamoDbEndpoint = new DynamoDbEndpoint<SampleItem>(_mockDynamoDbContext.Object, _context.Object);
    }

    [Fact]
    public async Task GetItemsAsync_Success()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "User1";

        var mockResponse = new QueryResponse
        {
            Items =
            [
                new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue { S = userId } },
                    { "Id", new AttributeValue { S = "Item1" } }
                }
            ]
        };
        _mockDynamoDbContext.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default))
            .ReturnsAsync(mockResponse);

        // Act
        var items = await _dynamoDbEndpoint.GetItemsAsync(tableName, userId);

        // Assert
        Assert.Single(items);
        _mockDynamoDbContext.Verify(x => x.QueryAsync(It.Is<QueryRequest>(r => r.TableName == tableName), default), Times.Once);
    }

    [Fact]
    public async Task GetItemsAsync_WithNonExistentItems()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "User2";

        _mockDynamoDbContext.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default))
            .ReturnsAsync(new QueryResponse());

        // Act
        var items = await _dynamoDbEndpoint.GetItemsAsync(tableName, userId);

        // Assert
        Assert.Empty(items);
        _mockDynamoDbContext.Verify(x => x.QueryAsync(It.Is<QueryRequest>(r => r.TableName == tableName), default), Times.Once);
    }

    [Fact]
    public async Task GetItemAsync_WithDynamoDbException()
    {
        // Arrange
        var tableName = "TestTable";
        var userId = "User3";

        _mockDynamoDbContext.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default))
            .ThrowsAsync(new AmazonDynamoDBException("Error"));

        // Act
        var exception = await Xunit.Record.ExceptionAsync(() => _dynamoDbEndpoint.GetItemsAsync(tableName, userId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<AmazonDynamoDBException>(exception);
        _mockDynamoDbContext.Verify(x => x.QueryAsync(It.Is<QueryRequest>(r => r.TableName == tableName), default), Times.Once);
    }
}