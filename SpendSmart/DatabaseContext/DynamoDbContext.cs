using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace SpendSmart.DatabaseContext;

public class SpendSmartDynamoDbContext(IAmazonDynamoDB dynamoDbClient, ILogger<SpendSmartDynamoDbContext> logger)
{
    private readonly IAmazonDynamoDB _dynamoDbClient = dynamoDbClient;
    private readonly ILogger<SpendSmartDynamoDbContext> _logger = logger;

    public async Task CreateTableIfNotExistsAsync(string tableName)
    {
        try
        {
            List<AttributeDefinition> attributeDefinitions =
            [
                new() {
                    AttributeName = "UserId",
                    AttributeType = ScalarAttributeType.S
                },
            ];

            List <KeySchemaElement> keySchemaElements =
            [
                new() {
                        AttributeName = "UserId",
                        KeyType = KeyType.HASH
                },
            ];

            if (tableName != "Users")
            {
                attributeDefinitions.Add(new AttributeDefinition
                {
                    AttributeName = "Id",
                    AttributeType = ScalarAttributeType.S
                });

                keySchemaElements.Add(new KeySchemaElement
                {
                    AttributeName = "Id",
                    KeyType = KeyType.RANGE
                });
            }

            await _dynamoDbClient.CreateTableAsync(
                new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = attributeDefinitions,
                    KeySchema = keySchemaElements,
                    BillingMode = BillingMode.PROVISIONED,
                    ProvisionedThroughput = 
                    new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 2
                    }
                });

            int attempts = 0;
            bool isCreated = false;

            while (attempts < 10 && !isCreated)
            {
                var tableStatus = await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = "Users"
                });

                if (tableStatus.Table.TableStatus == TableStatus.ACTIVE)
                {
                    isCreated = true;
                }
                else
                {
                    await Task.Delay(1000);
                    attempts++;
                }
            }

            if (!isCreated)
                _logger.LogWarning("Table did not become active within the expected time.");
        }
        catch (ResourceInUseException)
        {
            _logger.LogInformation("Table already exists.");
        }
        catch (Exception)
        {
            _logger.LogError("An unexpected exception occurred while creating the table.");
        }
    }
}
