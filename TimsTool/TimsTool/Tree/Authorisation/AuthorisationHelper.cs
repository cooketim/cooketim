using Identity;
using Models.ViewModels;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Security.Principal;
using TimsTool.DataService;
using System.Threading.Tasks;

namespace TimsTool.Tree.Authorisation
{
    public class AuthorisationHelper : IAuthorisationHelper
    {
        private readonly IPrincipal customPrincipal;
        private readonly IResultsDataClient resultsDataClient;

        public AuthorisationHelper(IPrincipal customPrincipal, IResultsDataClient resultsDataClient)
        {
            this.customPrincipal = customPrincipal;
            this.resultsDataClient = resultsDataClient;
        }
        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        private void output(LogonModel viewModel, string output, LogEventLevel logEventLevel)
        {
            viewModel.Messages = viewModel.Messages + output + Environment.NewLine;
            switch (logEventLevel)
            {
                case LogEventLevel.Error:
                    {
                        Log.Error(output);
                        break;
                    }
                case LogEventLevel.Fatal:
                    {
                        Log.Fatal(output);
                        break;
                    }
                case LogEventLevel.Debug:
                    {
                        Log.Debug(output);
                        break;
                    }
                case LogEventLevel.Verbose:
                    {
                        Log.Verbose(output);
                        break;
                    }
                case LogEventLevel.Warning:
                    {
                        Log.Warning(output);
                        break;
                    }
                case LogEventLevel.Information:
                    {
                        Log.Information(output);
                        break;
                    }
            }
        }

        public async Task<bool> Authorise(User signedInUser)
        {
            //Get the collection of authorised users
            AllUsers allUsers = await resultsDataClient.GetAuthorisedUsersAsync();

            var matchedUser = allUsers.AuthorisedUsers.FirstOrDefault(x => x.Email.ToLowerInvariant() == signedInUser.Email.ToLowerInvariant());
            var authorised = matchedUser != null;
            if (authorised)
            {
                //set the roles
                ResultsTreeIdentity resultsTreeIdentity = customPrincipal as ResultsTreeIdentity;
                if (resultsTreeIdentity != null)
                {
                    resultsTreeIdentity.User = matchedUser;
                }

                //set the signed in user on the authentication helper
                IdentityHelper.SignedInUser = matchedUser;
            }

            return authorised;
        }

        public void SetSecurityPrincipal(LogonModel viewModel)
        {
            if (viewModel.SignedInUser == null)
            {
                output(viewModel, "You are not authenticated.", LogEventLevel.Error);
                return;
            }

            //Get the current principal object
            ResultsTreePricipal resultsPrincipal = customPrincipal as ResultsTreePricipal;
            if (resultsPrincipal == null)
            {
                output(viewModel, "The application's default thread principal must be set to a CustomPrincipal object on startup.", LogEventLevel.Error);
                return;
            }

            //Authenticate the user
            resultsPrincipal.Identity = new ResultsTreeIdentity(viewModel.SignedInUser, true);

            if (viewModel.SignedInUser.Roles != null && viewModel.SignedInUser.Roles.Contains("administrator"))
            {
                output(viewModel, "Administrator detected!", LogEventLevel.Information);
            }

            return;
        }
    }
}
