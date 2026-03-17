using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Models;

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
            DisableFetchingTableMetadata = true,
            Conversion = DynamoDBEntryConversion.V2
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
    public async Task<GameSession> GetGameSessionByInviteCodeAsync(int inviteCode)
    {
        var search = DBContext.FromQueryAsync<GameSession>(new QueryOperationConfig()
        {
            IndexName = "InviteCode-index",
            Filter = new QueryFilter("InviteCode", QueryOperator.Equal, inviteCode)
        });
        var searchResponse = await search.GetRemainingAsync();
        if (searchResponse.Count == 0)
        {
            throw new KeyNotFoundException($"Game session with invite code '{inviteCode}' was not found.");
        }
        else
        {
            return searchResponse[0]; //if we found anything that matches our condition, get the first one
        }
    }

    public async Task DeleteAllEntriesFromTableAsync<T>()
    {
        var search = DBContext.FromScanAsync<T>(new ScanOperationConfig());
        var searchResponse = await search.GetRemainingAsync();
        foreach (var item in searchResponse)
        {
            await DBContext.DeleteAsync(item);
        }
    }
}