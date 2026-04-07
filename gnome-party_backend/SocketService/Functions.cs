using Amazon;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using CombatService;
using GnomeParty.Database;
using Models;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.GameMetaData;
using System.Net;
using System.Text;
using System.Text.Json;

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
            var regionName = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-2";

            return new AmazonApiGatewayManagementApiClient(
                new AmazonApiGatewayManagementApiConfig
                {
                    ServiceURL = endpoint,
                    AuthenticationRegion = regionName
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
    //{"route":"player-action","EncounterId":"","TargetCharacterId":"test-enemy-1","SourceCharacterId":"","Action":"Slash", "GameSessionId":""}
    public async Task<APIGatewayProxyResponse> PlayerActionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var databaseService = new DatabaseService();
            JsonDocument message = JsonDocument.Parse(request.Body);
            var combatRequest = message.Deserialize<CombatRequest>();
            var combatService = new CombatService.CombatService();
            var response = await combatService.CombatRequestHandlerAsync(combatRequest);
            if (response.Count == 0)
            {
               await SendToConnectionAsync(request.RequestContext.ConnectionId, request, new ConnectionMessage("combat-request-recieved-no-action",""));
            }
            else
            {
                var gameSession = await databaseService.LoadAsync<GameSession>(combatRequest.GameSessionId);            //var activeEncounter = new ActiveCombatEncounter()
                await BroadcastToConnectionAsync(gameSession, request, new ConnectionMessage("action-handler", response));
            }

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

    //{"route": "begin-combat-encounter", "GameSessionId": ""}
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

        Console.WriteLine($"BeginCombatEncounterHandler reached for GameSessionId: {gameSessionId}");
        var gameSession = await databaseService.LoadAsync<GameSession>(gameSessionId);
        Console.WriteLine($"After load");
        Console.WriteLine($"Game session is {JsonSerializer.Serialize(gameSession)}");

        var connectionId = request.RequestContext.ConnectionId;

        var activeEncounter = new ActiveCombatEncounter(gameSession.Campaign.PlayerCharacters, gameSession.Campaign.Encounters[0].Enemies);
        Console.WriteLine($"Pre save");
        Console.WriteLine($"Active encounter: {JsonSerializer.Serialize(activeEncounter)}");

        await databaseService.SaveAsync(activeEncounter);
        Console.WriteLine($"After save");

        await BroadcastToConnectionAsync(gameSession, request, new ConnectionMessage("begin-combat-encounter", activeEncounter));


        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "all good"
        };
    }

    //{"route":"join-game", "InviteCode":849175}
    public async Task<APIGatewayProxyResponse> JoinGameSessionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            var domainName = request.RequestContext.DomainName;
            var stage = request.RequestContext.Stage;

            context.Logger.LogInformation($"JoinGameSessionHandler reached");
            context.Logger.LogInformation($"ConnectionId: {connectionId}");
            context.Logger.LogInformation($"DomainName: {domainName}");
            context.Logger.LogInformation($"Stage: {stage}");
            context.Logger.LogInformation($"Body: {request.Body}");

            var databaseService = new DatabaseService();
            JsonDocument message = JsonDocument.Parse(request.Body);
            //var inviteCode = message.Deserialize<int>();
            //Console.WriteLine($"Invite code: {inviteCode}");

            JsonElement inviteCodeJsonElement;
            if (!message.RootElement.TryGetProperty("InviteCode", out inviteCodeJsonElement))
            {
                context.Logger.LogInformation("Could not find InviteCode in input");
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
            var inviteCode = inviteCodeJsonElement.GetInt32();
            GameSession gameSession;
            try
            {
                gameSession = await databaseService.GetGameSessionByInviteCodeAsync(inviteCode);
            }
            catch (KeyNotFoundException)
            {
                context.Logger.LogInformation($"No game session found with invite code {inviteCode}");
                await SendToConnectionAsync(connectionId, request, new ConnectionMessage("join-game-session-failed", $"No game session found with invite code {inviteCode}"));
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = $"No game session found with invite code {inviteCode}"
                };
            }
            context.Logger.LogInformation("Loaded existing game session");
            var playerId = CreateNewPlayerId();
            var connection = new GameConnection(connectionId, playerId, gameSession);

            gameSession.AddParticipant(connection);

            await databaseService.SaveAsync(connection);
            context.Logger.LogInformation("Saved connection");

            await databaseService.SaveAsync(gameSession);
            context.Logger.LogInformation("Saved game session");

            await SendToConnectionAsync(connectionId, request, new ConnectionMessage("join-game-connection", connection));
            await SendToConnectionAsync(connectionId, request, new ConnectionMessage("join-game-session", gameSession));
            context.Logger.LogInformation("Sent game session to connection");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "all good"
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("JoinGameSessionHandler failed: " + e.Message);
            context.Logger.LogInformation(e.StackTrace);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to join game: {e.Message}"
            };
        }
    }

    //{"route":"host-game"}
    public async Task<APIGatewayProxyResponse> HostGameSessionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;

            var databaseService = new DatabaseService();

            var playerId = CreateNewPlayerId();
            var connection = new GameConnection(connectionId, playerId);
            var gameSession = new GameSession(connection);
            connection.GameSessionId = gameSession.GameSessionId;
            while (true)
            {
                try
                {
                    await databaseService.GetGameSessionByInviteCodeAsync(gameSession.InviteCode); //will throw error if no game session with this code, disregard return value, just want to check if it exists 
                    context.Logger.LogInformation($"Generated invite code {gameSession.InviteCode} already exists. Generating a new code.");
                    gameSession.InviteCode = new Random().Next(100000, 1000000);//generate a new 6 digit code
                }
                catch (KeyNotFoundException) //strangely, if it throws this error, that means there is no existing game session with the same invite code, which is what we want
                { 
                    break;
                }
            }

            await databaseService.SaveAsync(connection);
            await databaseService.SaveAsync(gameSession);
            await SendToConnectionAsync(connectionId, request, new ConnectionMessage("host-game", gameSession));

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "all good"
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("JoinGameSessionHandler failed: " + e.Message);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to join game: {e.Message}"
            };
        }
    }

    //{"route":"lobby-ready", "CharacterType":"Mage"}
    public async Task<APIGatewayProxyResponse> LobbyParticipantReadyHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;

            var databaseService = new DatabaseService();

            JsonDocument message = JsonDocument.Parse(request.Body);
            var lobbyReadyRequest = message.Deserialize<LobbyReadyRequest>();

            var connection = await databaseService.LoadAsync<GameConnection>(connectionId);
            var gameSession = await databaseService.LoadAsync<GameSession>(connection.GameSessionId);

            Character character = lobbyReadyRequest!.CharacterType switch
            {
                "Mage" => new Mage(),
                "Warrior" => new Warrior(),
                _ => throw new ArgumentException($"Unknown character type: {lobbyReadyRequest.CharacterType}")

            };

            gameSession.AddPlayerCharacter(character);

            var tasks = new List<Task>
            {
                databaseService.SaveAsync(gameSession),
                SendToConnectionAsync(connectionId, request, new ConnectionMessage("lobby-ready-success", character)),
                SendToConnectionAsync(gameSession.Host.ConnectionId, request, new ConnectionMessage("lobby-ready", character))
            };
            await Task.WhenAll(tasks);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "all good"
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("LobbyParticipantReadyHandler failed: " + e.Message);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to ready up: {e.Message}"
            };
        }
    }

    //{"route":"lobby-unready"}
    public async Task<APIGatewayProxyResponse> LobbyParticipantUnreadyHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;

            var databaseService = new DatabaseService();

            var connection = await databaseService.LoadAsync<GameConnection>(connectionId);
            var gameSession = await databaseService.LoadAsync<GameSession>(connection.GameSessionId);

            gameSession.RemovePlayerCharacter(connectionId);

            var tasks = new List<Task>
            {
                databaseService.SaveAsync(gameSession),
                SendToConnectionAsync(connectionId, request, new ConnectionMessage("lobby-unready-success", "")),
                SendToConnectionAsync(gameSession.Host.ConnectionId, request, new ConnectionMessage("lobby-unready", connection))
            };
            await Task.WhenAll(tasks);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "all good"
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("LobbyParticipantUnreadyHandler failed: " + e.Message);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to unready up: {e.Message}"
            };
        }
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

            if (connection.GameSessionId != "not_inited") //is connection in a game session?
            {
                var gameSession = await databaseClient.LoadAsync<GameSession>(connection.GameSessionId);
                Console.WriteLine($"Game Session = {gameSession}");
                if (gameSession != null)
                {
                    if (gameSession.Host.ConnectionId == connectionId)
                    {
                        context.Logger.LogInformation($"Host disconnected, deleting game session {gameSession.GameSessionId}");
                        await databaseClient.DeleteAsync(gameSession);
                        await BroadcastToConnectionAsync(gameSession, request, new ConnectionMessage("host-disconnected", connection));
                        foreach(var participant in gameSession.Participants)
                        {
                            await databaseClient.DeleteAsync(participant);
                        }
                    }
                    else
                    {
                        gameSession.RemoveParticipant(connection.ConnectionId);
                        await databaseClient.SaveAsync(gameSession);
                        await BroadcastToConnectionAsync(gameSession, request, new ConnectionMessage("player-disconnected", connection));
                    }
                    Console.WriteLine($"disconnect complete");
                }
            }
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

        string endpoint = domainName.Contains(".execute-api.")
            ? $"https://{domainName}/{stage}"
            : $"https://{domainName}";

        Console.WriteLine($"SendToConnectionAsync");
        Console.WriteLine($"  connectionId: {connectionId}");
        Console.WriteLine($"  domainName: {domainName}");
        Console.WriteLine($"  stage: {stage}");
        Console.WriteLine($"  endpoint: {endpoint}");

        var apiClient = ApiGatewayManagementApiClientFactory(endpoint);
        await apiClient.PostToConnectionAsync(postConnectionRequest);
        return true;
    }

    public async Task<bool> BroadcastToConnectionAsync(GameSession gameSession, APIGatewayProxyRequest request, object data)
    {
        var tasks = new List<Task>();

        try
        {
            tasks.Add(SendToConnectionAsync(gameSession.Host.ConnectionId, request, data));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send message to Host {gameSession.Host.ConnectionId}: {ex.Message}");
        }

        foreach (var participant in gameSession.Participants)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await SendToConnectionAsync(participant.ConnectionId, request, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send message to participant {participant.ConnectionId}: {ex.Message}");
                }
            }));
        }
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error broadcasting to connections: {ex.Message}");
        }
            //await Task.WhenAll(tasks);
        return true;
    }
}