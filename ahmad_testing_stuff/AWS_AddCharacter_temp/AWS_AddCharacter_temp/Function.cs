using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using System.Text;
using System.Text.Json;
using System.IO;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWS_AddCharacter_temp
{
    public class Function
    {
        private readonly IAmazonDynamoDB _dynamoClient;

        // Update these if your table names differ
        private const string CharacterTable = "Characters";
        private const string ConnectionTable = "WebSocketConnections";

        // Safer JSON options (case-insensitive, matches your lowerCamel JSON)
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public Function()
        {
            _dynamoClient = new AmazonDynamoDBClient();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            var routeKey = request?.RequestContext?.RouteKey ?? "";
            var connectionId = request?.RequestContext?.ConnectionId ?? "";
            context.Logger.LogLine($"Route hit: {routeKey} | ConnectionId: {connectionId}");

            try
            {
                return routeKey switch
                {
                    "$connect" => await HandleConnect(request, context),
                    "$disconnect" => await HandleDisconnect(request, context),
                    "addCharacter" => await HandleAddCharacter(request, context),
                    _ => new APIGatewayProxyResponse
                    {
                        StatusCode = 400,
                        Body = $"Unknown route: {routeKey}"
                    }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogLine("Unhandled exception: " + ex);

                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = "Internal server error"
                };
            }
        }

        #region CONNECT

        private async Task<APIGatewayProxyResponse> HandleConnect(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var connectionId = request.RequestContext.ConnectionId;

            context.Logger.LogLine($"$connect -> Storing ConnectionId: {connectionId}");

            await _dynamoClient.PutItemAsync(new PutItemRequest
            {
                TableName = ConnectionTable,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "ConnectionId", new AttributeValue { S = connectionId } },
                    { "ConnectedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
                }
            });

            return new APIGatewayProxyResponse { StatusCode = 200 };
        }

        #endregion

        #region DISCONNECT

        private async Task<APIGatewayProxyResponse> HandleDisconnect(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var connectionId = request.RequestContext.ConnectionId;

            context.Logger.LogLine($"$disconnect -> Deleting ConnectionId: {connectionId}");

            await _dynamoClient.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = ConnectionTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ConnectionId", new AttributeValue { S = connectionId } }
                }
            });

            return new APIGatewayProxyResponse { StatusCode = 200 };
        }

        #endregion

        #region ADD CHARACTER

        private async Task<APIGatewayProxyResponse> HandleAddCharacter(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                context.Logger.LogLine("addCharacter -> Request body was empty.");
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "Request body is empty" };
            }

            context.Logger.LogLine("addCharacter -> Raw body: " + request.Body);

            // Your WebSocket message format:
            // {"action":"addCharacter","data":{"characterName":"Sam","characterClass":"Bard","health":"25"}}
            var wrapper = JsonSerializer.Deserialize<ActionWrapper>(request.Body, JsonOptions);

            if (wrapper?.Data == null)
            {
                context.Logger.LogLine("addCharacter -> wrapper.Data was null (invalid payload).");
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid character payload" };
            }

            var character = wrapper.Data;
            character.CharacterID = Guid.NewGuid();

            // Save character
            var item = new Dictionary<string, AttributeValue>
            {
                { "CharacterID", new AttributeValue { S = character.CharacterID.ToString() } },
                { "CharacterName", new AttributeValue { S = character.CharacterName ?? "" } },
                { "CharacterClass", new AttributeValue { S = character.CharacterClass ?? "" } },
                { "Health", new AttributeValue { S = character.Health ?? "" } }
            };

            context.Logger.LogLine($"addCharacter -> Writing to DynamoDB table {CharacterTable}...");
            await _dynamoClient.PutItemAsync(new PutItemRequest
            {
                TableName = CharacterTable,
                Item = item
            });
            context.Logger.LogLine("addCharacter -> DynamoDB PutItem succeeded.");

            // Send response back to caller via WebSocket Management API
            var apiClient = CreateManagementApiClient(request, context);

            var responseJson = JsonSerializer.Serialize(new
            {
                action = "addCharacterResult",
                data = character
            });

            context.Logger.LogLine("addCharacter -> Posting response to connection...");
            await PostToConnectionOrThrow(apiClient, request.RequestContext.ConnectionId, responseJson, context);

            // Note: APIGatewayProxyResponse body is not what the WebSocket client receives;
            // PostToConnection is what matters. Still returning 200 is correct.
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "OK"
            };
        }

        private AmazonApiGatewayManagementApiClient CreateManagementApiClient(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var apiId = request.RequestContext.ApiId;
            var stage = request.RequestContext.Stage;
            var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

            var serviceUrl = $"https://{apiId}.execute-api.{region}.amazonaws.com/{stage}";
            context.Logger.LogLine("Management API ServiceURL: " + serviceUrl);

            return new AmazonApiGatewayManagementApiClient(
                new AmazonApiGatewayManagementApiConfig
                {
                    ServiceURL = serviceUrl
                });
        }

        private static async Task PostToConnectionOrThrow(
            IAmazonApiGatewayManagementApi apiClient,
            string connectionId,
            string message,
            ILambdaContext context)
        {
            try
            {
                await apiClient.PostToConnectionAsync(new PostToConnectionRequest
                {
                    ConnectionId = connectionId,
                    Data = new MemoryStream(Encoding.UTF8.GetBytes(message))
                });

                context.Logger.LogLine("PostToConnection succeeded.");
            }
            catch (GoneException)
            {
                // Client disconnected; not a server bug.
                context.Logger.LogLine("PostToConnection failed: GoneException (stale connection).");
                throw;
            }
            catch (Exception ex)
            {
                context.Logger.LogLine("PostToConnection failed with exception: " + ex);
                throw;
            }
        }

        #endregion
    }

    #region MODELS

    public class ActionWrapper
    {
        public string Action { get; set; }
        public Character Data { get; set; }
    }

    public class Character
    {
        public Guid? CharacterID { get; set; }
        public string CharacterName { get; set; }
        public string CharacterClass { get; set; }
        public string Health { get; set; }
    }

    #endregion
}
