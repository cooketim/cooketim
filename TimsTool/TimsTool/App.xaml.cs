using System;
using System.Data;
using System.Linq;
using System.Windows;
using Configuration;
using System.IO;
using System.Net;
using System.Security.Principal;
using Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using Newtonsoft.Json;
using TimsTool.Tree.Authorisation;
using Models.ViewModels;
using System.Xml.Linq;
using TimsTool.DataService;

namespace TimsTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            //adjust app settings to the local environment
            var settings = SetAppSettings();

            //Configure Logging
            ConfigureLogging(settings);

            //set up the dependency injection
            SetConfiguration(settings.BaseFileDirectory);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            ConfigureServices(serviceCollection, settings);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);

            var mainWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            mainWindow.Show();            

            //Set some service settings
            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
        }

        private void ConfigureLogging(IAppSettings settings)
        {
            //Set up the logging
            Directory.CreateDirectory(settings.DataFileDirectory);
            var filePath = Path.Combine(settings.DataFileDirectory, "resultsTree.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.File($@"{filePath}", rollingInterval: RollingInterval.Month)
                .CreateLogger();

            Log.Information("Started ResultsTree");
        }

        private void SetConfiguration(string baseFileDirectory)
        {          
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(baseFileDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        private IAppSettings SetAppSettings()
        {
            //adjust the appsettings to be specific for the runtime environment
            var baseFileDirectory = Directory.GetCurrentDirectory();
            string json = File.ReadAllText("appsettings.json");
            var settings = JsonConvert.DeserializeObject<AppSettingsWrapper>(json);

            settings.AppSettings.BaseFileDirectory = baseFileDirectory;
            settings.AppSettings.DataFileDirectory = Path.Combine(baseFileDirectory, settings.AppSettings.DataFileFolder);
            settings.AppSettings.DataFilePath = Path.Combine(settings.AppSettings.DataFileDirectory, settings.AppSettings.DataFileName);
            var reportsDirectory = Path.Combine(settings.AppSettings.DataFileDirectory, "Reports");
            settings.AppSettings.AppVersion = GetMsixPackageVersion(baseFileDirectory);

            if(settings.AppSettings.IsTest)
            {
                settings.AppSettings.AppVersion = String.Format("{0} - TEST VERSION", GetMsixPackageVersion(baseFileDirectory));
            }
            else
            {
                settings.AppSettings.AppVersion = GetMsixPackageVersion(baseFileDirectory);
            }

            string newSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("appsettings.json", newSettings);

            //ensure that there is a backup and a reports directory
            Directory.CreateDirectory(reportsDirectory);

            return settings.AppSettings;
        }

        private string GetMsixPackageVersion(string baseFileDirectory)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var manifestName = assembly.ManifestModule.Name.Replace("dll", "exe.manifest");
            var manifestPath = Path.Combine(baseFileDirectory, manifestName);
            if (File.Exists(manifestPath))
            {
                var xDoc = XDocument.Load(manifestPath);
                var versionElement = xDoc.Descendants().First(e => e.Name.LocalName == "assemblyIdentity");
                var version = versionElement.Attributes().First(a => a.Name.LocalName == "version").Value.ToString();
                return string.Format("Published Version - {0}", versionElement.Attributes().First(a => a.Name.LocalName == "version").Value);
            }

            return "Published Version - Local";
        }

        private void ConfigureServices(IServiceCollection services, IAppSettings settings)
        {
            //Principal
            var customPrincipal = GetResultsTreePricipal(); 
            services.AddSingleton<IPrincipal>(customPrincipal);

            // ...
            var appSettings = Configuration.GetSection(nameof(AppSettings));
            services.Configure<AppSettings>(appSettings);
            services.AddSingleton<IAppSettings>(settings);

            services.AddSingleton<IAuthorisationHelper, AuthorisationHelper>();

            services.AddSingleton<IResultsDataClient, ResultsDataClient>();

            //Commands
            //services.AddSingleton<IAddFixedListCommand, AddFixedListCommand>();
            //services.AddSingleton<IAddNewChildNowSubscriptionCommand, AddNewChildNowSubscriptionCommand>();
            //services.AddSingleton<IAddNewChildResultDefinitionCommand, AddNewChildResultDefinitionCommand>();
            //services.AddSingleton<IAddNewEDTCommand, AddNewEDTCommand>();
            //services.AddSingleton<IAddNewNowCommand, AddNewNowCommand>();
            //services.AddSingleton<IAddNewNowSubscriptionCommand, AddNewNowSubscriptionCommand>();
            //services.AddSingleton<IAddNewResultDefinitionCommand, AddNewResultDefinitionCommand>();
            //services.AddSingleton<IAddNewResultDefinitionWordGroupCommand, AddNewResultDefinitionWordGroupCommand>();
            //services.AddSingleton<IAddNewResultPromptCommand, AddNewResultPromptCommand>();
            //services.AddSingleton<IAddNewResultPromptWordGroupCommand, AddNewResultPromptWordGroupCommand>();
            //services.AddSingleton<IAddNowRequirementTextValueCommand, AddNowRequirementTextValueCommand>();
            //services.AddSingleton<IAddNowTextValueCommand, AddNowTextValueCommand>();
            //services.AddSingleton<ICopyEDTCommand, CopyEDTCommand>();
            //services.AddSingleton<ICopyFixedListCommand, CopyFixedListCommand>();
            //services.AddSingleton<ICopyNowCommand, CopyNowCommand>();
            //services.AddSingleton<ICopyResultDefinitionCommand, CopyResultDefinitionCommand>();
            //services.AddSingleton<ICopyResultDefinitionWordGroupCommand, CopyResultDefinitionWordGroupCommand>();
            //services.AddSingleton<ICopyResultPromptCommand, CopyResultPromptCommand>();
            //services.AddSingleton<ICopyResultPromptWordGroupCommand, CopyResultPromptWordGroupCommand>();
            //services.AddSingleton<IDataPatchCommand, DataPatchCommand>();
            //services.AddSingleton<IDeleteEDTCommand, DeleteEDTCommand>();
            //services.AddSingleton<IDeleteFixedListCommand, DeleteFixedListCommand>();
            //services.AddSingleton<IDeleteNowCommand, DeleteNowCommand>();
            //services.AddSingleton<IDeleteNowRequirementCommand, DeleteNowRequirementCommand>();
            //services.AddSingleton<IDeleteNowRequirementTextValueCommand, DeleteNowRequirementTextValueCommand>();
            //services.AddSingleton<DeleteNowSubscriptionCommand, DeleteNowSubscriptionCommand>();
            //services.AddSingleton<IDeleteNowTextValueCommand, DeleteNowTextValueCommand>();
            //services.AddSingleton<IDeleteResultDefinitionCommand, DeleteResultDefinitionCommand>();
            //services.AddSingleton<IDeleteResultDefinitionWordGroupCommand, DeleteResultDefinitionWordGroupCommand>();
            //services.AddSingleton<IDeleteResultPromptCommand, DeleteResultPromptCommand>();
            //services.AddSingleton<IDeleteResultPromptWordGroupCommand, DeleteResultPromptWordGroupCommand>();
            //services.AddSingleton<IDraftFixedListCommand, DraftFixedListCommand>();
            //services.AddSingleton<IDraftNowCommand, DraftNowCommand>();
            //services.AddSingleton<IDraftNowSubscriptionCommand, DraftNowSubscriptionCommand>();
            //services.AddSingleton<IDraftResultDefinitionCommand, DraftResultDefinitionCommand>();
            //services.AddSingleton<IDraftResultDefinitionWordGroupCommand, DraftResultDefinitionWordGroupCommand>();
            //services.AddSingleton<IDraftResultPromptCommand, DraftResultPromptCommand>();
            //services.AddSingleton<IDraftResultPromptWordGroupCommand, DraftResultPromptWordGroupCommand>();
            //services.AddSingleton<IGenerateReportCommand, GenerateReportCommand>();
            //services.AddSingleton<IMergeLocalChangesCommand, MergeLocalChangesCommand>();
            //services.AddSingleton<IPasteFixedListCommand, PasteFixedListCommand>();
            //services.AddSingleton<IPasteIncludedExcludedSubscriptionCommand, PasteIncludedExcludedSubscriptionCommand>();
            //services.AddSingleton<IPasteResultDefinitionChildCommand, PasteResultDefinitionChildCommand>();
            //services.AddSingleton<IPasteResultDefinitionCommand, PasteResultDefinitionCommand>();
            //services.AddSingleton<IPasteResultDefinitionWordGroupCommand, PasteResultDefinitionWordGroupCommand>();
            //services.AddSingleton<IPasteResultPromptCommand, PasteResultPromptCommand>();
            //services.AddSingleton<IPasteResultPromptWordGroupCommand, PasteResultPromptWordGroupCommand>();
            //services.AddSingleton<IPublishDraftsCommand, PublishDraftsCommand>();
            //services.AddSingleton<IResequenceCommand, ResequenceCommand>();
            //services.AddSingleton<ISampleNowCommand, SampleNowCommand>();
            //services.AddSingleton<ISaveCommand, SaveCommand>();
            //services.AddSingleton<ISaveToFileCommand, SaveToFileCommand>();
            //services.AddSingleton<ISearchItemCommand, SearchItemCommand>();
            //services.AddSingleton<ISetSubscriptionFromResultCommand, SetSubscriptionFromResultCommand>();
            //services.AddSingleton<IUndeleteFixedListCommand, UndeleteFixedListCommand>();
            //services.AddSingleton<IUndeleteResultDefinitionCommand, UndeleteResultDefinitionCommand>();
            //services.AddSingleton<IUndeleteResultPromptCommand, UndeleteResultPromptCommand>();
            //services.AddSingleton<IUndeleteResultPromptWordGroupCommand, UndeleteResultPromptWordGroupCommand>();
            //services.AddSingleton<IWelshDataImportProvider, WelshDataImportProvider>();

            //View Models
            services.AddSingleton<ITreeModel, TreeModel>();
            services.AddSingleton<IReportViewModel, ReportViewModel>();

            //User Controls
            //services.AddSingleton<IEdtControl, EdtControl>();
            //services.AddSingleton<IEdtSubscriptionControl, EdtSubscriptionControl>();
            //services.AddSingleton<IEdtSubscriptionControl, EdtSubscriptionControl>();
            //services.AddSingleton<IEdtControl, EdtControl>();
            //services.AddSingleton<IInformantRegisterControl, InformantRegisterControl>();
            //services.AddSingleton<IInformantRegisterControl, InformantRegisterControl>();
            //services.AddSingleton<INowsControl, NowsControl>();
            //services.AddSingleton<INowSubscriptionsControl, NowSubscriptionsControl>();
            //services.AddSingleton<INowSubscriptionControl, NowSubscriptionControl>();
            //services.AddSingleton<INowControl, NowControl>();
            //services.AddSingleton<IPrisonCourtRegisterSubscriptionsControl, PrisonCourtRegisterSubscriptionsControl>();
            //services.AddSingleton<IPrisonCourtRegisterSubscriptionControl, PrisonCourtRegisterSubscriptionControl>();
            //services.AddSingleton<IResultDefinitionsControl, ResultDefinitionsControl>();
            //services.AddSingleton<IResultDefinitionControl, ResultDefinitionControl>();

            //Windows
            services.AddTransient(typeof(LoginWindow));
            services.AddTransient(typeof(TreeWindow));
            services.AddTransient(typeof(ReportWindow));
        }

        private ResultsTreePricipal GetResultsTreePricipal()
        {
            //Get the roles of the current identity
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            var groupNames = identity.Groups == null ? null : identity.Groups.Translate(typeof(NTAccount))
                                                        .Select(x => x.Value.ToLowerInvariant())
                                                        .Where(x => x.Contains(@"\") && !x.StartsWith("builtin") && !x.StartsWith("nt authority")).ToList();

            var inHMCTS = groupNames.FirstOrDefault(x => x.Contains(@"hmcts\")) != null;
            if (!identity.IsAuthenticated || !inHMCTS)
            {
                //Create a custom principal with an anonymous identity at startup
                ResultsTreePricipal customPrincipal = new ResultsTreePricipal(new AnonymousIdentity());
                AppDomain.CurrentDomain.SetThreadPrincipal(customPrincipal);
                return customPrincipal;
            }
            else
            {
                //Create a custom principal with the windows identity at startup
                ResultsTreePricipal customPrincipal = new ResultsTreePricipal(identity);
                AppDomain.CurrentDomain.SetThreadPrincipal(customPrincipal);
                return customPrincipal;
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Log.Error(e, "MyHandler caught : " + e.Message);
            Log.Error(e, "Runtime terminating: {0}", args.IsTerminating);
        }
    }
}
