using Models.ViewModels;
using System;
using DataLib;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Net;
using System.IO;
using Serilog;
using NowsRequestDataLib;
using Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Models.Commands
{
    public interface ISampleNowCommand : ICommand { }

    public class SampleNowCommand : ISampleNowCommand
    {
        private ITreeModel treeModel;
        private IAppSettings appSettings;
        public SampleNowCommand(IAppSettings appSettings, ITreeModel treeModel)
        {
            this.appSettings = appSettings;
            this.treeModel = treeModel;
        }

        public JSchema json_schema = null;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            // I intentionally left these empty because
            // this command never raises the event, and
            // not using the WeakEvent pattern here can
            // cause memory leaks.  WeakEvent pattern is
            // not simple to implement, so why bother.
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            //Determine the selected item
            var selectedNow = treeModel.SelectedItem as NowTreeViewModel;

            //determine the strategy for application nows, optional data and number of nows to be selected
            var applicationRequest = parameter as ApplicationRequestViewModel;
            if (applicationRequest != null)
            {
                SampleNow(selectedNow.NowViewModel.Now, true, "mags", applicationRequest.WithDebug, false, applicationRequest);
                SampleNow(selectedNow.NowViewModel.Now, true, "crown", applicationRequest.WithDebug, false, applicationRequest);
            }
            else
            { 
                var args = (string)parameter;


                var parts = args.Split(',');
                if (parts.Length == 1)
                {
                    if (parts[0].ToLowerInvariant() == "all")
                    {
                        SampleAllNows();
                    }

                    if (parts[0].ToLowerInvariant() == "alllatest")
                    {
                        SampleAllNowsLatest();
                    }
                    return;
                }

                //includeAllOptionalData = true,debug = false,pdf = false, application = true
                if (parts.Length >= 3)
                {
                    var includeAllOptionalData = false;
                    var debug = false;
                    var pdf = false;

                    //process each part
                    foreach (var part in parts)
                    {
                        var subparts = part.Split('=');
                        if (subparts.Length == 2)
                        {
                            if (subparts[0] == "includeAllOptionalData")
                            {
                                includeAllOptionalData = bool.Parse(subparts[1]);
                            }

                            if (subparts[0] == "debug")
                            {
                                debug = bool.Parse(subparts[1]);
                            }

                            if (subparts[0] == "pdf")
                            {
                                pdf = bool.Parse(subparts[1]);
                            }
                        }
                    }                  

                    //Sample the now
                    if (includeAllOptionalData)
                    {
                        //sample both Mags and Crown
                        SampleNow(selectedNow.NowViewModel.Now, true, "mags", debug, pdf, null);
                        SampleNow(selectedNow.NowViewModel.Now, true, "crown", debug, pdf, null);
                    }
                    else
                    {
                        //random sampling
                        SampleNow(selectedNow.NowViewModel.Now, false, null, debug, pdf, null);
                    }
                }
            }     
        }

        private void SampleAllNowsLatest()
        {
            var latestDate = treeModel.AllData.Nows.Where(x => x.DeletedDate == null && !x.IsEDT).Max(x => x.CreatedDate);
            foreach (var now in treeModel.AllData.Nows.Where(x => x.DeletedDate == null && 
                                                !x.IsEDT && 
                                                //!x.Name.ToLowerInvariant().Contains("attachment of earnings") &&
                                                x.CreatedDate.Date == latestDate.Date)
                )
            {
                //sample both mags and crown
                SampleNow(now, true, "mags", false, false, null);
                SampleNow(now, true, "crown", false, false, null);
            }
        }

        private void SampleAllNows()
        {
            foreach(var now in treeModel.AllData.Nows.Where(x=>x.DeletedDate == null && !x.IsEDT 
            //&& !x.Name.ToLowerInvariant().Contains("attachment of earnings")
            ))
            {
                //sample both mags and crown
                SampleNow(now, true, "mags", false, false, null);
                SampleNow(now, true, "crown", false, false, null);
            }
        }

        private void SampleNow(Now now, bool includeAllOptionalData, string jurisdiction, bool withHearingOutput, bool withPDF, ApplicationRequestViewModel applicationVM)
        {
            //for debugging with fiddler
            //WebRequest request = WebRequest.Create("http://DESKTOP-42O184N:8090/getDocument");
            //WebRequest request = WebRequest.Create("http://DESKTOP-42O184N:7070/generate");
            //request.Proxy = new WebProxy("127.0.0.1", 8888);
            try
            {
                //generate sample data
                var generator = new NowDocumentRequestGenerator();
                var applnReq = applicationVM == null ? null :
                    new ApplicationRequest(
                        applicationVM.IsLinkedApplication, applicationVM.IsLiveProceedings, applicationVM.IsHeardWithOtherCases, applicationVM.IsResentenceOrActivation, applicationVM.WithDebug);
                
                var nowData = generator.GetNowRequestData(now, treeModel.AllData.FixedLists, treeModel.AllData.NowSubscriptions, includeAllOptionalData, jurisdiction, treeModel.AllData.ResultDefinitions, treeModel.AllData.NowRequirements, applnReq);

                JsonSerializer serialiser = new JsonSerializer();
                serialiser.NullValueHandling = NullValueHandling.Ignore;
                serialiser.Formatting = Formatting.Indented;

                // Output the sample data
                string fileNowRequest = null;

                if (nowData.HearingResults != null && withHearingOutput)
                {
                    WriteJsonRequest(now.IsEDT, nowData.HearingResults, nowData.HearingResultsFileName, nowData.HearingResultsFileNameSuffix, serialiser);
                }

                if (nowData.NowDocumentRequest != null)
                {
                    fileNowRequest = WriteJsonRequest(now.IsEDT, nowData.NowDocumentRequest, nowData.NowDocumentRequestFileName, nowData.NowDocumentRequestFileNameSuffix, serialiser);

                    //validate the doc request
                    //ValidateNowDocumentRequest(fileNowRequest, nowData.NowDocumentRequestErrorsFileName);
                }

                if (withPDF)
                {
                    //stream back the data for posting
                    var data = File.ReadAllText(fileNowRequest);

                    //Process the document request content
                    // Create a request using a URL that can receive a post.   
                    WebRequest request = WebRequest.Create("http://localhost:8090/getDocument");

                    // Set the Method property of the request to POST.  
                    request.Method = "POST";

                    //format the post data
                    var postData = string.Format("templateName={0}&subTemplateName={1}&nowContent={2}",
                                          now.TemplateName,
                                          now.SubTemplateName,
                                          data);

                    // Convert the post data into a byte array.  
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    //new spring boot app
                    //byte[] byteArray = Encoding.UTF8.GetBytes(nowData.DocmosisNowRequest251Json);

                    // Set the ContentType property of the WebRequest.  
                    request.ContentType = "application/json";
                    // Set the ContentLength property of the WebRequest.  
                    request.ContentLength = byteArray.Length;

                    // Get the request stream.  
                    Stream dataStream = request.GetRequestStream();
                    // Write the data to the request stream.  
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    // Close the Stream object.  
                    dataStream.Close();

                    // Post the request & get the response.  
                    WebResponse response = request.GetResponse();

                    // Output the response as PDF
                    WritePDFResponse(response, now, nowData.HearingResults.IsCrown ? "CROWN" : "MAGS", includeAllOptionalData);

                    // Close the response.  
                    response.Close();
                }
            }
            catch (WebException we)
            {
                Log.Error(we, "Web exception failure generating sample NOW {0}", now.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate sample NOW {0}", now.Name);
            }
        }

        private void ValidateNowDocumentRequest(string sourceDatafilePath, string errorFileName)
        {
            if (json_schema == null)
            {
                //load resolver
                LoadSchema();
            }            

            //load the data
            string data = File.ReadAllText(sourceDatafilePath);
            var model = JObject.Parse(data);

            //validate
            IList<string> messages;
            if (!model.IsValid(json_schema, out messages))
            {
                //do something with the messages
                using (var sw = File.CreateText(errorFileName))
                {
                    foreach(var message in messages)
                    {
                        sw.WriteLine(message);
                    }

                    sw.Flush();
                }                    
            }
        }

        private void LoadSchema()
        {
            var dataFileDirectory = appSettings.DataFileDirectory;
            var globalSchemasPath = Path.Combine(dataFileDirectory, "JSONSchema", "global");
            var nowDocumentSchemasPath = Path.Combine(globalSchemasPath, "nowDocument");
            var definitionsFilePath = Path.Combine(globalSchemasPath, "courtsDefinitions.json");
            var rootFilePath = Path.Combine(nowDocumentSchemasPath, "now-document-request.json");

            //load the schema
            var root = File.ReadAllText(rootFilePath);

            //create a schema resolver
            var resolver = new JSchemaPreloadedResolver();
            var filePaths = Directory.GetFiles(nowDocumentSchemasPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (var filePath in filePaths)
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name.ToLowerInvariant();

                if (fileName != "now-document-request.json")
                {
                    var uri = string.Format(@"http://justice.gov.uk/core/courts/nowdocument/{0}", fileName);
                    using (var fs = File.Open(filePath, FileMode.Open))
                    {
                        resolver.Add(new Uri(uri), fs);
                    }
                }
            }

            using (var fs = File.Open(definitionsFilePath, FileMode.Open))
            {
                resolver.Add(new Uri("http://justice.gov.uk/core/courts/courtsDefinitions.json"), fs);
            }

            json_schema = JSchema.Parse(root, resolver);
        }

        private string WriteJsonRequest(bool isEDT, object content, string fileName, string fileNameSuffix, JsonSerializer serialiser)
        {
            string filePath = GetFilePathForOutput(isEDT, ref fileName, fileNameSuffix);

            Log.Information("Writing to file: {0}.  Path is {1} characters long.", filePath, filePath.Length);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;
                        serialiser.Serialize(writer, content);
                        writer.Flush();
                    }
                }
            }

            return filePath;
        }

        private string GetFilePathForOutput(bool isEDT, ref string fileName, string fileNameSuffix)
        {
            var dataFileDirectory = appSettings.DataFileDirectory;
            if (!Directory.Exists(dataFileDirectory))
            {
                Directory.CreateDirectory(dataFileDirectory);
            }

            var directoryPath = Path.Combine(dataFileDirectory, isEDT ? "edtContent" : "nowContent");


            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, fileName + fileNameSuffix);
            if (filePath.Length > 260)
            {
                var noCharsToRemove = filePath.Length - 260;
                fileName = fileName.Substring(0, fileName.Length - noCharsToRemove);
                filePath = Path.Combine(directoryPath, fileName + fileNameSuffix);
            }

            return filePath;
        }

        private void WritePDFResponse(WebResponse response, Now now, string jurisdiction, bool includeAllOptionalData)
        {
            var dataFileDirectory = appSettings.DataFileDirectory;
            if (!Directory.Exists(dataFileDirectory))
            {
                Directory.CreateDirectory(dataFileDirectory);
            }

            var directoryPath = Path.Combine(dataFileDirectory, "nowContent");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var fileName = string.Format("{0}_{1}_sample_{2}_{3}.pdf", now.Name.Replace("/", "_").Replace("/", "_"), DateTime.Now.ToString("yyyy-MM-dd"), jurisdiction, includeAllOptionalData ? "AllOptionalData" : "SampledOptionalData");
            var filePath = Path.Combine(directoryPath, fileName);

            using (var fs = File.Create(filePath))
            {
                response.GetResponseStream().CopyTo(fs);
            }
        }
    }
}
