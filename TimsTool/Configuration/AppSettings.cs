namespace Configuration
{
    public class AppSettingsWrapper
    {
        public AppSettings AppSettings { get; set; }
    }
    public class AppSettings : IAppSettings
    {
        public string BaseFileDirectory { get; set; }
        public string DataFileDirectory { get; set; }
        public string DataFilePath { get; set; }
        public string DataFileFolder { get; set; }
        public string DataFileName { get; set; }
        public bool IsTest { get; set; }
        public string TestResultsDataServiceBaseURI { get; set; }
        public string ProdResultsDataServiceBaseURI { get; set; }
        public string LocalResultsDataServiceBaseURI { get; set; }
        public string AppVersion { get; set; }
    }
}
