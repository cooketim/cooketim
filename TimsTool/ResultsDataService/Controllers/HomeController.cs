using Azure.Storage.Blobs.Models;
using DataLib;
using Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ResultsDataService.Controllers
{
    [ApiController]
    //[Route("api/data/v1")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;

        private readonly IBackupBlobService backupBlobService;
        private readonly ICoreBlobService coreBlobService;
        private readonly IConfiguration configuration;
        private readonly IServiceScopeFactory serviceProvider;

        private const string treeBackupName = "tree_Backup_{0}.bin";
        private const string backupDateFormat = "yyyy-MM-dd_HH-mm";

        public HomeController(ILogger<HomeController> logger, 
            IBackupBlobService backupBlobService, 
            ICoreBlobService coreBlobService, 
            IConfiguration configuration,
            IServiceScopeFactory serviceProvider)
        {
            this.logger = logger;
            this.coreBlobService = coreBlobService;
            this.backupBlobService = backupBlobService;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
        }

        [Route("ping")]
        [HttpGet]
        public string ping()
        {
            return "I Am Alive";
        }

        [Route(template: "{fileNameType}")]
        [HttpGet]
        public async Task<IActionResult> GetDataAsync([FromRoute] BlobTypeFileName fileNameType)
        {
            var containerName = configuration.GetValue<string>("AzureContainerName");
            var data = await GetFileAsync(fileNameType, containerName);
            return File(data.Content, data.ContentType);
        }

        private async Task<BlobInfo> GetFileAsync(BlobTypeFileName fileNameType, string containerName)
        {
            var fileName = fileNameType.GetDescription();
            return await coreBlobService.GetBlobAsync(fileName, containerName);
        }


        [Route("saveidmapasync")]
        [HttpPut]
        public async Task<IActionResult> SaveIdMapAsync([FromBody] string contentjson)
        {
            var containerName = configuration.GetValue<string>("AzureContainerName");
            var fileName = BlobTypeFileName.IdMap.GetDescription();
            await coreBlobService.UploadContentBlobAsync(contentjson, fileName, containerName);
            return Ok();
        }

        [Route("uploadfileasync")]
        [HttpPost]
        public async Task<IActionResult> UploadFileAsync([FromForm] UploadFileRequest uploadFileRequest)
        {
            // Check if the request contains multipart/form-data.
            if (uploadFileRequest.SourceFile == null)
            {
                return new UnsupportedMediaTypeResult();
            }

            if (uploadFileRequest.SourceFile.Length == 0)
            {
                return NotFound("Failed to upload file. No file data found");
            }

            await UploadFileAsync(uploadFileRequest.BlobTypeFileName, uploadFileRequest.SourceFile);
            return Ok(new UploadFileResponse() { Status = "Success", Length = uploadFileRequest.SourceFile.Length, Name = uploadFileRequest.SourceFile.FileName });
        }

        [Route("cleanbackupsasync")]
        [HttpGet]
        public async Task<IActionResult> CleanBackupsAsync()
        {
            await DeleteIntradayBackupsAsync();
            return Ok();
        }

        private async Task UploadFileAsync(BlobTypeFileName blobTypeFileName, IFormFile sourceFile)
        {
            //back up tree data when required in a fire and forget process 
            var isTest = configuration.GetValue<bool>("IsTest");

            var containerName = configuration.GetValue<string>("AzureContainerName");

            //if (!isTest && blobTypeFileName == BlobTypeFileName.TreeData)
            if (blobTypeFileName == BlobTypeFileName.TreeData)
            {
                var ms = new MemoryStream();
                await sourceFile.CopyToAsync(ms);
                _ = Task.Run(async () =>
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                            //var backupClient = scope.ServiceProvider.GetRequiredService<BackupBlobClient>();
                            var backupService = scope.ServiceProvider.GetRequiredService<IBackupBlobService>();
                            await WriteBackupAsync(ms, containerName);
                        }
                    });
            }

            //upload the file
            var fileName = blobTypeFileName.GetDescription();
            using (var ms = new MemoryStream())
            {
                await sourceFile.CopyToAsync(ms);
                await coreBlobService.UploadFileBlobAsync(ms, fileName, containerName);
            }
        }

        /// <summary>
        /// Designed to be invoked on a background thread
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private async Task WriteBackupAsync(MemoryStream ms, string containerName)
        {
            var fileName = string.Format(treeBackupName, DateTime.Now.ToString(backupDateFormat));

            try
            {
                await backupBlobService.UploadFileBlobAsync(ms, fileName, containerName);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to write backup", ex);
            }
            finally
            {
                ms.Close();
            }

            try
            {
                //Delete backups that have aged
                await DeleteIntradayBackupsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to delete intraday backups", ex);
            }
        }
        private async Task DeleteIntradayBackupsAsync()
        {
            var containerName = configuration.GetValue<string>("AzureContainerName");
            var blobItems = new List<BlobItem>(await backupBlobService.ListBlobsAsync(containerName));

            //build a collection of date blob name pairs
            var blobNamesByDate = from blobItem in blobItems select new { Name = blobItem.Name, UTCAge = blobItem.Properties.LastModified.Value.DateTime };

            var agedStartDateTime = DateTime.UtcNow.AddDays(configuration.GetValue<int>("IntradayRetentionDays")*-1);
            var maxRetentionAge = DateTime.UtcNow.AddDays(configuration.GetValue<int>("MaxRetentionDays") * -1);

            var toDelete = blobNamesByDate.Where(x => x.UTCAge < maxRetentionAge).ToList();

            // first deal with aged blobs that are older than the maximum retention period - note this should not happen because the container in azure should be configured to mange this house keeping
            foreach (var item in blobNamesByDate.Where(x => x.UTCAge < maxRetentionAge))
            {
                await backupBlobService.DeleteBlobAsync(item.Name, containerName);
            }

            //now deal with any intraday blobs that have reached their retention age
            var agedBlobs = blobNamesByDate.Where(x=>x.UTCAge < agedStartDateTime && x.UTCAge > maxRetentionAge).GroupBy(x=>x.UTCAge.Date);

            foreach (var agedGroup in agedBlobs)
            {
                foreach (var item in agedGroup.OrderByDescending(x => x.UTCAge).Skip(1))
                {
                    await backupBlobService.DeleteBlobAsync(item.Name, containerName);
                }
            }
        }

        [Route("checkoutasync")]
        [HttpPut]
        public async Task<IActionResult> CheckOutAsync([FromBody] User requestUser)
        {
            // get the current check out to ensure that the file is not already checked out
            var containerName = configuration.GetValue<string>("AzureContainerName");
            var data = await GetFileAsync(BlobTypeFileName.CheckoutData, containerName);
            User checkedOutUser = null;
            using (var reader = new StreamReader(data.Content))
            {
                var json = await reader.ReadToEndAsync();
                checkedOutUser = JsonConvert.DeserializeObject<User>(json);
            }

            if (string.IsNullOrEmpty(checkedOutUser.Email))
            {
                //set the checkout
                var json = JsonConvert.SerializeObject(requestUser);
                var fileName = BlobTypeFileName.CheckoutData.GetDescription();
                await coreBlobService.UploadContentBlobAsync(json, fileName, containerName);
                checkedOutUser = requestUser;
            }

            return Ok(checkedOutUser);
        }

        [Route("cancelcheckoutasync")]
        [HttpPut]
        public async Task<IActionResult> CancelCheckOutAsync([FromBody] User requestUser)
        {
            //get the current check out to ensure that the file checkedout to the checkoutuser
            var containerName = configuration.GetValue<string>("AzureContainerName");
            var data = await GetFileAsync(BlobTypeFileName.CheckoutData, containerName);
            User currentCheckedOutUser = null;
            using (var reader = new StreamReader(data.Content))
            {
                var currentCheckedOutUserJson = await reader.ReadToEndAsync();
                currentCheckedOutUser = JsonConvert.DeserializeObject<User>(currentCheckedOutUserJson);
            }

            //determine if checkout already cancelled
            if(string.IsNullOrEmpty(currentCheckedOutUser.Email))
            {
                logger.LogWarning("Cancel Checkout '{0}', No active checkout is current.", requestUser.Email);
                return Ok(currentCheckedOutUser);
            }

            //ensure that the current checked out user is the checkedOutUser
            if (!string.Equals(requestUser.Email, currentCheckedOutUser.Email, StringComparison.InvariantCulture))
            {
                //current checked out user is different to the checkoutuser requested
                logger.LogWarning("Cancel Checkout '{0}', Another user '{1}' is checked out.", requestUser.Email, currentCheckedOutUser.Email);
                return Ok(currentCheckedOutUser);
            }

            //cancel the checkout
            var emptyUser = new User(string.Empty, string.Empty, new string[] { });
            var json = JsonConvert.SerializeObject(emptyUser);
            var fileName = BlobTypeFileName.CheckoutData.GetDescription();
            await coreBlobService.UploadContentBlobAsync(json, fileName, containerName);
            return Ok(emptyUser);
        }

        [Route("checkinasync")]
        [HttpPost]
        public async Task<IActionResult> CheckInAsync([FromForm] UploadDataRequest request)
        {
            // Check if the request contains multipart/form-data.
            if (request.SourceFile == null)
            {
                return new UnsupportedMediaTypeResult();
            }

            if (request.SourceFile.Length == 0)
            {
                return NotFound(new UploadDataResponse() { Status = "Failed", Message = "Failed to checkin data. No data found" });
            }

            var containerName = configuration.GetValue<string>("AzureContainerName");

            var checkedOutUser = await SaveDataAsync(request.RequestUser, request.SourceFile);
            if (!string.Equals(checkedOutUser.Email, request.RequestUser.Email, StringComparison.InvariantCulture))
            {
                //current checked out user is different to the checkoutuser requested, abandon saving the data changes as the user is not authorised to save changes
                return new ObjectResult(new UploadDataResponse() { Status = "Forbidden", CheckedoutUser = checkedOutUser, Message = "Not Authorised, requesting user is not the checked out user" }) { StatusCode = 403 };
            }

            //set the checkout to an empty (default) user
            checkedOutUser = new User(string.Empty, string.Empty, new string[] { });
            var json = JsonConvert.SerializeObject(checkedOutUser);
            var fileName = BlobTypeFileName.CheckoutData.GetDescription();
            await coreBlobService.UploadContentBlobAsync(json, fileName, containerName);

            return Ok(new UploadDataResponse() { Status = "Success", CheckedoutUser = checkedOutUser, Length = request.SourceFile.Length, Name = request.SourceFile.FileName });
        }

        private async Task<User> SaveDataAsync(User requestUser, IFormFile sourceFile)
        {
            // get the current check out to ensure that the file is checked out to the requesting user
            var containerName = configuration.GetValue<string>("AzureContainerName");
            var data = await GetFileAsync(BlobTypeFileName.CheckoutData, containerName);
            User checkedOutUser = null;
            using (var reader = new StreamReader(data.Content))
            {
                var json = await reader.ReadToEndAsync();
                checkedOutUser = JsonConvert.DeserializeObject<User>(json);
            }

            if (!string.Equals(checkedOutUser.Email, requestUser.Email, StringComparison.InvariantCulture))
            {
                //current checked out user is different to the checkoutuser requested, abandon saving the data changes
                logger.LogWarning("Save Data Requested By '{0}', Another user '{1}' is checked out, changes not saved", requestUser.Email, checkedOutUser.Email);
                return checkedOutUser;
            }

            //Save the data
            await UploadFileAsync(BlobTypeFileName.TreeData, sourceFile);

            return requestUser;
        }

        [Route("savedataasync")]
        [HttpPost]
        public async Task<IActionResult> SaveDataAsync([FromForm] UploadDataRequest request)
        {
            // Check if the request contains multipart/form-data.
            if (request.SourceFile == null)
            {
                return new UnsupportedMediaTypeResult();
            }

            if (request.SourceFile.Length == 0)
            {
                return NotFound(new UploadDataResponse() { Status = "Failed", Message = "Failed to save data. No data found" });
            }

            var checkedOutUser = await SaveDataAsync(request.RequestUser, request.SourceFile);

            if (!string.Equals(checkedOutUser.Email, request.RequestUser.Email, StringComparison.InvariantCulture))
            {
                //current checked out user is different to the requesting user, abandon saving the data changes as the user is not authorised to save changes
                return new ObjectResult(new UploadDataResponse() { Status = "Forbidden", CheckedoutUser = checkedOutUser, Message = "Not Authorised, requesting user is not the checked out user" }) { StatusCode = 403};
            }

            return Ok(new UploadDataResponse() { Status = "Success", CheckedoutUser = checkedOutUser, Length = request.SourceFile.Length, Name = request.SourceFile.FileName });
        }
    }
}
