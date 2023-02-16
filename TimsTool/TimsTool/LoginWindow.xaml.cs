using Identity;
using Serilog;
using System;
using System.Windows;
using System.Threading;
using Models.ViewModels;
using Configuration;
using Serilog.Events;
using TimsTool.Tree.Authorisation;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using TimsTool.DataService;
using System.Threading.Tasks;
using TimsTool.Tree.Authentication;
using System.Linq;
using DataLib;

namespace TimsTool
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private LogonModel viewModel;
        private readonly AppSettings settings;
        private readonly IServiceProvider serviceProvider;
        private readonly IAuthorisationHelper authorisationHelper;
        private readonly IResultsDataClient resultsDataClient;
        bool IsLocalChangeMode = false;

        public LoginWindow(IOptions<AppSettings> settings, IAuthorisationHelper authorisationHelper, IResultsDataClient resultsDataClient)
        {
            this.settings = settings.Value;
            this.authorisationHelper = authorisationHelper;
            serviceProvider = ((App)Application.Current).ServiceProvider;
            this.resultsDataClient = resultsDataClient;

            InitializeComponent();

            viewModel = new LogonModel();
            viewModel.TestVisibility = settings.Value.IsTest;

            viewModel.Messages = string.Format("Running {0} '{1}'{2}Running at '{3}'{2}", settings.Value.IsTest ? "TEST" : "PROD", settings.Value.AppVersion, Environment.NewLine, settings.Value.BaseFileDirectory);

            //see if the user is already authenticated by way of windows authentication
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                WindowsIdentity identity = Thread.CurrentPrincipal.Identity as WindowsIdentity;
                viewModel.SignedInUser = new User(identity.Name, identity.Name, new string[] { });
                authorisationHelper.SetSecurityPrincipal(viewModel);

                //Authorise
                Authorise(viewModel).Wait();
  
                if (viewModel.IsAuthorised)
                {
                    //Make relevant buttons visible
                    SetButtonVisibility();
                }
            }

            // Let the UI bind to the view-model.
            DataContext = viewModel;
        }

        private async Task Authorise(LogonModel viewModel)
        {
            if (viewModel.SignedInUser == null)
            {
                output("You are not authenticated.", LogEventLevel.Error);
                viewModel.IsAuthorised = false;
                return;                
            }

            try
            {
                viewModel.IsAuthorised = await authorisationHelper.Authorise(viewModel.SignedInUser);

                if (!viewModel.IsAuthorised)
                {
                    //not authorised, remove the data file
                    var dataFilePath = settings.DataFilePath;

                    if (File.Exists(dataFilePath))
                    {
                        File.Delete(dataFilePath);
                    }

                    //set some output
                    output(string.Format("You are not authorised to continue. '{0}' is not an authorised user.  Please contact Tim Cooke.", viewModel.SignedInUser.Email), LogEventLevel.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                output("Access to Tim's Tool is not possible. Errors occured obtaining authorised users.  Please contact Tim Cooke.", LogEventLevel.Error);
                Log.Error(ex, "Exception whilst obtaining authorised users from Azure");
                viewModel.IsAuthorised = false;
                return;
            }            
        }

        private async void googleBtn_Click(object sender, RoutedEventArgs e)
        {
            //Authenticate
            var authProvider = new GoogleAuth();
            await authProvider.Authenticate(viewModel);
            if (!viewModel.IsConnected)
            {
                //Make relevant buttons visible
                btnLocalData.Visibility = Visibility.Visible;
                return;
            }
            
            if (viewModel.SignedInUser == null)
            {
                output("You are not authenticated.", LogEventLevel.Error);
                return;
            }

            authorisationHelper.SetSecurityPrincipal(viewModel);

            ////Authorise
            //authorisationHelper.AuthoriseUserForResultsTree(viewModel);
            //if (viewModel.IsAuthorised)
            //{
            //    // Determine current checkout status
            //    viewModel.CheckedOutUser = await resultsBlobService.GetCheckedOutUserAsync();

            //    //Make relevant buttons visible
            //    SetButtonVisibility();
            //}

            //Authorise
            var authorised = false;
            try
            {
                //hack to not use "data use quota" when user is tim cooke
                if (viewModel.SignedInUser.Email == "tim.cooke@solirius.com")
                {
                    viewModel.SignedInUser.Username = "tim.cooke";
                    viewModel.SignedInUser.Roles = new string[5] {"administrator",
                                                                    "edittor",
                                                                    "local_edittor",
                                                                    "local_resultsedittor",
                                                                    "reader"};
                    IdentityHelper.SignedInUser = viewModel.SignedInUser;
                    authorised = true;
                }
                else
                {
                    authorised = await authorisationHelper.Authorise(viewModel.SignedInUser);
                }                
            }
            catch (Exception ex)
            {
                output("Access to Tim's Tool is not possible. Errors occured obtaining authorised users.  Please contact Tim Cooke.", LogEventLevel.Error);
                Log.Error(ex, "Exception whilst obtaining authorised users from Azure");
                viewModel.IsAuthorised = false;
                return;
            }
            if (authorised)
            {
                viewModel.IsAuthorised = true;

                // Determine current checkout status
                try
                {
                    //hack to not use "data use quota" when user is tim cooke
                    if (viewModel.SignedInUser.Email == "tim.cooke@solirius.com")
                    {
                        viewModel.CheckedOutUser = null;
                    }
                    else
                    {
                        viewModel.CheckedOutUser = await resultsDataClient.GetCheckedOutUserAsync();
                    }
                }
                catch (Exception ex)
                {
                    output("Access to Tim's Tool is not possible. Errors occured setting the checked out user.  Please contact Tim Cooke.", LogEventLevel.Error);
                    Log.Error(ex, "Exception whilst obtaining checked out user from Azure");
                    viewModel.IsAuthorised = false;
                    return;
                }

                //Make relevant buttons visible
                SetButtonVisibility();
            }
            else
            {
                viewModel.IsAuthorised = false;
                output(string.Format("You are not authorised to continue. '{0}' is not an authorised user.  Please contact Tim Cooke.", viewModel.SignedInUser.Email), LogEventLevel.Error);
            }
        }

        private async void msBtn_Click(object sender, RoutedEventArgs e)
        {
            var authProvider = new MicrosoftAuth();
            await authProvider.Authenticate(viewModel);

            if (!viewModel.IsConnected)
            {
                //Make relevant buttons visible
                btnLocalData.Visibility = Visibility.Visible;
                return;
            }

            if (viewModel.IsAuthorised)
            {
                //Make relevant buttons visible
                SetButtonVisibility();
            }
        }

        public void SetButtonVisibility()
        {
            if (viewModel.IsCheckedOutUser)
            {
                //show
                btnCheckedout.Visibility = Visibility.Visible;
                btnCancelCheckout.Visibility = Visibility.Visible;
                //administrators can save locally even when checked out
                if (IdentityHelper.IsSignedInUserAnAdministrator())
                {
                    btnLocalData.Visibility = Visibility.Visible;
                }

                //collapse
                btnLatestData.Visibility = Visibility.Collapsed;
                btnCheckout.Visibility = Visibility.Collapsed;
                btnCheckout.Visibility = Visibility.Collapsed;

                if (!IdentityHelper.IsSignedInUserAnAdministrator())
                {
                    btnLocalData.Visibility = Visibility.Collapsed;
                }                
            }
            else
            {
                if (viewModel.IsCheckedOut)
                {
                    //is checkedout to someone else

                    //collapse
                    btnCheckout.Visibility = Visibility.Collapsed;

                    //show
                    btnLatestData.Visibility = Visibility.Visible;
                    if (IdentityHelper.IsSignedInUserALocalEdditor())
                    {
                        btnLocalData.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    //collapse
                    btnLocalData.Visibility = Visibility.Collapsed;

                    //show
                    btnLatestData.Visibility = Visibility.Visible;

                    //local edittors can run in local save mode
                    if (IdentityHelper.IsSignedInUserALocalEdditor())
                    {
                        btnLocalData.Visibility = Visibility.Visible;
                    }

                    if (IdentityHelper.IsSignedInUserAnEdditor())
                    {
                        btnCheckout.Visibility = Visibility.Visible;
                    }
                }

                //collapse
                btnCheckedout.Visibility = Visibility.Collapsed;
                btnCancelCheckout.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnLatestData_Click(object sender, RoutedEventArgs e)
        {
            //get latest data
            //await resultsBlobService.GetLatestDataAsync();

            try
            {
                await resultsDataClient.GetLatestDataAsync(settings.DataFileDirectory);
            }
            catch (Exception ex)
            {
                output("Access to Tim's Tool is not possible. Errors occured retrieving latest data.  Please contact Tim Cooke.", LogEventLevel.Error);
                Log.Error(ex, "Exception whilst obtaining latest data from Azure");
                return;
            }


            if (IdentityHelper.IsSignedInUserALocalEdditor())
            {
                IsLocalChangeMode = true;
            }

            OpenTree();
        }      

        private void btnLocalData_Click(object sender, RoutedEventArgs e)
        {
            if (IdentityHelper.IsSignedInUserALocalEdditor())
            {
                IsLocalChangeMode = true;
            }
            OpenTree();
        }

        private void btnCheckedout_Click(object sender, RoutedEventArgs e)
        {
            OpenTree();
        }

        private async void btnCancelCheckout_Click(object sender, RoutedEventArgs e)
        {
            //// Cancel checkout
            //await resultsBlobService.CancelCheckoutAsync();
            //viewModel.CheckedOutUser = null;

            try
            {
                viewModel.CheckedOutUser = await resultsDataClient.CancelCheckoutAsync(IdentityHelper.SignedInUser, settings.DataFileDirectory);
            }
            catch (Exception ex)
            {
                output("Access to Tim's Tool is not possible. Errors occured cancelling checkout.  Please contact Tim Cooke.", LogEventLevel.Error);
                Log.Error(ex, "Exception whilst cacelling checkout");
                return;
            }

            if (IdentityHelper.IsSignedInUserALocalEdditor())
            {
                IsLocalChangeMode = true;
            }

            // Open the tree
            OpenTree();
        }

        private async void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            // Determine current checkout status
            //viewModel.CheckedOutUser = await resultsBlobService.GetCheckedOutUserAsync();

            ////When already checked out abort
            //if (viewModel.IsCheckedOut)
            //{
            //    output(string.Format("Already checked out to {0} - {1}.  Please select again ...", viewModel.CheckedOutUser.Username, viewModel.CheckedOutUser.Email), LogEventLevel.Information);

            //    //Make relevant buttons visible                
            //    SetButtonVisibility();
            //    return;
            //}

            ////Check out the data
            //await resultsBlobService.CheckoutAsync();

            ////set the checked out user to the logged on user
            //viewModel.CheckedOutUser = IdentityHelper.SignedInUser;


            try
            {
                //set the checked out user
                viewModel.CheckedOutUser = await resultsDataClient.CheckoutAsync(IdentityHelper.SignedInUser, settings.DataFileDirectory);
            }
            catch (Exception ex)
            {
                output("Access to Tim's Tool is not possible. Errors occured retrieving checking out user.  Please contact Tim Cooke.", LogEventLevel.Error);
                Log.Error(ex, "Exception whilst checking out user in Azure");
                return;
            }

            //ensure that the checked out user is the signed in user i.e. that the checkout was successful
            if (!string.Equals(viewModel.CheckedOutUser.Email, IdentityHelper.SignedInUser.Email, StringComparison.InvariantCulture))
            {
                output(string.Format("Already checked out to {0} - {1}.  Please select again ...", viewModel.CheckedOutUser.Username, viewModel.CheckedOutUser.Email), LogEventLevel.Information);

                //Make relevant buttons visible
                SetButtonVisibility();
                return;
            }


            // Open the tree
            OpenTree();
        }

        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        public void output(string output, LogEventLevel logEventLevel)
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

        private void OpenTree()
        {
            output("Preparing data ...", LogEventLevel.Information);

            //Initialise the tree model
            var treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            treeModel.CheckOutUser = viewModel.IsCheckedOut ? viewModel.CheckedOutUser : null;
            treeModel.IsReadOnly = !viewModel.IsConnected;
            treeModel.IsLocalChangeMode = IsLocalChangeMode;
            ShowTree();
        }

        private void ShowTree()
        {
            var tree = serviceProvider.GetRequiredService<TreeWindow>();
            tree.Show();
            Close();
        }
    }
}
