using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using Moq;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;


namespace SocketService.Tests;

public class FunctionsTest
{
    public FunctionsTest()
    {
    }

    [Fact]
    public async Task TestConnect()
    {
        Mock<IAmazonDynamoDB> _mockDDBClient = new Mock<IAmazonDynamoDB>();
        Mock<IAmazonApiGatewayManagementApi> _mockApiGatewayClient = new Mock<IAmazonApiGatewayManagementApi>();
        string tableName = "mocktable";
        string connectionId = "test-id";

        _mockDDBClient.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((request, token) =>
            {
                Assert.Equal(tableName, request.TableName);
                Assert.Equal(connectionId, request.Item[Functions.ConnectionIdField].S);
            });

        var functions = new Functions(_mockDDBClient.Object, (endpoint) => _mockApiGatewayClient.Object, tableName);

        var lambdaContext = new TestLambdaContext();

        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId
            }
        };
        var response = await functions.OnConnectHandler(request, lambdaContext);
        Assert.Equal(200, response.StatusCode);
    }


    [Fact]
    public async Task TestDisconnect()
    {
        Mock<IAmazonDynamoDB> _mockDDBClient = new Mock<IAmazonDynamoDB>();
        Mock<IAmazonApiGatewayManagementApi> _mockApiGatewayClient = new Mock<IAmazonApiGatewayManagementApi>();
        string tableName = "mocktable";
        string connectionId = "test-id";

        _mockDDBClient.Setup(client => client.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DeleteItemRequest, CancellationToken>((request, token) =>
            {
                Assert.Equal(tableName, request.TableName);
                Assert.Equal(connectionId, request.Key[Functions.ConnectionIdField].S);
            });

        var functions = new Functions(_mockDDBClient.Object, (endpoint) => _mockApiGatewayClient.Object, tableName);

        var lambdaContext = new TestLambdaContext();

        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId
            }
        };
        var response = await functions.OnDisconnectHandler(request, lambdaContext);
        Assert.Equal(200, response.StatusCode);
    }
    
}