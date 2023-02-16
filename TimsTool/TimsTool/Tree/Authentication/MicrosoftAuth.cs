using Microsoft.Identity.Client;
using Models.ViewModels;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Linq;
using Serilog.Events;
using System;
using Identity;

namespace TimsTool.Tree.Authentication
{
    public class MicrosoftAuth :AuthBase
    {
        const string clientId = "30a73090-33ac-4e44-a64f-7af97c5f4ea9";
        const string tennantId = "336fad72-f897-4f0a-b2bc-0f898b2f4cba";
        const string clientSecret = "93j7Q~R..VqIC2r~HRbmD7AtcUEr1e3l544oy";
        const string instance = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";

        const string authorizationEndpoint = "https://login.microsoftonline.com/organizations/oauth2/v2.0/authorize";
        const string tokenEndpoint = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";

        /// <summary>
        /// Path to the token cache
        /// </summary>
        private static string cacheFilePath = Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";

        private static readonly object FileLock = new object();

        //public async Task Authenticate(LogonModel viewModel)
        //{
        //    this.viewModel = viewModel;

        //    viewModel.IsConnected = CheckInternetConnection();

        //    if (!viewModel.IsConnected)
        //    {
        //        output("If you proceed with your local cached data, you will not be able to apply any changes and will have a READ ONLY view.", LogEventLevel.Warning);
        //        return;
        //    }

        //    // Generates state and PKCE values.
        //    string state = randomDataBase64url(32);
        //    string code_verifier = randomDataBase64url(32);
        //    string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
        //    const string code_challenge_method = "S256";
        //    const string api_scope = "email%20openid%20profile";

        //    // Creates a redirect URI using an available port on the loopback address.
        //    //string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
        //    string redirectURI = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        //    output("Checking web for sign in page", LogEventLevel.Information);

        //    // Creates an HttpListener to listen for requests on that redirect URI.
        //    var http = new HttpListener();
        //    http.Prefixes.Add(redirectURI);
        //    http.Start();

        //    // Creates the OAuth 2.0 authorization request.
        //    string authorizationRequest = string.Format("{0}?response_type=code&scope={1}&redirect_uri={2}&client_id={3}&state={4}&code_challenge={5}&code_challenge_method={6}",
        //        authorizationEndpoint,
        //        api_scope,
        //        System.Uri.EscapeDataString(redirectURI),
        //        clientId,
        //        state,
        //        code_challenge,
        //        code_challenge_method);

        //    // Opens request in the browser.
        //    System.Diagnostics.Process.Start(authorizationRequest);

        //    // Waits for the OAuth authorization response.
        //    var context = await http.GetContextAsync();

        //    // Brings this app back to the foreground.
        //    //this.Activate();

        //    // Sends an HTTP response to the browser.
        //    var response = context.Response;
        //    string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://login.microsoftonline.com'></head><body>Please return to Tim's Tool application.</body></html>");
        //    var buffer = Encoding.UTF8.GetBytes(responseString);
        //    response.ContentLength64 = buffer.Length;
        //    var responseOutput = response.OutputStream;
        //    Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
        //    {
        //        responseOutput.Close();
        //        http.Stop();
        //        Console.WriteLine("HTTP server stopped.");
        //    });

        //    // Checks for errors.
        //    if (context.Request.QueryString.Get("error") != null)
        //    {
        //        var error = String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error"));
        //        output(error, LogEventLevel.Error);
        //        return;
        //    }
        //    if (context.Request.QueryString.Get("code") == null
        //        || context.Request.QueryString.Get("state") == null)
        //    {
        //        var error = "Malformed authorization response. " + context.Request.QueryString;
        //        output(error, LogEventLevel.Error);
        //        return;
        //    }

        //    // extracts the code
        //    var code = context.Request.QueryString.Get("code");
        //    var incoming_state = context.Request.QueryString.Get("state");

