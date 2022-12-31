// See https://aka.ms/new-console-template for more information

/*
 * This Sample is for a User to login interactively to the Dataverse using the MSAL Library
 * This method will also respect any potential MFA / Conditional Access Policy Requirements
 *
 * Nuget Packages:
 * ----------------
 * Microsoft.Identity.Client - Version 4.49.1
 * Microsoft.PowerPlatform.Dataverse.Client - Version 1.0.26
 */

using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Utils;
using MsalUserInteractiveEarlyBoundSample.Core.Dataverse;

try
{
    Console.WriteLine("Starting Microsoft Dataverse connection sample code.");
    Console.WriteLine("Creating Connection Objects...");
    
    var dataverseAuthenticationService = new DataverseAuthenticationService();
    var dataverseUri = new Uri("https://<Org Name>.crmXX.dynamics.com");

    try
    {
        var client = new ServiceClient(dataverseUri, dataverseAuthenticationService.GenerateBearerTokenAsync);

        Console.WriteLine("Executing Who Am I request for Dataverse Instance...");
        var whoAmIRequest = new WhoAmIRequest();
        var whoAmIResponse = (WhoAmIResponse)client.Execute(whoAmIRequest);

        Console.WriteLine("Who Am I Response:");
        Console.WriteLine("\t Organization Id: " + whoAmIResponse.OrganizationId);
        Console.WriteLine("\tBusiness Unit Id: " + whoAmIResponse.BusinessUnitId);
        Console.WriteLine("\t         User Id: " + whoAmIResponse.UserId);

        Console.WriteLine("Attempting to retrieve active account records...");

        using var dataverseContext = new DataverseContext(client);
        var accounts = dataverseContext.AccountSet
            .Where(a => a.StateCode == AccountState.Active)
            .Take(10)
            .ToArray();

        Console.WriteLine($"Retrieved {accounts.Length} record(s).");
        Console.WriteLine(Environment.NewLine);

        foreach (var account in accounts)
        {
            Console.WriteLine("\t         Account Name: " + account.Name);
            Console.WriteLine("\t           Account Id: " + account.Id);
            Console.WriteLine("\tAccount Status Reason: " + account.StatusCode);
            Console.WriteLine(Environment.NewLine);
        }

        Console.WriteLine("End of account data found.");
    }
    catch (FaultException faultException)
    {
        //These errors should normally occur when there is an application error
        Console.WriteLine("Fault Exception occured whilst querying Dataverse data");
        Console.WriteLine("Fault Reason: " + faultException.Message);
    }
    catch (DataverseConnectionException connectionException)
    {
        //These errors should normally occur when there is a problem connecting to the dataverse instance
        Console.WriteLine("Dataverse Client has a connection exception");
        Console.WriteLine("Exception Message: " + connectionException.Message);
    }
    
}
catch (Exception exception)
{
  //Catch any other errors 
  Console.WriteLine("Fatal Exception occurred!");
  Console.WriteLine(exception);
}

Console.WriteLine("End of Sample code.");
Console.ReadLine();

/// <summary>
/// Class that retrieves Authentication Tokens for Dataverse
/// </summary>
public class DataverseAuthenticationService
{
    private IPublicClientApplication _clientApplication;

    public DataverseAuthenticationService()
    {
        _clientApplication = PublicClientApplicationBuilder
            .Create(ApplicationId)
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

    public async Task<string> GenerateBearerTokenAsync(string str = default)
    {
        string bearerToken = null;
        try
        {
            var accounts = await _clientApplication.GetAccountsAsync();
            var account = accounts?.FirstOrDefault();
            AuthenticationResult authenticationResult;
            //Check if we have a cached account profile.
            if (null != account)
            {
                //Do not challenge interactively, if previously challenged. 
                authenticationResult = await _clientApplication.AcquireTokenSilent(_scopes, account)
                    .ExecuteAsync();
            }
            else
            {
                //Challenge the user to sign in interactively. 
                authenticationResult = await _clientApplication.AcquireTokenInteractive(_scopes)
                    .ExecuteAsync();
            }

            bearerToken = authenticationResult.AccessToken;
        }
        catch (MsalException msalException)
        {
            Console.WriteLine(msalException);
        }

        return bearerToken;
    }
}