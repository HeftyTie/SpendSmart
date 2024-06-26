using Amazon.DynamoDBv2.DataModel;

namespace SpendSmart.Tests.DatabaseContext.DynamoDbEndpoint;

[DynamoDBTable("TestTable")]
public class SampleItem
{
    public string UserId { get; set; }
    public string Id { get; set; }
    public decimal Balance { get; set; }
}