        //    // Compares the receieved state to the expected value, to ensure that
        //    // this app made the request which resulted in authorization.
        //    if (incoming_state != state)
        //    {
        //        var error = String.Format("Received request with invalid state ({0})", incoming_state);
        //        output(error, LogEventLevel.Error);
        //        return;
        //    }
        //    output("Authorization code: " + code, LogEventLevel.Information);

        //    // Starts the code exchange at the Token Endpoint.
        //    viewModel.SignedInUser = await performCodeExchange(code, code_verifier, redirectURI);

        //    viewModel.IsAuthorised = await AuthoriseUserForResultsTree(viewModel.SignedInUser);
        //    if (!viewModel.IsAuthorised)
        //    {
        //        return;
        //    }

        //    //Get the current checked out user
        //    viewModel.CheckedOutUser = await TreeDataBlobService.GetCheckedOutUserAsync();
        //}

        //private async Task<User> performCodeExchange(string code, string code_verifier, string redirectURI)
        //{
        //    output("Checking authentication tokens", LogEventLevel.Information);

        //    // builds the  request
        //    string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
        //        code,
        //        System.Uri.EscapeDataString(redirectURI),
        //        clientId,
        //        code_verifier,
        //        clientSecret
        //        );

        //    // sends the request
        //    HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
        //    tokenRequest.Method = "POST";
        //    tokenRequest.ContentType = "application/x-www-form-urlencoded";
        //    tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        //    byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
        //    tokenRequest.ContentLength = _byteVersion.Length;
        //    Stream stream = tokenRequest.GetRequestStream();
        //    await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
        //    stream.Close();

        //    try
        //    {
        //        // gets the response
        //        WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
        //        using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
        //        {
        //            // reads response body
        //            string responseText = await reader.ReadToEndAsync();
        //            Log.Information(responseText);

        //            // converts to dictionary
        //            Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

        //            string access_token = tokenEndpointDecoded["access_token"];
        //            //var user = await userinfoCall(access_token);
        //            return null; //user;
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        output("Error exchanging tokens during sign in...", LogEventLevel.Error);
        //        if (ex.Status == WebExceptionStatus.ProtocolError)
        //        {
        //            var response = ex.Response as HttpWebResponse;
        //            if (response != null)
        //            {
        //                output("HTTP: " + response.StatusCode, LogEventLevel.Error);
        //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //                {
        //                    // reads response body
        //                    string responseText = await reader.ReadToEndAsync();
        //                    output(responseText, LogEventLevel.Error);
        //                }
        //            }

        //        }
        //        return null;
        //    }
        //}

        public async Task Authenticate(LogonModel viewModel)
        {
            this.viewModel = viewModel;
            string[] scopes = new string[] { "user.read" };

            var clientApp = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority($"{instance}{tennantId}")
                .WithDefaultRedirectUri()
                .Build();

            EnableSerialization(clientApp.UserTokenCache);

            var accounts = await clientApp.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();
            AuthenticationResult authResult = null;

            try
            {
                authResult = await clientApp.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await clientApp.AcquireTokenInteractive(scopes)
                        .WithAccount(firstAccount)
                        //.WithParentActivityOrWindow(new WindowInteropHelper(this).Handle) // optional, used to center the browser on the window
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    output($"Error Acquiring Token:{System.Environment.NewLine}{msalex}", LogEventLevel.Error);
                }
            }
            catch (Exception ex)
            {
                output($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}", LogEventLevel.Error);
                return;
            }

            if (authResult != null)
            {
                viewModel.SignedInUser = new User(authResult.Account.Username, authResult.Account.Username, new string[] { });
            }
        }

        private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(File.Exists(cacheFilePath)
                        ? ProtectedData.Unprotect(File.ReadAllBytes(cacheFilePath),
                                                 null,
                                                 DataProtectionScope.CurrentUser)
                        : null);
            }
        }

        public static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changesgs in the persistent store
                    File.WriteAllBytes(cacheFilePath,
                                       ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                             null,
                                                             DataProtectionScope.CurrentUser)
                                      );
                }
            }
        }

        private static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }
}
