using Models.ViewModels;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IAddNewEDTCommand : ICommand { }

    public class AddNewEDTCommand : AddNewNowCommand, IAddNewEDTCommand
    {
        public AddNewEDTCommand(ITreeModel treeModel) : base(treeModel) {}

        protected override void Execute(PublicationTagsModel ptm)
        {
            //make a new item and select it
            var newItem = MakeAndStoreNowTreeViewModel(true, ptm);
            //reset any copied item
            newItem.PendingCopyModel = null;

            newItem.IsSelected = true;
        }
    }
}
