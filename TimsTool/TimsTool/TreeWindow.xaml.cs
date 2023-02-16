using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Models.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using TimsTool.DataService;
using Identity;
using Serilog;
using System.Threading.Tasks;

namespace TimsTool
{
    /// <summary>
    /// Interaction logic for TreeWindow.xaml
    /// </summary>
    public partial class TreeWindow : Window
    {
        private readonly IAppSettings appSettings;
        private readonly ITreeModel treeModel;
        private readonly IServiceProvider serviceProvider;
        private readonly IResultsDataClient resultsDataClient;

        public TreeWindow(IOptions<AppSettings> settings, IResultsDataClient resultsDataClient, ITreeModel treeModel)
        {
            appSettings = settings.Value;
            this.resultsDataClient = resultsDataClient;
            serviceProvider = ((App)Application.Current).ServiceProvider;

            this.treeModel = treeModel;
            var data = treeModel.AllData;
            InitializeComponent();

            // Let the UI bind to the view-model.
            DataContext = treeModel;

            //vary the save button according to debug
            if (treeModel.IsLocalChangeMode)
            {
                btnSave.Content = "Save Locally";
            }
        }
        private async void btnFind_Click(object sender, RoutedEventArgs e)
        {
            SearchableTreeViewModel tvm = null;
            SilentObservableCollection<TreeViewItemViewModel> searchData = null;
            switch (treeModel.SelectedIndex)
            {
                case 0:
                    {
                        //result definitions tab selected, search result definitions
                        tvm = treeModel.AllResultDefinitionsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Cast<TreeViewItemViewModel>());
                        break;
                    }
                case 1:
                    {
                        //nows tab selected, search nows
                        tvm = treeModel.AllNowsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllNowsTreeViewModel.Nows.Cast<TreeViewItemViewModel>());
                        break;
                    }
                case 2:
                    {
                        //edts tab selected, search edts
                        tvm = treeModel.AllEDTsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllEDTsTreeViewModel.EDTs.Cast<TreeViewItemViewModel>());
                        break;
                    }
                case 3:
                    {
                        //now subscriptions tab selected, search now subscriptions
                        tvm = treeModel.AllNowSubscriptionsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.Cast<TreeViewItemViewModel>());
                        break;
                    }
                case 4:
                    {
                        //EDT subscriptions tab selected, search EDT subscriptions
                        tvm = treeModel.AllEDTSubscriptionsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.Cast<TreeViewItemViewModel>());
                        break;
                    }
                case 5:
                    {
                        //Informant Register subscriptions tab selected, search Informant Register subscriptions
                        tvm = treeModel.AllInformantRegisterSubscriptionsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.Cast<TreeViewItemViewModel>());
                        break;
                    }
                case 6:
                    {
                        //Court Register subscriptions tab selected, search Court Register subscriptions
                        tvm = treeModel.AllCourtRegisterSubscriptionsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.Cast<TreeViewItemViewModel>());
                        break;
                    }
                case 7:
                    {
                        //Prison Court Register subscriptions tab selected, search Prison Court Register subscriptions
                        tvm = treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel;
                        searchData = new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.Cast<TreeViewItemViewModel>());
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if (tvm != null)
            {
                tvm.PerformSearch(searchData, treeModel.SearchText);

                if (!string.IsNullOrEmpty(tvm.SearchErrorText))
                {
                    OnOpenErrorDialog(tvm.SearchErrorText, "Search returned the following errors...", "Search Errors");
                }
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //save tree data
            var errorMessage = await SaveData();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                OnOpenErrorDialog(errorMessage, "Changes not saved. Fatal errors ocurred saving to Azure");
            }
            else
            {

                //show any validation errors
                if (string.IsNullOrEmpty(treeModel.ValidateCommand.ErrorMessages))
                {
                    OnOpenErrorDialog(null, "Saved without errors.");
                }
                else
                {
                    OnOpenErrorDialog(treeModel.ValidateCommand.ErrorMessages, "Saved with the following data errors...");
                }
            }
        }

        private async Task<string> SaveData()
        {
            //first validate
            treeModel.ValidateCommand.Execute(null);

            //save to file
            treeModel.SaveToFileCommand.Execute(appSettings.DataFilePath);

            //only save to azure when required
            if (treeModel.IsLocalChangeMode)
            {
                return null;
            }

            try
            {
                var resp = await resultsDataClient.SaveDataAsync(IdentityHelper.SignedInUser, appSettings.DataFilePath);
                treeModel.CheckOutUser = resp.CheckedoutUser;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception whilst saving changes to Azure");
                return ex.Message;
            }
            return null;
        }

        private async void btnCheckIn_Click(object sender, RoutedEventArgs e)
        {
            //first validate
            treeModel.ValidateCommand.Execute(null);

            //save to file
            treeModel.SaveToFileCommand.Execute(appSettings.DataFilePath);

            try
            {
                var resp = await resultsDataClient.CheckinAsync(IdentityHelper.SignedInUser, appSettings.DataFilePath);
                treeModel.CheckOutUser = resp.CheckedoutUser;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception whilst checking in changes to Azure");
                OnOpenErrorDialog(ex.Message, "Checkin failed, changes not saved to the server. Fatal errors ocurred saving to Azure");
                return;
            }

            //show any validation errors
            if (string.IsNullOrEmpty(treeModel.ValidateCommand.ErrorMessages))
            {
                OnOpenErrorDialog(null, "Checked in and saved without errors.");
            }
            else
            {
                OnOpenErrorDialog(treeModel.ValidateCommand.ErrorMessages, "Checked in and saved with the following data errors...");
            }
        }

        private void OnOpenErrorDialog(string errorMessages, string title, string windowTitle = null)
        {
            var errorModel = new MessagesModel()
            {
                Title = title,
                Messages = errorMessages
            };
            if (!string.IsNullOrEmpty(windowTitle))
            {
                errorModel.WindowTitle = windowTitle;
            }
            MessageWindow win = new MessageWindow(errorModel);
            win.Owner = this;
            win.ShowDialog();
        }

        private async void btnExportDataPatches_Click(object sender, RoutedEventArgs e)
        {
            //only allow data patches for valid data
            treeModel.ValidateCommand.Execute(null);
            if (string.IsNullOrEmpty(treeModel.ValidateCommand.ErrorMessages))
            {
                //open data patch dialog
                await OnOpenDataPatchDialog();
            }
            else
            {
                // show errors
                OnOpenErrorDialog(treeModel.ValidateCommand.ErrorMessages, "Please correct the following errors before exporting data...");
            }
        }

        private async void btnReport_Click(object sender, RoutedEventArgs e)
        {
            ReportWindow win = serviceProvider.GetRequiredService<ReportWindow>();
            win.Owner = this;

            if (win.ShowDialog() == true)
            {
                //set the data map when required
                var reportVM = win.DataContext as ReportViewModel;
                if (reportVM.SelectedReport.ReportShortName.ToLowerInvariant() == "fullextract")
                {
                    var errorMessage = await GetIdMap();
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        OnOpenErrorDialog(errorMessage, "Cannot generate report. Fatal errors ocurred retrieving the Id Map from Azure");
                        return;
                    }
                }
                
                treeModel.GenerateReportCommand.Execute(reportVM);
                OpenFiles("Reports");
            }
        }

        private void btnSampleNows_Click(object sender, RoutedEventArgs e)
        {
            treeModel.SampleNowCommand.Execute("all");
            OpenFiles("nowContent");
        }

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFiles(null);
        }

        private async Task OnOpenDataPatchDialog()
        {
            //determine if changes should be saved or overwritten
            if (treeModel.CheckOutUser != null && IdentityHelper.SignedInUser != null
                && !string.IsNullOrEmpty(treeModel.CheckOutUser.Email) && !string.IsNullOrEmpty(IdentityHelper.SignedInUser.Email)
                && string.Equals(treeModel.CheckOutUser.Email, IdentityHelper.SignedInUser.Email, StringComparison.InvariantCulture))
            {
                //save any changes first
                var errorMessage = await SaveData();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnOpenErrorDialog(errorMessage, "Cannot generate data patch, changes not saved. Fatal errors ocurred saving to Azure");
                    return;
                }
            }

            //set the data map
            if (treeModel.Map == null)
            {
                var errorMessage = await GetIdMap();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnOpenErrorDialog(errorMessage, "Cannot generate data patch. Fatal errors ocurred retrieving the Id Map from Azure");
                    return;
                }
            }

            DataPatchByTagWindow win = new DataPatchByTagWindow(treeModel, appSettings.DataFileDirectory);
            win.Owner = this;

            if (win.ShowDialog() == true)
            {
                treeModel.DataPatchByTagCommand.Execute(win.DataContext);
                await SaveIdMap();
                OpenFiles("DataPatches");
            }
        }

        private async Task SaveIdMap()
        {
            if (!treeModel.IsReadOnly && treeModel.Map.IsChanged)
            {
                try
                {
                    await resultsDataClient.SaveIdMapAsync(treeModel.Map);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception whilst saving the Id Map in Azure");
                }
            }
        }

        private async Task<string> GetIdMap()
        {
            try
            {
                treeModel.Map = await resultsDataClient.GetIdMapAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception whilst retrieving the Id Map from Azure");
                return ex.Message;
            }
            return null;
        }

        private void ResultsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                //un select tree view items when tab is changed
                if (treeModel.SelectedItem != null)
                {
                    treeModel.SelectedItem.IsSelected = false;
                    treeModel.SelectedItem = null;
                }
            }
        }

        public void OpenFiles(string folder)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Data files (*.bin;*.json;*.xlsx,*.csv,*.zip,*.txt,*.log)|*.bin;*.json;*.xlsx;*.csv;*.zip;*.txt;*.log|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = !string.IsNullOrEmpty(folder) ? System.IO.Path.Combine(appSettings.DataFileDirectory, folder) : appSettings.DataFileDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.EndsWith("csv") || openFileDialog.FileName.EndsWith("xlsx") || openFileDialog.FileName.EndsWith("json") || openFileDialog.FileName.EndsWith("zip") || openFileDialog.FileName.EndsWith("txt"))
                {
                    var process = new Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = openFileDialog.FileName;
                    process.Start();
                }
            }
        }
    }
}
