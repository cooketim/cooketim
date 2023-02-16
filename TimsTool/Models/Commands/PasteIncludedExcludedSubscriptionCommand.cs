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
    public interface IPasteIncludedExcludedSubscriptionCommand : ICommand { }

    public class PasteIncludedExcludedSubscriptionCommand : IPasteIncludedExcludedSubscriptionCommand
    {
        private ITreeModel treeModel;
        public PasteIncludedExcludedSubscriptionCommand(ITreeModel treeModel)
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
            Log.Verbose("Pasting an included excluded NOW started");
            //Determine the selected item
            var selectedIncludedExcluded = treeModel.SelectedItem as NowSubscriptionNowsTreeViewModel;

            if (selectedIncludedExcluded != null)
            {
                Log.Verbose("Target of paste is a NowSubscriptionNowsTreeViewModel");

                //parent result definition selected as target of paste
                PasteTarget(selectedIncludedExcluded);
            }
            else
            {
                if (treeModel.SelectedItem == null)
                {
                    Log.Verbose("Nothing selecting as target of the paste.  Copy item for the paste left active");
                    return;
                }
                Log.Verbose("Target of paste is a {0} - nothing to do", treeModel.SelectedItem.GetType().ToString());
            }

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private void PasteTarget(NowSubscriptionNowsTreeViewModel target)
        {
            //update the vm
            if (target.IsIncludedNowsTree)
            {
                Log.Verbose("Pasting to Included"); 
                //update the included collection
                target.NowSubscriptionViewModel.IncludedNOWS.Add(PendingCopyModel().NowViewModel.Now);
            }
            else
            {
                Log.Verbose("Pasting to Excluded");

                //update the excluded collection
                target.NowSubscriptionViewModel.ExcludedNOWS.Add(PendingCopyModel().NowViewModel.Now);
            }

            //clone the now tree for the purpose of this subscription i.e. so that it may become its own child entry for this subscription
            var clone = PendingCopyModel().Copy(target);

            target.Children.Add(clone);
            clone.IsSelected = true;

            //set the data as modified
            target.NowSubscriptionViewModel.LastModifiedDate = DateTime.Now;
        }

        private NowTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as NowTreeViewModel;
        }

    }
}
