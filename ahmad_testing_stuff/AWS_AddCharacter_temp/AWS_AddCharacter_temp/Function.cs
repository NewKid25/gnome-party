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
        private readonly AmazonDynamoDBClient _dynamoClient;

        private const string CharacterTable = "Characters";
        private const string ConnectionTable = "WebSocketConnections";

        public Function()
        {
            _dynamoClient = new AmazonDynamoDBClient();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            var routeKey = request.RequestContext.RouteKey;
            context.Logger.LogLine($"Route hit: {routeKey}");

            try
            {
                switch (routeKey)
                {
                    case "$connect":
                        return await HandleConnect(request);

                    case "$disconnect":
                        return await HandleDisconnect(request);

                    case "addCharacter":
                        return await HandleAddCharacter(request, context);

                    default:
                        return new APIGatewayProxyResponse
                        {
                            StatusCode = 400,
                            Body = "Unknown route"
                        };
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine(ex.ToString());

                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = "Internal server error"
                };
            }
        }

        #region CONNECT

        private async Task<APIGatewayProxyResponse> HandleConnect(APIGatewayProxyRequest request)
        {
            await _dynamoClient.PutItemAsync(new PutItemRequest
            {
                TableName = ConnectionTable,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "ConnectionId", new AttributeValue { S = request.RequestContext.ConnectionId } }
                }
            });

            return new APIGatewayProxyResponse { StatusCode = 200 };
        }

        #endregion

        #region DISCONNECT

        private async Task<APIGatewayProxyResponse> HandleDisconnect(APIGatewayProxyRequest request)
        {
            await _dynamoClient.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = ConnectionTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ConnectionId", new AttributeValue { S = request.RequestContext.ConnectionId } }
                }
            });

            return new APIGatewayProxyResponse { StatusCode = 200 };
        }

        #endregion

        #region ADD CHARACTER

        private async Task<APIGatewayProxyResponse> HandleAddCharacter(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (string.IsNullOrEmpty(request.Body))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Request body is empty"
                };
            }


            context.Logger.LogInformation($"Body is ======= {request.Body}");

            var wrapper = JsonSerializer.Deserialize<ActionWrapper>(request.Body);

            if (wrapper?.Data == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Invalid character payload"
                };
            }

            var character = wrapper.Data;
            character.CharacterID = Guid.NewGuid();

            var item = new Dictionary<string, AttributeValue>
            {
                { "CharacterID", new AttributeValue { S = character.CharacterID.ToString() } },
                { "CharacterName", new AttributeValue { S = character.CharacterName ?? "" } },
                { "CharacterClass", new AttributeValue { S = character.CharacterClass ?? "" } },
                { "Health", new AttributeValue { S = character.Health ?? "" } }
            };

            await _dynamoClient.PutItemAsync(new PutItemRequest
            {
                TableName = CharacterTable,
                Item = item
            });

            // Send response back to caller
            var domain = request.RequestContext.DomainName;
            var stage = request.RequestContext.Stage;

            var apiClient = new AmazonApiGatewayManagementApiClient(
                new AmazonApiGatewayManagementApiConfig
                {
                    ServiceURL = $"https://{domain}/{stage}",
                    RegionEndpoint = _dynamoClient.Config.RegionEndpoint
                });

            var responseJson = JsonSerializer.Serialize(new
            {
                action = "addCharacterResult",
                data = character
            });

            await apiClient.PostToConnectionAsync(new PostToConnectionRequest
            {
                ConnectionId = request.RequestContext.ConnectionId,
                Data = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
            });

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = responseJson
            };
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
