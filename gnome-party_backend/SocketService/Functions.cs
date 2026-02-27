using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.Runtime.Endpoints;
using Models;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SocketService;

public class Functions
{
    public const string ConnectionIdField = "connectionId";
    private const string TABLE_NAME_ENV = "TABLE_NAME";

    /// <summary>
    /// DynamoDB table used to store the open connection ids. More advanced use cases could store logged on user map to their connection id to implement direct message chatting.
    /// </summary>
    string ConnectionMappingTable { get; }

    /// <summary>
    /// DynamoDB service client used to store and retieve connection information from the ConnectionMappingTable
    /// </summary>
    IAmazonDynamoDB DDBClient { get; }

    /// <summary>
    /// Factory func to create the AmazonApiGatewayManagementApiClient. This is needed to created per endpoint of the a connection. It is a factory to make it easy for tests
    /// to moq the creation.
    /// </summary>
    Func<string, IAmazonApiGatewayManagementApi> ApiGatewayManagementApiClientFactory { get; }


    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
        DDBClient = new AmazonDynamoDBClient();

        // Grab the name of the DynamoDB from the environment variable setup in the CloudFormation template serverless.template
        if(System.Environment.GetEnvironmentVariable(TABLE_NAME_ENV) == null)
        {
            throw new ArgumentException($"Missing required environment variable {TABLE_NAME_ENV}");
        }

        ConnectionMappingTable = System.Environment.GetEnvironmentVariable(TABLE_NAME_ENV) ?? "";

        this.ApiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) => 
        {
            return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = endpoint
            });
        });
    }

    /// <summary>
    /// Constructor used for testing allow tests to pass in moq versions of the service clients.
    /// </summary>
    /// <param name="ddbClient">The service client for accessing Amazon DynamoDB.</param>
    /// <param name="apiGatewayManagementApiClientFactory">The service client for accessing Amazon API Gateway.</param>
    /// <param name="connectionMappingTable">Name of the DynamoDB table to store websocket connection mappings.</param>
    public Functions(IAmazonDynamoDB ddbClient, Func<string, IAmazonApiGatewayManagementApi> apiGatewayManagementApiClientFactory, string connectionMappingTable)
    {
        this.DDBClient = ddbClient;
        this.ApiGatewayManagementApiClientFactory = apiGatewayManagementApiClientFactory;
        this.ConnectionMappingTable = connectionMappingTable;
    }

    public async Task<APIGatewayProxyResponse> OnConnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogInformation($"ConnectionId: {connectionId}");

            var playerId = CreateNewPlayerId();
            var ddbRequest = new PutItemRequest
            {
                TableName = ConnectionMappingTable,
                Item = new Dictionary<string, AttributeValue>
                {
                    {ConnectionIdField, new AttributeValue{ S = connectionId}}
                }
            };

            await DDBClient.PutItemAsync(ddbRequest);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Connected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("Error connecting: " + e.Message);
            context.Logger.LogInformation(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to connect: {e.Message}"
            };
        }
    }


    public async Task<APIGatewayProxyResponse> RouteRequestHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            await SendToConnectionAsync(request.RequestContext.ConnectionId, request, "hello from the other side");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "all good"
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("Error disconnecting: " + e.Message);
            context.Logger.LogInformation(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to send message: {e.Message}"
            };
        }
    }

    public async Task<APIGatewayProxyResponse> JoinGameSessionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var connectionId = request.RequestContext.ConnectionId;

        var playerId = CreateNewPlayerId();
        var ddbRequest = new PutItemRequest
        {
            TableName = ConnectionMappingTable,
            Item = new Dictionary<string, AttributeValue>
                {
                    {ConnectionIdField, new AttributeValue{ S = connectionId}},
                    {"playerId", new AttributeValue{ S =  playerId} }
                }
        };
        var respoonseTask = DDBClient.PutItemAsync(ddbRequest);

        var playerCharacter = new Character();
        var gameSession = new GameSession(new Connection(connectionId, playerId));
        gameSession.character = playerCharacter;

        var config = new DynamoDBContextConfig
        {
            DisableFetchingTableMetadata = true
        };

        var dbContext = new DynamoDBContext(DDBClient, config);

        var gameSessionTask =  dbContext.SaveAsync(gameSession);
        var boolTask = SendToConnectionAsync(request.RequestContext.ConnectionId, request, gameSession);

        //need to await all async code, otherwise the lambda will exit before the code has a chance to execute
        await respoonseTask;
        await gameSessionTask;
        await boolTask;

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "all good"
        };
    }

    public async Task<APIGatewayProxyResponse> OnDisconnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogInformation($"ConnectionId: {connectionId}");

            var ddbRequest = new DeleteItemRequest
            {
                TableName = ConnectionMappingTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    {ConnectionIdField, new AttributeValue {S = connectionId}}
                }
            };

            await DDBClient.DeleteItemAsync(ddbRequest);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Disconnected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("Error disconnecting: " + e.Message);
            context.Logger.LogInformation(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }

    string CreateNewPlayerId()
    {
        return "player-" + Guid.NewGuid().ToString();
    }

    async Task<bool> SendToConnectionAsync(string connectionId, APIGatewayProxyRequest request, object data)
    {
        return await SendToConnectionAsync(connectionId, request.RequestContext.DomainName, request.RequestContext.Stage, data);
    }

    async Task<bool> SendToConnectionAsync(string connectionId, string domainName, string stage, object data)
    {
        var postConnectionRequest = new PostToConnectionRequest
        {
            ConnectionId = connectionId,
            Data = new MemoryStream(UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)))
        };
        var endpoint = $"https://{domainName}/{stage}";
        var apiClient = ApiGatewayManagementApiClientFactory(endpoint);
        await apiClient.PostToConnectionAsync(postConnectionRequest);
        return true;
    }
}