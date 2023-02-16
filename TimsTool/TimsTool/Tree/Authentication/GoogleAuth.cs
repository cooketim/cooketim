using Identity;
using Models.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TimsTool.Tree.Authentication
{
    public class GoogleAuth : AuthBase
    {
        //from https://console.developers.google.com/ for account calvustopcat@gmail.com
        const string clientID = "415183612713-fcg04ht8ngshnitein0b4rmkh76c2jha.apps.googleusercontent.com";
        const string clientSecret = "iUDBh92Dv4r61ZUmF89_Ky5O";

        const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

        public async Task Authenticate(LogonModel viewModel)
        {
            this.viewModel = viewModel;

            viewModel.IsConnected = CheckInternetConnection();

            if (!viewModel.IsConnected)
            {
                output("If you proceed with your local cached data, you will not be able to apply any changes and will have a READ ONLY view.", LogEventLevel.Warning);
                return;
            }

            // Generates state and PKCE values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";
            const string api_scope = "email%20openid%20profile";

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            output("Checking web for sign in page", LogEventLevel.Information);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope={1}&redirect_uri={2}&client_id={3}&state={4}&code_challenge={5}&code_challenge_method={6}",
                authorizationEndpoint,
                api_scope,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            // Opens request in the browser.
            var psi = new ProcessStartInfo
            {
                FileName = authorizationRequest,
                UseShellExecute = true
            };
            Process.Start(psi);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings this app back to the foreground.
            //this.Activate();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to Tim's Tool application.</body></html>");
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                var error = String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error"));
                output(error, LogEventLevel.Error);
                return;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                var error = "Malformed authorization response. " + context.Request.QueryString;
                output(error, LogEventLevel.Error);
                return;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                var error = String.Format("Received request with invalid state ({0})", incoming_state);
                output(error, LogEventLevel.Error);
                return;
            }
            output("Authorization code: " + code, LogEventLevel.Information);

            // Starts the code exchange at the Token Endpoint.
            viewModel.SignedInUser = await performCodeExchange(code, code_verifier, redirectURI);
        }

        private async Task<User> performCodeExchange(string code, string code_verifier, string redirectURI)
        {
            output("Checking authentication tokens", LogEventLevel.Information);

            // builds the  request
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier,
                clientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    Log.Information(responseText);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string access_token = tokenEndpointDecoded["access_token"];
                    var user = await userinfoCall(access_token);
                    return user;
                }
            }
            catch (WebException ex)
            {
                output("Error exchanging tokens during sign in...", LogEventLevel.Error);
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        output("HTTP: " + response.StatusCode, LogEventLevel.Error);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync();
                            output(responseText, LogEventLevel.Error);
                        }
                    }

                }
                return null;
            }
        }

        private async Task<User> userinfoCall(string access_token)
        {
            output("Getting user details...", LogEventLevel.Information);

            // sends the request
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userInfoEndpoint);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
            userinfoRequest.ContentType = "application/x-www-form-urlencoded";
            userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            // gets the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();

            using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
            {
                // reads response body
                string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
                Log.Information(userinfoResponseText);

                JObject jsonResp = JObject.Parse(userinfoResponseText);
                var name = jsonResp.Root["name"];
                var emailToken = jsonResp.Root["email"];
                var email = emailToken == null ? null : emailToken.ToString();

                output(string.Format("Welcome {0} - {1}", name, email), LogEventLevel.Information);

                //set administrator when appropriate                
                return new User(name == null ? null : name.ToString(), email, new string[] { });
            }
        }
    }
}
