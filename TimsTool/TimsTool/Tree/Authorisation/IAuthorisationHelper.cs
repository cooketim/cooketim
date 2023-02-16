using Identity;
using Models.ViewModels;
using System.Threading.Tasks;

namespace TimsTool.Tree.Authorisation
{
    public interface IAuthorisationHelper
    {
        Task<bool> Authorise(User signedInUser);
        void SetSecurityPrincipal(LogonModel viewModel);
    }
}