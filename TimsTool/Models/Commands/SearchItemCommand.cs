using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ISearchItemCommand : ICommand { }

    public class SearchItemCommand : ISearchItemCommand
    {
        private ITreeModel treeModel;
        public SearchItemCommand(ITreeModel treeModel)
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
            switch (treeModel.SelectedIndex)
            {
                case 0:
                case 1:
                    {
                        //result definitions tab selected, search result definitions
                        treeModel.AllResultDefinitionsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                case 2:
                case 3:
                    {
                        //nows tab selected, search nows
                        treeModel.AllNowsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllNowsTreeViewModel.Nows.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                case 4:
                case 5:
                    {
                        //edts tab selected, search edts
                        treeModel.AllEDTsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllEDTsTreeViewModel.EDTs.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                case 6:
                case 7:
                    {
                        //now subscriptions tab selected, search now subscriptions
                        treeModel.AllNowSubscriptionsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                case 8:
                case 9:
                    {
                        //EDT subscriptions tab selected, search EDT subscriptions
                        treeModel.AllEDTSubscriptionsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                case 10:
                case 11:
                    {
                        //Informant Register subscriptions tab selected, search Informant Register subscriptions
                        treeModel.AllInformantRegisterSubscriptionsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                case 12:
                case 13:
                    {
                        //Court Register subscriptions tab selected, search Court Register subscriptions
                        treeModel.AllCourtRegisterSubscriptionsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                case 14:
                case 15:
                    {
                        //Prison Court Register subscriptions tab selected, search Prison Court Register subscriptions
                        treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PerformSearch(new SilentObservableCollection<TreeViewItemViewModel>(treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.Cast<TreeViewItemViewModel>()), treeModel.SearchText);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
