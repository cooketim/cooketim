using DataLib;
using Models.ViewModels;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteEDTCommand : ICommand { }

    public class DeleteEDTCommand : DeleteNowCommandBase, IDeleteEDTCommand
    {
        private ITreeModel treeModel;
        public DeleteEDTCommand(ITreeModel treeModel): base(treeModel)
        {
            this.treeModel = treeModel;
        }
        protected override void Execute(NowTreeViewModel selectedItem)
        {
            int index = -1;
            index = treeModel.AllEDTsTreeViewModel.EdtsDraft.IndexOf(selectedItem);

            //remove edt from the view models
            treeModel.AllEDTsTreeViewModel.EDTs.RemoveAll(x => x.NowViewModel.UUID == selectedItem.NowViewModel.UUID);
            treeModel.AllEDTsTreeViewModel.EdtsDraft.RemoveAll(x => x.NowViewModel.UUID == selectedItem.NowViewModel.UUID);
            treeModel.AllEDTsViewModel.EDTs.RemoveAll(x => x.UUID == selectedItem.NowViewModel.UUID);

            //select the next node
            if (treeModel.AllEDTsTreeViewModel.EdtsDraft.Count > 0)
            {
                if (index > 0 && treeModel.AllEDTsTreeViewModel.EdtsDraft.Count - 1 <= index)
                {
                    index = index - 1;
                }
                treeModel.AllEDTsTreeViewModel.EdtsDraft[index].IsSelected = true;
            }
        }
    }
}
