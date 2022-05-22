using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;


namespace KeyV.Function
{
    public static class KeyVFuncDemo
    {
        [FunctionName("KeyVFuncDemo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var keyVaultName = ("KEYVAULT NAME");
            var kvUri = $"https://{keyVaultName}.vault.azure.net";
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            const string secretName = "SuperBowl2014";
            var secret = await client.GetSecretAsync(secretName);
            log.LogInformation($"{secretName}: {secret.Value.Value}");
            
            
            
            const string secretName1 = "SuperBowlWinner2024";
            const string secretName2= "SuperBowl2023";
            var secret1 = await client.GetSecretAsync(secretName1);
            var secret2 = await client.GetSecretAsync(secretName2);
            log.LogInformation($"{secretName2}: {secret2.Value.Value}");
            log.LogInformation($"{secretName1}: {secret1.Value.Value}");
            string responseMessage = $"Super Bowl Winners\n==============\n{secretName}:{secret.Value.Value}\n{secretName2}:{secret2.Value.Value}\n{secretName1}:{secret1.Value.Value}\n";
            
            return new OkObjectResult(responseMessage);
        }
    }
}
