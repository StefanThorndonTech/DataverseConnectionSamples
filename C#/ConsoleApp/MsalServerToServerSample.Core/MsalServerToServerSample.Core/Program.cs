// See https://aka.ms/new-console-template for more information

/*
This sample is for a server to server interaction using the MSAL library
and a Client App and Secret to authenticate
   
   Nuget Packages: 
   ---------------
   Microsoft.Identity.Client - version 4.49.0
   Newtonsoft.Json - version 13.0.2
   
 */
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

try
{
    Console.WriteLine("Starting Dataverse connection sample code");
    Console.WriteLine("Creating Connection Objects...");
    var dataverseAuthenticationService = new DataverseAuthenticationService();
    var dataverseHttpClientMessageHandler = new DataverseHttpMessageHandler(dataverseAuthenticationService);
    var httpClient = new HttpClient(dataverseHttpClientMessageHandler)
    {
        BaseAddress = new Uri("https://<Org Name>.crmXX.dynamics.com")
    };

    Console.WriteLine("Executing Who Am I request for Dataverse Instance...");

    var whoAmIRequest = new HttpRequestMessage(HttpMethod.Get, "api/data/v9.2/WhoAmI");
    var whoAmIResponse = await httpClient.SendAsync(whoAmIRequest);

    if (whoAmIResponse.IsSuccessStatusCode)
    {
        var responseBodyString = await whoAmIResponse.Content.ReadAsStringAsync();
        dynamic responseBody = JObject.Parse(responseBodyString);
        Console.WriteLine("Who Am I Request Successful!");
        Console.WriteLine("Who Am I Response:");
        Console.WriteLine("\t Organization Id: " + responseBody.OrganizationId);
        Console.WriteLine("\tBusiness Unit Id: " + responseBody.BusinessUnitId);
        Console.WriteLine("\t         User Id: " + responseBody.UserId);
        
    }
    else
    {
        Console.WriteLine("Who Am I Request has failed.");
        Console.WriteLine($"Response Status Code: {whoAmIResponse.StatusCode}; Response Status Phase: {whoAmIResponse.ReasonPhrase}");
        var responseMessage = await whoAmIResponse.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(responseMessage))
        {
            Console.WriteLine("Response Message: " +responseMessage);
        }
    }
}
catch (Exception exception)
{
    Console.WriteLine("Fatal Exception occurred!");
    Console.WriteLine(exception);
}

Console.WriteLine("End of Sample code");
Console.ReadLine();


/// <summary>
/// A handler to authenticate the http message to Dataverse
/// </summary>
public class DataverseHttpMessageHandler : DelegatingHandler
{
    private readonly DataverseAuthenticationService _dataverseAuthenticationService;
    
    /// <summary>
    /// Create an instance of the Dataverse Http Client Handler
    /// </summary>
    /// <param name="dataverseAuthenticationService"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DataverseHttpMessageHandler(DataverseAuthenticationService dataverseAuthenticationService)
    {
        /*
         * Setting the Inner Handler should only be needed in demos.
         * If using a Http Client Factory, this should not be needed.
         */
        InnerHandler = new HttpClientHandler(); 
        _dataverseAuthenticationService = dataverseAuthenticationService ?? 
                                          throw new ArgumentNullException(nameof(dataverseAuthenticationService));
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //Retrieve the bearer token for the request. One should be made per request.
        var bearerToken = await _dataverseAuthenticationService.GenerateBearerTokenAsync(cancellationToken);
        //Create and set the Authentication Header
        var authenticationHeader = new AuthenticationHeaderValue("bearer",bearerToken);
        request.Headers.Authorization = authenticationHeader;
        //Submit Request with Auth header
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}

/// <summary>
/// Class that retrieves Authentication Tokens for Dataverse
/// </summary>
public class DataverseAuthenticationService
{
    private IConfidentialClientApplication _clientApplication;

    public DataverseAuthenticationService()
    {
        _clientApplication = ConfidentialClientApplicationBuilder
            .Create(ApplicationId)
            .WithClientSecret(ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{TenantId}"))
            .WithRedirectUri("http://localhost")
            .Build();
    }
    
    /*
     * These values should never be stored in source code as constants.
     * They should be stored in configuration and should not be readable.
     */
    /// <summary>
    /// The Application Id that has been registered to access the D365 Api
    /// </summary>
    /// <remarks>
    /// Please see on how to register an app:
    /// https://docs.microsoft.com/en-us/power-apps/developer/data-platform/walkthrough-register-app-azure-active-directory
    /// After registering app, you must also add the application as a user in your Dataverse environment and assign a security role.
    /// </remarks>
    private const string ApplicationId = "<App Registation Id>";

    /// <summary>
    /// The Client Secret for the Application Registration
    /// </summary>
    private const string ClientSecret = "<App Registration Secret>";

    /// <summary>
    /// The tenant for the Application Registration and the Dataverse instance
    /// </summary>
    private const string TenantId = "<Azure AD Tenant Id>";

    /// <summary>
    /// The scopes for the Authentication Request
    /// </summary>
    private readonly string[] _scopes = new[]
    {
        /*
         * The scopes take the form of:
         *  https://<Org Name>.crmXX.dynamics.com/.default
         */
        "https://<Org Name>.crmXX.dynamics.com/.default"
    }; 
    
    public async Task<string> GenerateBearerTokenAsync(CancellationToken cancellationToken)
    {
        string bearerToken = null;
        try
        {
            var authenticationResult = await _clientApplication
                .AcquireTokenForClient(_scopes)
                .ExecuteAsync(cancellationToken);

            if (null != authenticationResult)
            {
                bearerToken = authenticationResult.AccessToken;
            }
        }
        catch (MsalException msalException)
        {
            Console.WriteLine(msalException);
        }
        return bearerToken;
    }
}