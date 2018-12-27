using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Xml.Linq;
using PRTGService.Service;
using System.IO;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PRTG
{

    public class Function
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

        private static async Task<Stream> GetPRTGSummary(
            String id,
            String sdate, 
            String edate,
            String username,
            String passhash
        )
        {
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");
            // sdate 2018-10-29-00-00-00
            // edate 2018-11-01-00-00-00

            username = Uri.EscapeDataString(username);

            var serviceUri = "https://monitoring.consegna.cloud/api/historicdata.xml" +
                "?id=" + id +
                "&sdate=" + sdate + 
                "&edate=" + edate + 
                "&username=" + username +
                "&passhash=" + passhash;
            Console.WriteLine(serviceUri);

            //var client = new HttpClient();
            var response = await client.GetAsync(serviceUri);

            if (!response.IsSuccessStatusCode) {
                throw new Exception("Service Request not successful " + response.StatusCode);
            }

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }


        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            try {
                //var queryStrings = apigProxyEvent.QueryStringParameters["sdate"];
                Console.WriteLine("apigProxyEvent count " + apigProxyEvent.QueryStringParameters.Count());

                var id = apigProxyEvent.QueryStringParameters["id"];
                var sdate = apigProxyEvent.QueryStringParameters["sdate"];
                var edate = apigProxyEvent.QueryStringParameters["edate"];
                var username = apigProxyEvent.QueryStringParameters["username"];
                var passhash = apigProxyEvent.QueryStringParameters["passhash"];

                // load xml into XElement
                XElement dataFromPRTG =
                    XElement.Load(GetPRTGSummary(
                        id,
                        sdate, 
                        edate,
                        username,
                        passhash
                    ).Result);
                var itemsList = from item in dataFromPRTG.Elements() select item;

                // get stats from xml
                StatsSummaryHolder stats = new StatsSummaryHolder();
                var summaryCalculator = new SummaryCalculator();
                stats = summaryCalculator.GetDataXml(stats, itemsList);

                var cpu = summaryCalculator.GetAverageFromSummary(stats, "CPU Utilization");

                Dictionary<string, string> body = new Dictionary<string, string>
                {
                    { "status", "success" },
                    { "cpu", cpu },
                };

                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(body),
                    StatusCode = 200,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };


            } catch (Exception e) {
                // catch error
                Dictionary<string, string> body = new Dictionary<string, string>
                {
                    { "status", "error" },
                    { "message", e.Message },
                    { "stacktrace", e.StackTrace },
                };

                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(body),
                    StatusCode = 500,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
        }
    }
}
