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
    public interface IAddNewChildNowSubscriptionCommand : ICommand { }

    public class AddNewChildNowSubscriptionCommand : AddNewNowSubscriptionCommand, IAddNewChildNowSubscriptionCommand
    {
        public AddNewChildNowSubscriptionCommand(ITreeModel treeModel) : base(treeModel) { }

        protected override void Execute(Tuple<string, PublicationTagsModel> paramsModel) { }

        protected override NowSubscriptionViewModel MakeViewModel(Tuple<string, PublicationTagsModel> paramsModel)
        {
            //find the parent tree item
            var parent = GetTreeViewItemForSelectedItem();
            if (parent == null) { return null; }

            var data = new NowSubscription(parent.NowSubscriptionViewModel.NowSubscription);
            return new NowSubscriptionViewModel(treeModel, data);
        }

        protected override void StoreTreeItem(NowSubscriptionTreeViewModel newChildItem)
        {   
            //find the parent for the new tree view item
            var parent = GetTreeViewItemForSelectedItem();

            //make the new tree item a child of this data item
            var index = parent.Children.ToList().FindLastIndex(x => x.GetType() == typeof(NowSubscriptionTreeViewModel));
            if (index == parent.Children.Count())
            {
                index = parent.Children.Count() - 1;
            }

            if (index < 0)
            {
                index = 0;
            }

            parent.Children.Insert(index, newChildItem);

            //expand the parent
            parent.IsExpanded = true;

            //update the parent child collection in the view model & the data 
            if (parent.NowSubscriptionViewModel.ChildNowSubscriptions == null) { parent.NowSubscriptionViewModel.ChildNowSubscriptions = new List<NowSubscriptionViewModel>(); }
            parent.NowSubscriptionViewModel.ChildNowSubscriptions.Add(newChildItem.NowSubscriptionViewModel);

            if (parent.NowSubscriptionViewModel.NowSubscription.ChildNowSubscriptions == null) { parent.NowSubscriptionViewModel.NowSubscription.ChildNowSubscriptions = new List<NowSubscription>(); }
            parent.NowSubscriptionViewModel.NowSubscription.ChildNowSubscriptions.Add(newChildItem.NowSubscriptionViewModel.NowSubscription);
        }

        private NowSubscriptionTreeViewModel GetTreeViewItemForSelectedItem()
        {
            var selectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;

            if (selectedNowSubscription != null) { return selectedNowSubscription; }

            return null;
        }
    }
}
