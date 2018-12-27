using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using PRTG;

namespace PRTG.Tests
{
  public class FunctionTest
  {
    private static readonly HttpClient client = new HttpClient();

    private static async Task<string> GetCallingIP()
    {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var stringTask = client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);

            var msg = await stringTask;
            return msg.Replace("\n","");
    }

    //[Fact]
    public void TestPRTGFunctionHandler()
    {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            string cpu = "0.76";
            Dictionary<string, string> body = new Dictionary<string, string>
            {
                { "message", "prtg world" },
                { "cpu", cpu },
            };

            var ExpectedResponse = new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(body),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            request.QueryStringParameters = new Dictionary<string, string>();

            request.QueryStringParameters.Add("sdate", "2018-11-09-00-00-00");
            request.QueryStringParameters.Add("edate", "2018-11-29-00-00-00");

            var function = new Function();
            response = function.FunctionHandler(request, context);

            Console.WriteLine("Lambda Response: \n" + response.Body);
            Console.WriteLine("Expected Response: \n" + ExpectedResponse.Body);

            Assert.Equal(ExpectedResponse.Body, response.Body);
            Assert.Equal(ExpectedResponse.Headers, response.Headers);
            Assert.Equal(ExpectedResponse.StatusCode, response.StatusCode);
    }
  }
}
