using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using GnomeParty.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using GnomeParty.Database;
using GnomeParty.Combat;

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

            var databaseClient = new DatabaseService();
            var connection = new GameConnection(connectionId);
            await databaseClient.SaveAsync(connection);


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

    //{"route": "player-action","EncounterId":"50b8c0cf-e032-4625-ba07-dad08231081b", "TargetCharacterId":"test-enemy", "SourceCharacterId":"player-6a71319b-1c22-4fe6-a791-459d6d546ba5", "Action":"Slash", "GameSessionId":"f4477afa-a9e8-48fc-9dcc-60e7ac64ac3b"}
    public async Task<APIGatewayProxyResponse> PlayerActionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            await SendToConnectionAsync(request.RequestContext.ConnectionId, request, "player handler reached...");
            var databaseService = new DatabaseService();
            JsonDocument message = JsonDocument.Parse(request.Body);
            var combatRequest = message.Deserialize<CombatRequest>();
            var combatService = new CombatService();
            var response = await combatService.CombatRequestHandlerAsync(combatRequest);

            var gameSession = await databaseService.LoadAsync<GameSession>(combatRequest.GameSessionId);            //var activeEncounter = new ActiveCombatEncounter()
            //Console.WriteLine($"Game session {JsonSerializer.Serialize(gameSession)}");
            await BroadcastToConnectionAsync(gameSession, request, response);


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



    public async Task<APIGatewayProxyResponse> BeginCombatEncounterHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // example message body to trigger this route
        //{"route": "begin-combat-encounter", "GameSessionId": "f4477afa-a9e8-48fc-9dcc-60e7ac64ac3b"}
        // ripped most of this Json parsing code from WebSocket sample at https://github.com/aws/aws-lambda-dotnet/blob/master/Blueprints/BlueprintDefinitions/vs2026/WebSocketAPIServerless/template/src/BlueprintBaseName.1/Functions.cs
        JsonDocument message = JsonDocument.Parse(request.Body);

        JsonElement gameSessionIdElement;
        if (!message.RootElement.TryGetProperty("GameSessionId", out gameSessionIdElement) || gameSessionIdElement.GetString() == null)
        {
            context.Logger.LogInformation("Failed to find GameSessionId element in JSON document");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
        var gameSessionId = gameSessionIdElement.GetString() ?? "";
        var databaseService = new DatabaseService();

        var gameSession = await databaseService.LoadAsync<GameSession>(gameSessionId);
        var connectionId = request.RequestContext.ConnectionId;

        var activeEncounter = new ActiveCombatEncounter(gameSession.Campaign.PlayerCharacters, gameSession.Campaign.Encounters[0].Enemies);
        await databaseService.SaveAsync(activeEncounter);
        await SendToConnectionAsync(connectionId, request, activeEncounter);


        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "all good"
        };
    }

    //{"route":"join-game"}
    public async Task<APIGatewayProxyResponse> JoinGameSessionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var connectionId = request.RequestContext.ConnectionId;
        var databaseService = new DatabaseService();

        GameSession gameSession;
        var playerId = CreateNewPlayerId();
        var connection = new GameConnection(connectionId, playerId);
        try 
        {
            gameSession = await databaseService.GetGameSessionByInviteCodeAsync(0); //will throw exception if invite code not found
            //await SendToConnectionAsync(connectionId, request, "joining existing game session...");
        }
        catch
        {
            // create new game session (this will eventually be in a route that just the host calls)
            gameSession = new GameSession(connection); //in this case the new connection is also the host
        }

        gameSession.AddParticipant(connection);
        var connectionSaveTask = databaseService.SaveAsync(connection);
        var sessionSaveTask = databaseService.SaveAsync(gameSession);
        var sendTask = SendToConnectionAsync(connectionId, request, gameSession);
        //need to await all async code, otherwise the lambda will exit before the code has a chance to execute
        await connectionSaveTask;
        await sessionSaveTask;
        await sendTask;

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

            var databaseClient = new DatabaseService();
            var connection = await databaseClient.LoadAsync<GameConnection>(connectionId);
            await databaseClient.DeleteAsync(connection);

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

    public async Task<bool> BroadcastToConnectionAsync(GameSession gameSession, APIGatewayProxyRequest request, object data)
    {
        var hostSendTask = SendToConnectionAsync(gameSession.Host.ConnectionId, request, data);
        gameSession.Participants.ForEach(async (participant) =>
        {
            try
            {
                await SendToConnectionAsync(participant.ConnectionId, request, data);
            }
            catch (Exception ex)
            {
                // Log the exception and continue sending to other participants
                Console.WriteLine($"Failed to send message to participant {participant.ConnectionId}: {ex.Message}");
            }
        });
        await hostSendTask;
        return true;
    }
}