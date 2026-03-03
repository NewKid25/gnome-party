using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using GnomeParty.Models;

namespace GnomeParty.Database;
public class DatabaseService
{
    IAmazonDynamoDB DDBClient { get; }
    IDynamoDBContext DBContext { get; }

    public DatabaseService()
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
    public DatabaseService(IAmazonDynamoDB ddbClient, IDynamoDBContext dbContext)
    {
        DDBClient = ddbClient;
        DBContext = dbContext;
    }

    public async Task SaveAsync<T>(T item)
    {
        await DBContext.SaveAsync(item);
        return;
    }

    public async Task<T> LoadAsync<T>(Object hashKey)
    {
        return await DBContext.LoadAsync<T>(hashKey);
    }
     public async Task DeleteAsync<T>(T item)
    {
        await DBContext.DeleteAsync(item);
        return;
    }
}