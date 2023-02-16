using DataLib;
using Models.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IAddNewNowCommand : ICommand { }
    public class AddNewNowCommand : IAddNewNowCommand
    {
        private ITreeModel treeModel;
        public AddNewNowCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

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
            var ptm = parameter == null ? null : parameter as PublicationTagsModel;

            if (ptm == null)
            {
                Log.Error("Publication Tags Model not sent for add new NOW");
                return;
            }

            Execute(ptm);
        }

        protected virtual void Execute(PublicationTagsModel ptm)
        {
            //make a new item and select it
            var newItem = MakeAndStoreNowTreeViewModel(false, ptm);
            //reset any copied item
            newItem.PendingCopyModel = null;

            newItem.IsSelected = true;
        }

        protected NowTreeViewModel MakeAndStoreNowTreeViewModel(bool isEDT, PublicationTagsModel ptm)
        {
            //Make a new data item 
            var data = new Now(isEDT);

            //set the publication status
            data.PublishedStatus = PublishedStatus.Draft;
            data.IsNewItem = true;

            if (ptm != null)
            {
                data.PublicationTags = ptm.SelectedPublicationTags.ToList();
            }

            var nowVM = new NowViewModel(treeModel, data, treeModel.AllNowRequirementsViewModel, treeModel.AllResultDefinitionsViewModel);

            //ensure that name is unique
            var duplicateName = false;
            int i = 1;
            do
            {
                duplicateName = isEDT ? treeModel.AllEDTsTreeViewModel.EDTs.FirstOrDefault(x => x.NowViewModel.Name == data.Name) != null
                                      : treeModel.AllNowsTreeViewModel.Nows.FirstOrDefault(x => x.NowViewModel.Name == data.Name) != null;
                if (duplicateName)
                {
                    i++;
                    data.Name = string.Format("New {0} ({1})", isEDT ? "EDT" : "NOW", i);
                }

            } while (duplicateName);

            //sort out the other data store objects
            if (isEDT)
            {
                treeModel.AllEDTsViewModel.EDTs.Add(nowVM);
                treeModel.AllData.Nows.Add(data);
            }
            else
            {
                treeModel.AllNowsViewModel.Nows.Add(nowVM);
                treeModel.AllData.Nows.Add(data);
            }

            //Make new tree model
            var newItem = new NowTreeViewModel(treeModel, nowVM);

            //determine the index of the current selection 
            var selectedNowEdt = GetTreeViewItemForSelectedItem();
            var index = isEDT ? treeModel.AllEDTsTreeViewModel.EdtsDraft.IndexOf(selectedNowEdt)
                              : treeModel.AllNowsTreeViewModel.NowsDraft.IndexOf(selectedNowEdt);
            if (index < 0) { index = 0; }

            //determine the new rank based on the selected item
            if (selectedNowEdt.NowViewModel.Rank.HasValue)
            {
                if (isEDT)
                {
                    if (index < treeModel.AllEDTsTreeViewModel.EdtsDraft.Count() - 1)
                    {
                        var nextItem = treeModel.AllEDTsTreeViewModel.EdtsDraft[index + 1];
                        if (nextItem.NowViewModel.Now.Rank.HasValue)
                        {
                            SetRank(nextItem.NowViewModel, selectedNowEdt.NowViewModel, data);
                        }
                    }
                    else
                    {
                        data.Rank = selectedNowEdt.NowViewModel.Rank.Value + 100;
                    }
                }
                else
                {
                    if (index < treeModel.AllNowsTreeViewModel.NowsDraft.Count() - 1)
                    {
                        var nextItem = treeModel.AllNowsTreeViewModel.NowsDraft[index + 1];
                        if (nextItem.NowViewModel.Now.Rank.HasValue)
                        {
                            SetRank(nextItem.NowViewModel, selectedNowEdt.NowViewModel, data);
                        }
                    }
                    else
                    {
                        data.Rank = selectedNowEdt.NowViewModel.Rank.Value + 100;
                    }
                }
            }

            //add to the tree of root nows or EDTs
            if (isEDT)
            {
                treeModel.AllEDTsTreeViewModel.EDTs.AddSorted(newItem);
                treeModel.AllEDTsTreeViewModel.EdtsDraft.AddSorted(newItem);
            }
            else
            {
                treeModel.AllNowsTreeViewModel.Nows.AddSorted(newItem);
                treeModel.AllNowsTreeViewModel.NowsDraft.AddSorted(newItem);
            }

            return newItem;
        }

        private NowTreeViewModel GetTreeViewItemForSelectedItem()
        {
            var selectedNow = treeModel.SelectedItem as NowTreeViewModel;

            if (selectedNow != null) { return selectedNow; }

            return null;
        }

        private void SetRank(NowViewModel nextItem, NowViewModel selectedItem, Now data)
        {
            if (selectedItem.Rank.Value + 20 < nextItem.Rank.Value)
            {
                data.Rank = selectedItem.Rank.Value + 20;
                return;
            }

            if (selectedItem.Rank.Value + 10 < nextItem.Rank.Value)
            {
                data.Rank = selectedItem.Rank.Value + 10;
                return;
            }
            if (selectedItem.Rank.Value + 5 < nextItem.Rank.Value)
            {
                data.Rank = selectedItem.Rank.Value + 5;
                return;
            }

            int total = nextItem.Rank.Value + selectedItem.Rank.Value;
            decimal average = total > 0 ? total / 2 : 0;
            data.Rank = average == 0 ? null : (int?)Math.Round(average);
        }
    }
}
