# Azure Functions & Microsoft Identity

**Note: This is a developer scribble, for detailed steps, please comment.**
In this repo, we have 2 samples which show case the usage of Microsoft identity platform in Azure Functions.

1. **Sample 1(KVDemo) uses Managed Identity and Keyvault**. It showcases that you can fetch resources in Azure protected by Azure AD with out credentials or secrets etc.

We use Azure.Security.KeyVault.Secrets to fetch us a secret client and all the work of talking to IDP and getting a token is all handled by the SDK. If you compare the
lines of code that SDK helps reduce is incrdible.

Firstly enable Managed Identity, you can do that in Azure Function App-> Identity : Turn on System assigned Mananaged Identity
Now the Function app would be available in the Access Policies of Key Vault. Go to your keyvault resource add an Access policy for the above function.

Here's the snip of code if you were to use SDK, ensure to add the right name of key vault, rest is all magic.

            var keyVaultName = ("name of Keyvault");
            var kvUri = $"https://{keyVaultName}.vault.azure.net";
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            const string secretName = "SuperBowl2014";
            var secret = await client.GetSecretAsync(secretName);

Now in contrast, here's what you would have to do if you do not want the SDK, get a token and then get resources with KeyVault

```
public static async Task<string> Run(string input, TraceWriter log)
{
    string token = await GetToken("https://vault.azure.net", "2017-09-01");
    string secret = await GetSecret(input, token, "2016-10-01");
    return secret;
}
public static HttpClient InitializeTokenClient() {
    var client = new HttpClient() {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("MSI_ENDPOINT"))
    };
    client.DefaultRequestHeaders.Add("Secret", Environment.GetEnvironmentVariable("MSI_SECRET"));
    return client;
    
}
public static HttpClient tokenClient = InitializeTokenClient();
public static async Task<string> GetToken(string resource, string apiversion)  {
    string endpoint = String.Format("?resource={0}&api-version={1}", resource, apiversion);
    JObject tokenServiceResponse = JsonConvert.DeserializeObject<JObject>(await tokenClient.GetStringAsync(endpoint));
    return tokenServiceResponse["access_token"].ToString();
}

// Interacting with Key Vault
public static HttpClient keyVaultClient = new HttpClient();
public static async Task<string> GetSecret(string secretName, string token, string apiVersion) {
    string endpoint = String.Format("{0}secrets/{1}?api-version={2}",
        Environment.GetEnvironmentVariable("KEYVAULT_URL"),
        secretName,
        apiVersion
        );
    keyVaultClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
    JObject keyVaultResponse = JsonConvert.DeserializeObject<JObject>(await keyVaultClient.GetStringAsync(endpoint));
    return keyVaultResponse["value"].ToString(); 
}
```**

2. Now in Sample 2 we see how to use Graph and Graph SDK in Functions.
For this sample I am using Client credential flowessentially making my Azure Function a Daemon. This is handy in scenarios where we do not have a login experience To do that,

1. Enable Application Permissions for all the permission your app uses. In my cases I just User.Read.All.
2. Grant Admin consent as we do not have as we will not have a login experience.

Below is my snip of code. I am using Graph SDK to get the required info, in this case I am running a graph query using SDK. SDk is very handy and saves ton of plumbing code for getting data from Json output.

```

            const string ClientId="f8a9dc79-c660-43ba-93e2-a609925d8ad3";
            const string TenantId= "3c507b8e-c092-4587-8e82-8d01e802b405";
            const string ClientS="FqP8Q~2huIMgt~ui2iKKu.HR5pLZ7kSFQQHiqcBf";
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
```
Happy Coding!
