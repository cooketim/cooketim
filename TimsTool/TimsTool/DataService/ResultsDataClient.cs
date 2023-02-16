using Configuration;
using DataLib;
using Identity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TimsTool.DataService
{
    public class ResultsDataClient : IResultsDataClient
    {
        // the web api server address 
        private readonly string baseURL;
        private const string apiKey = "3WJxMDeTfrNmqjnleosnpOtrVKtB6MsJ6BE3HIrADVs3tQQaGVNrRYvP4Wl82hfB2c4IWXOtwWRoXhoem4LdgNZggMd7MUTAk2oSUnVFnWEoF2KYG47MPzRoxtkFYO7c";
        public ResultsDataClient(IAppSettings appSettings)
        {
            if (appSettings.IsTest)
            {
                baseURL = appSettings.TestResultsDataServiceBaseURI;
            }
            else
            {
                baseURL = appSettings.ProdResultsDataServiceBaseURI;
            }
        }

        public async Task<User> GetCheckedOutUserAsync()
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                var url = ((int)BlobTypeFileName.CheckoutData).ToString();
                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                // deserialize to user 
                var ms = await response.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    return JsonConvert.DeserializeObject<User>(sr.ReadToEnd());
                }
            }
        }

        public async Task<AllUsers> GetAuthorisedUsersAsync()
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                var url = ((int)BlobTypeFileName.AuthorisedUsers).ToString();
                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                // deserialize to a list of users
                var ms = await response.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    return JsonConvert.DeserializeObject<AllUsers>(sr.ReadToEnd());
                }
            }
        }

        public async Task<IdMap> GetIdMapAsync()
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                var url = ((int)BlobTypeFileName.IdMap).ToString();
                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                // deserialize to a list of users
                var ms = await response.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    return JsonConvert.DeserializeObject<IdMap>(sr.ReadToEnd());
                }
            }
        }

        public async Task SaveIdMapAsync(IdMap map)
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                var requestJson = JsonConvert.SerializeObject(map);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("saveidmapasync", requestContent);

                response.EnsureSuccessStatusCode();
            }
        }

        public async Task GetLatestDataAsync(string dataPath)
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                var url = ((int)BlobTypeFileName.TreeData).ToString();
                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                // write stream to disk
                var fileName = BlobTypeFileName.TreeData.GetDescription();
                var filePath = Path.Combine(dataPath, fileName);
                var ms = await response.Content.ReadAsStreamAsync();
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    ms.Position = 0;
                    await ms.CopyToAsync(fs);
                }
            }
            return;
        }

        public async Task<User> CheckoutAsync(User requestUser, string dataPath)
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                //update the checked out user                
                var requestJson = JsonConvert.SerializeObject(requestUser);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("checkoutasync", requestContent);

                response.EnsureSuccessStatusCode();

                // deserialize to user
                User checkedOutUser = null;
                var ms = await response.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    checkedOutUser = JsonConvert.DeserializeObject<User>(sr.ReadToEnd());
                }

                //ensure that the checked out user matches the requested checkout
                if (!string.Equals(checkedOutUser.Email, requestUser.Email, StringComparison.InvariantCulture))
                {
                    //not equal, return the existing checked out user
                    return checkedOutUser;
                }
            }

            //get the latest data
            await GetLatestDataAsync(dataPath);

            return requestUser;
        }

        public async Task<User> CancelCheckoutAsync(User requestUser, string dataPath)
        {
            User newCheckedOutUser = null;

            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                //cancel the checkout status for the current checked out user
                var requestJson = JsonConvert.SerializeObject(requestUser);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("cancelcheckoutasync", requestContent);

                response.EnsureSuccessStatusCode();

                // deserialize to user                
                var ms = await response.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    newCheckedOutUser = JsonConvert.DeserializeObject<User>(sr.ReadToEnd());
                }
            }

            //get the latest data to overwrite any local changes that have been cancelled
            await GetLatestDataAsync(dataPath);

            return newCheckedOutUser;
        }

        public async Task<UploadDataResponse> CheckinAsync(User requestUser, string fullFilePath)
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                HttpResponseMessage response;
                using (var fs = File.OpenRead(fullFilePath))
                {
                    var requestContent = CreateMultipartContent(requestUser, fullFilePath, fs);
                    response = await client.PostAsync("checkinasync", requestContent);
                }
                response.EnsureSuccessStatusCode();

                // deserialize to user
                var ms = await response.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    return JsonConvert.DeserializeObject<UploadDataResponse>(sr.ReadToEnd());
                }
            }
        }

        public async Task<UploadDataResponse> SaveDataAsync(User requestUser, string fullFilePath)
        {
            // make an API request 
            using (HttpClient client = BuildHttpClient())
            {
                HttpResponseMessage response;
                using (var fs = File.OpenRead(fullFilePath))
                {
                    var requestContent = CreateMultipartContent(requestUser, fullFilePath, fs);
                    response = await client.PostAsync("savedataasync", requestContent);
                }
                response.EnsureSuccessStatusCode();

                // deserialize to user
                var ms = await response.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    return JsonConvert.DeserializeObject<UploadDataResponse>(sr.ReadToEnd());
                }
            }
        }

        private MultipartFormDataContent CreateMultipartContent(User requestUser, string fullFilePath, Stream stream)
        {
            var fileInfo = new FileInfo(fullFilePath);
            return new MultipartFormDataContent
                {
                    //payload -- no need to send the user roles, only name and email
                    { new  StringContent(requestUser.Username),"RequestUser.Username" },
                    { new  StringContent(requestUser.Email),"RequestUser.Email" },

                    //file
                    { new StreamContent(stream),"SourceFile",fileInfo.Name },
                };
        }

        private HttpClient BuildHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseURL);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("XApiKey", apiKey);
            return httpClient;
        }
    }
}
