using DataLib;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteNowCommand : ICommand { }

    public class DeleteNowCommand : DeleteNowCommandBase, IDeleteNowCommand
    {
        private ITreeModel treeModel;
        public DeleteNowCommand(ITreeModel treeModel) : base(treeModel)
        {
            this.treeModel = treeModel;
        }

        protected override void Execute(NowTreeViewModel selectedItem)
        {
            int index = -1;
            index = treeModel.AllNowsTreeViewModel.NowsDraft.IndexOf(selectedItem);

            //remove now from the view models
            treeModel.AllNowsTreeViewModel.Nows.RemoveAll(x => x.NowViewModel.UUID == selectedItem.NowViewModel.UUID);
            treeModel.AllNowsTreeViewModel.NowsDraft.RemoveAll(x => x.NowViewModel.UUID == selectedItem.NowViewModel.UUID);
            treeModel.AllNowsViewModel.Nows.RemoveAll(x => x.UUID == selectedItem.NowViewModel.UUID);

            //select the next node
            if (treeModel.AllNowsTreeViewModel.NowsDraft.Count > 0)
            {
                if (index > 0 && treeModel.AllNowsTreeViewModel.NowsDraft.Count - 1 <= index)
                {
                    index = index - 1;
                }
                treeModel.AllNowsTreeViewModel.NowsDraft[index].IsSelected = true;
            }
        }
    }
}
