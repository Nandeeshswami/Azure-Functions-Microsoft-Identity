# FunctionsnIdentity
In this repo, we have 2 samples which show case the usage of Microsoft identity platform in Azure Functions.

Sample 1(KVDemo) uses Managed Identity and Keyvault. It showcases that you can fetch resources in Azure protected by Azure AD with out credentials or secrets etc.

We use Azure.Security.KeyVault.Secrets to fetch us a secret client and all the work of talking to IDP and getting a token is all handled by the SDK. If you compare the
lines of code that SDK helps reduce is incrdible.

Here's the snip of code if you were to use SDK,

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
```
Happy Coding!
