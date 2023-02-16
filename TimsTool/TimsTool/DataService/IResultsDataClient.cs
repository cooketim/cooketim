using DataLib;
using Identity;
using System.Threading.Tasks;

namespace TimsTool.DataService
{
    public interface IResultsDataClient
    {
        Task<User> CancelCheckoutAsync(User requestUser, string dataPath);
        Task<UploadDataResponse> CheckinAsync(User requestUser, string fullFilePath);
        Task<User> CheckoutAsync(User requestUser, string dataPath);
        Task<AllUsers> GetAuthorisedUsersAsync();
        Task<IdMap> GetIdMapAsync();
        Task SaveIdMapAsync(IdMap map);
        Task<User> GetCheckedOutUserAsync();
        Task GetLatestDataAsync(string dataPath);
        Task<UploadDataResponse> SaveDataAsync(User requestUser, string fullFilePath);
    }
}