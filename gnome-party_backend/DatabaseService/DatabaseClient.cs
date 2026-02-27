using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Models;

namespace DatabaseService;
public class DatabaseClient
{
    IAmazonDynamoDB DDBClient { get; }
    IDynamoDBContext DBContext { get; }

    public DatabaseClient()
    {
        DDBClient = new AmazonDynamoDBClient();
        var config = new DynamoDBContextConfig
        {
            DisableFetchingTableMetadata = true
        };

        DBContext = new DynamoDBContext(DDBClient, config);
    }

    /// <summary>
    /// constructor that can be used to inject dependencies, for testing.
    /// </summary>
    public DatabaseClient(IAmazonDynamoDB ddbClient, IDynamoDBContext dbContext)
    {
        DDBClient = ddbClient;
        DBContext = dbContext;
    }

    public async Task SaveAsync<T>(T item)
    {
        await DBContext.SaveAsync(item);
        return;
    }
}