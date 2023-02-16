namespace Configuration
{
    public interface IAppSettings
    {
        string BaseFileDirectory { get; set; }
        string DataFileDirectory { get; set; }
        string DataFileFolder { get; set; }
        string DataFileName { get; set; }
        string DataFilePath { get; set; }
        bool IsTest { get; set; }
        string TestResultsDataServiceBaseURI { get; set; }
        string ProdResultsDataServiceBaseURI { get; set; }
        string LocalResultsDataServiceBaseURI { get; set; }
        string AppVersion { get; set; }
    }
}