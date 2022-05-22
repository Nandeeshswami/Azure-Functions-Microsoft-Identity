using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Azure.Identity;


namespace FuncISVGraph
{
    public static class FuncISVGraphDemo
    {
        [FunctionName("FuncISVGraphDemo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            const string ClientId="CLIENT ID";
            const string TenantId= "TENANT ID";
            const string ClientS="CLIENT SECRET";
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var options = new TokenCredentialOptions
{
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
};

            var clientSecretCredential = new ClientSecretCredential(
            TenantId, ClientId, ClientS, options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
           var users = await graphClient.Users
	                .Request()
	                .GetAsync();
            log.LogInformation($"Session is delivered by {users[0].DisplayName}\n\nFor any questions, contact him at {users[0].Mail}");

            return new OkObjectResult($"Session is delivered by {users[0].DisplayName}\nFor any questions, contact him at {users[0].Mail}");
        }
    }
}
