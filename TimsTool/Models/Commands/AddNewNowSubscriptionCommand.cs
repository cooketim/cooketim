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
    public interface IAddNewNowSubscriptionCommand : ICommand { }
    public class AddNewNowSubscriptionCommand : IAddNewNowSubscriptionCommand
    {
        protected ITreeModel treeModel;
        public AddNewNowSubscriptionCommand(ITreeModel treeModel)
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
            var paramsModel = parameter == null ? null : parameter as Tuple<string, PublicationTagsModel>;

            var subscriptionType = parameter == null ? null : parameter as string;
            paramsModel = new Tuple<string, PublicationTagsModel>(subscriptionType, null);

            Execute(paramsModel);

            //make a vm & tree model
            var vm = MakeViewModel(paramsModel);
            var treeItem = MakeNewTreeModel(vm);

            //Store the tree item and place in relevant display
            StoreTreeItem(treeItem);

            //store the data objects
            var exists = treeModel.AllData.NowSubscriptions.FirstOrDefault(x => x.UUID == vm.NowSubscription.UUID);
            treeModel.AllData.NowSubscriptions.Add(vm.NowSubscription);

            //reset any copied item
            treeItem.PendingCopyModel = null;

            //select the new item
            treeItem.IsSelected = true;
        }
        protected virtual void Execute(Tuple<string, PublicationTagsModel> paramsModel)
        {
            if (paramsModel == null || paramsModel.Item2 == null)
            {
                Log.Error("Publication Tags Model not sent for add new Subscription");
                return;
            }
        }

        protected virtual NowSubscriptionViewModel MakeViewModel(Tuple<string, PublicationTagsModel> paramsModel)
        {
            if (paramsModel == null || string.IsNullOrEmpty(paramsModel.Item1)) { return null; }

            //determine if a Now or an EDT or an informant register subscription or a court register subscription
            var isNow = false;
            var isEDT = false;
            var isInformantRegister = false;
            var isCourtRegister = false;
            var isPrisonCourtRegister = false;

            //Make a new data item 
            var subscriptionType = paramsModel.Item1.ToLowerInvariant();
            switch (subscriptionType)
            {
                case "now":
                    {
                        isNow = true;
                        break;
                    }
                case "edt":
                    {
                        isEDT = true;
                        break;
                    }
                case "informantregister":
                    {
                        isInformantRegister = true;
                        break;
                    }
                case "courtregister":
                    {
                        isCourtRegister = true;
                        break;
                    }
                case "prisoncourtregister":
                    {
                        isPrisonCourtRegister = true;
                        break;
                    }
            }
            var data = new NowSubscription(isNow, isEDT, isInformantRegister, isCourtRegister, isPrisonCourtRegister);
            data.PublishedStatus = PublishedStatus.Draft;
            data.IsNewItem = true;
            if (paramsModel.Item2 != null)
            {
                data.PublicationTags = paramsModel.Item2.SelectedPublicationTags.ToList();
            }
            return new NowSubscriptionViewModel(treeModel, data);
        }

        private NowSubscriptionTreeViewModel MakeNewTreeModel(NowSubscriptionViewModel vm)
        {
            //ensure that name is unique
            var duplicateName = false;
            var name = vm.Name;
            int i = 1;
            do
            {
                duplicateName = TestDuplicateName(vm);
                if (duplicateName)
                {
                    i++;
                    vm.Name = string.Format("{0} ({1})", name, i);
                }

            } while (duplicateName);


            //Make new tree model
            NowSubscriptionTreeViewModel newItem = null;
            if (vm.IsNow)
            {
                treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Add(vm);
                newItem = new NowSubscriptionTreeViewModel(treeModel, vm, treeModel.AllNowsTreeViewModel);
            }
            else if (vm.IsEDT)
            {
                treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Add(vm);
                newItem = new NowSubscriptionTreeViewModel(treeModel, vm, treeModel.AllEDTsTreeViewModel);
            }
            else
            {
                newItem = new NowSubscriptionTreeViewModel(treeModel, vm);
                if (vm.IsCourtRegister)
                {
                    treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Add(vm);
                }
                if (vm.IsPrisonCourtRegister)
                {
                    treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Add(vm);
                }
                if (vm.IsInformantRegister)
                {
                    treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Add(vm);
                }
            }

            return newItem;
        }

        protected virtual void StoreTreeItem(NowSubscriptionTreeViewModel newItem)
        {
            if (newItem.NowSubscriptionViewModel.IsNow)
            {
                //add to the tree of root now Subscriptions
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.AddSorted(newItem);
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsDraft.AddSorted(newItem);
                return;
            }

            if (newItem.NowSubscriptionViewModel.IsEDT)
            {
                //add to the tree of root now Subscriptions
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.AddSorted(newItem);
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsDraft.AddSorted(newItem);
                return;
            }

            if (newItem.NowSubscriptionViewModel.IsCourtRegister)
            {
                //add to the tree of root now Subscriptions
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.AddSorted(newItem);
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsDraft.AddSorted(newItem);
                return;
            }

            if (newItem.NowSubscriptionViewModel.IsPrisonCourtRegister)
            {
                //add to the tree of root now Subscriptions
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.AddSorted(newItem);
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsDraft.AddSorted(newItem);
                return;
            }

            if (newItem.NowSubscriptionViewModel.IsInformantRegister)
            {
                //add to the tree of root now Subscriptions
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.AddSorted(newItem);
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsDraft.AddSorted(newItem);
                return;
            }
        }

        private bool TestDuplicateName(NowSubscriptionViewModel vm)
        {
            if (vm.IsNow)
            {
                return treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.Name == vm.Name) != null;
            }
            if (vm.IsEDT)
            {
                return treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.Name == vm.Name) != null;
            }
            if (vm.IsCourtRegister)
            {
                return treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.Name == vm.Name) != null;
            }
            if (vm.IsPrisonCourtRegister)
            {
                return treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.Name == vm.Name) != null;
            }
            if (vm.IsInformantRegister)
            {
                return treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.Name == vm.Name) != null;
            }
            return true;
        }     
    }
}
