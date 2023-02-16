using DataLib;
using DocumentFormat.OpenXml.Office2016.Excel;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteRevisionsCommand : ICommand { }

    public class DeleteRevisionsCommand : IDeleteRevisionsCommand
    {
        protected ITreeModel treeModel;
        public DeleteRevisionsCommand(ITreeModel treeModel)
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
            ExecuteCommand(parameter);
        }

        protected virtual void ExecuteCommand(object parameter)
        {
            var selectedResultDefinitionRevision = treeModel.SelectedItem as ResultDefinitionRevisionTreeViewModel;
            if (selectedResultDefinitionRevision != null)
            {
                DeleteResultDefinitionRevisions(selectedResultDefinitionRevision);
                return;
            }

            var selectedNowRevision = treeModel.SelectedItem as NowRevisionTreeViewModel;
            if (selectedNowRevision != null)
            {
                if (selectedNowRevision.IsEDT)
                {
                    DeleteEdtRevisions(selectedNowRevision);
                }
                else
                {
                    DeleteNowRevisions(selectedNowRevision);
                }
                return;
            }

            var selectedNowSubscriptionRevision = treeModel.SelectedItem as NowSubscriptionRevisionTreeViewModel;
            if (selectedNowSubscriptionRevision != null)
            {
                switch (selectedNowSubscriptionRevision.SubscriptionType)
                {
                    case NowSubscriptionType.NOW:
                        {
                            DeleteNowSubscriptionRevisions(selectedNowSubscriptionRevision);
                            break;
                        }
                    case NowSubscriptionType.EDT:
                        {
                            DeleteEdtSubscriptionRevisions(selectedNowSubscriptionRevision);
                            break;
                        }
                    case NowSubscriptionType.CourtRegister:
                        {
                            DeleteCourtRegisterSubscriptionRevisions(selectedNowSubscriptionRevision);
                            break;
                        }
                    case NowSubscriptionType.PrisonCourtRegister:
                        {
                            DeletePrisonCourtRegisterSubscriptionRevisions(selectedNowSubscriptionRevision);
                            break;
                        }
                    case NowSubscriptionType.InformantRegister:
                        {
                            DeleteInformantRegisterSubscriptionRevisions(selectedNowSubscriptionRevision);
                            break;
                        }
                }
            }
        }

        private void DeleteInformantRegisterSubscriptionRevisions(NowSubscriptionRevisionTreeViewModel selectedNowSubscriptionRevision)
        {
            var revisionDate = selectedNowSubscriptionRevision.GroupDate;

            //Remove the tree view
            treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsRevision.Remove(selectedNowSubscriptionRevision);

            //Remove all now subscription tree views for the given revision date
            treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                               x.NowSubscriptionViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscription view models
            treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscriptions data
            treeModel.AllData.NowSubscriptions.RemoveAll(x => x.IsInformantRegister && x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
        }

        private void DeletePrisonCourtRegisterSubscriptionRevisions(NowSubscriptionRevisionTreeViewModel selectedNowSubscriptionRevision)
        {
            var revisionDate = selectedNowSubscriptionRevision.GroupDate;

            //Remove the tree view
            treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsRevision.Remove(selectedNowSubscriptionRevision);

            //Remove all now subscription tree views for the given revision date
            treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                               x.NowSubscriptionViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscription view models
            treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscriptions data
            treeModel.AllData.NowSubscriptions.RemoveAll(x => x.IsPrisonCourtRegister && x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
        }

        private void DeleteCourtRegisterSubscriptionRevisions(NowSubscriptionRevisionTreeViewModel selectedNowSubscriptionRevision)
        {
            var revisionDate = selectedNowSubscriptionRevision.GroupDate;

            //Remove the tree view
            treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsRevision.Remove(selectedNowSubscriptionRevision);

            //Remove all now subscription tree views for the given revision date
            treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                               x.NowSubscriptionViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscription view models
            treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscriptions data
            treeModel.AllData.NowSubscriptions.RemoveAll(x => x.IsCourtRegister && x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
        }

        private void DeleteEdtSubscriptionRevisions(NowSubscriptionRevisionTreeViewModel selectedNowSubscriptionRevision)
        {
            var revisionDate = selectedNowSubscriptionRevision.GroupDate;

            //Remove the tree view
            treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsRevision.Remove(selectedNowSubscriptionRevision);

            //Remove all now subscription tree views for the given revision date
            treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                               x.NowSubscriptionViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscription view models
            treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscriptions data
            treeModel.AllData.NowSubscriptions.RemoveAll(x => x.IsEDT && x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
        }

        private void DeleteNowSubscriptionRevisions(NowSubscriptionRevisionTreeViewModel selectedNowSubscriptionRevision)
        {
            var revisionDate = selectedNowSubscriptionRevision.GroupDate;

            //Remove the tree view
            treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsRevision.Remove(selectedNowSubscriptionRevision);

            //Remove all now subscription tree views for the given revision date
            treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                               x.NowSubscriptionViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscription view models
            treeModel.AllNowSubscriptionViewModel.NowSubscriptions.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now subscriptions data
            treeModel.AllData.NowSubscriptions.RemoveAll(x => x.IsNow && x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
        }

        private void DeleteNowRevisions(NowRevisionTreeViewModel selectedNowRevision)
        {
            var revisionDate = selectedNowRevision.GroupDate;

            //Remove the tree view
            treeModel.AllNowsTreeViewModel.NowsRevision.Remove(selectedNowRevision);

            //Remove all now tree views for the given revision date
            treeModel.AllNowsTreeViewModel.Nows.RemoveAll(x => x.NowViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                               x.NowViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all nows view models
            treeModel.AllNowsViewModel.Nows.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now requirements & nows data
            treeModel.AllData.NowRequirements.RemoveAll(x => IsAssociatedToRevisedNowEdt(x.NOWUUID, revisionDate, false));
            treeModel.AllData.Nows.RemoveAll(x => !x.IsEDT && x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);

            //cascade now subscriptions for the given revision date
            var nowSubRevisiontree = treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsRevision.FirstOrDefault(x => x.GroupDate == revisionDate);
            if (nowSubRevisiontree != null)
            {
                DeleteNowSubscriptionRevisions(nowSubRevisiontree);
            }
        }

        private void DeleteEdtRevisions(NowRevisionTreeViewModel selectedNowRevision)
        {
            var revisionDate = selectedNowRevision.GroupDate;

            //Remove the tree view
            treeModel.AllEDTsTreeViewModel.EdtsRevision.Remove(selectedNowRevision);

            //Remove all edt tree views for the given revision date
            treeModel.AllEDTsTreeViewModel.EDTs.RemoveAll(x => x.NowViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                               x.NowViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all edt view models
            treeModel.AllEDTsViewModel.EDTs.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all now requirements & nows data
            treeModel.AllData.NowRequirements.RemoveAll(x => IsAssociatedToRevisedNowEdt(x.NOWUUID, revisionDate, true));
            treeModel.AllData.Nows.RemoveAll(x => x.IsEDT && x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);

            //cascade edt subscriptions for the given revision date
            var nowSubRevisiontree = treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsRevision.FirstOrDefault(x => x.GroupDate == revisionDate);
            if (nowSubRevisiontree != null)
            {
                DeleteEdtSubscriptionRevisions(nowSubRevisiontree);
            }
        }

        private bool IsAssociatedToRevisedNowEdt(Guid? nOWUUID, DateTime revisionDate, bool isEDT)
        {
            if (nOWUUID == null) { return false; }
            var match = treeModel.AllData.Nows.FirstOrDefault(x => x.UUID == nOWUUID  && x.IsEDT == isEDT && x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
            if (match == null) { return false; }
            return true;
        }

        private void DeleteResultDefinitionRevisions(ResultDefinitionRevisionTreeViewModel selectedResultDefinitionRevision)
        {
            var revisionDate = selectedResultDefinitionRevision.GroupDate;

            //Remove the tree view
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRevision.Remove(selectedResultDefinitionRevision);

            //Remove all result definition tree views for the given revision date
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.RemoveAll(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                                                         x.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatusDate.Date == revisionDate.Date);
            //Remove all result definition view models
            treeModel.AllResultDefinitionsViewModel.Definitions.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all result rule view models
            treeModel.AllResultRuleViewModel.Rules.RemoveAll(x=> x.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                                 x.ChildResultDefinitionViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all result prompt rule view models
            treeModel.AllResultPromptViewModel.Prompts.RemoveAll(x => x.ResultDefinitionViewModel.PublishedStatus == PublishedStatus.Revision &&
                                                                 x.ResultDefinitionViewModel.PublishedStatusDate.Date == revisionDate.Date);

            //Remove all fixed list view models
            treeModel.AllFixedListViewModel.FixedLists.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision &&
                                                                 x.PublishedStatusDate.Date == revisionDate.Date);

            //Update the result prompt word group view models to remove any association to a removed result prompt
            foreach (var item in treeModel.AllResultPromptWordGroupViewModel.WordGroups)
            {
                item.ParentPrompts.RemoveAll(x => x.PublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate.Date == revisionDate.Date);
            }
            treeModel.AllResultPromptWordGroupViewModel.WordGroups.RemoveAll(x => !x.ParentPrompts.Any());

            //Update the result definition word group view models to remove any association to a removed result definition
            foreach (var item in treeModel.AllResultDefinitionWordGroupViewModel.WordGroups)
            {
                item.ParentWordGroups.RemoveAll(x => x.ParentResultDefinitionVM.PublishedStatus == PublishedStatus.Revision && x.ParentResultDefinitionVM.PublishedStatusDate.Date == revisionDate.Date);
            }
            treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.RemoveAll(x => !x.ParentWordGroups.Any());

            //Remove the data
            treeModel.AllData.ResultDefinitionRules.RemoveAll(x=>x.ResultDefinition.CalculatedPublishedStatus == PublishedStatus.Revision && x.ResultDefinition.PublishedStatusDate != null && x.ResultDefinition.PublishedStatusDate.Value.Date == revisionDate.Date);
            treeModel.AllData.ResultPromptRules.RemoveAll(x => IsAssociatedToRevisedRd(x.ResultDefinitionUUID, revisionDate));
            treeModel.AllData.ResultDefinitions.RemoveAll(x => x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
            RemoveRdWordGroupOrphans();
            RemoveRpWordGroupOrphans();
            RemoveFixedListOrphans();

            //cascade nows & edts for the given revision date
            var nowRevisiontree = treeModel.AllNowsTreeViewModel.NowsRevision.FirstOrDefault(x=>x.GroupDate == revisionDate);
            if (nowRevisiontree != null)
            {
                DeleteNowRevisions(nowRevisiontree);
            }
            var edtRevisiontree = treeModel.AllEDTsTreeViewModel.EdtsRevision.FirstOrDefault(x => x.GroupDate == revisionDate);
            if (edtRevisiontree != null)
            {
                DeleteEdtRevisions(edtRevisiontree);
            }

            //cascade registers for the given revision date
            var nowSubRevisiontree = treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsRevision.FirstOrDefault(x => x.GroupDate == revisionDate);
            if (nowSubRevisiontree != null)
            {
                DeleteCourtRegisterSubscriptionRevisions(nowSubRevisiontree);
            }
            nowSubRevisiontree = treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsRevision.FirstOrDefault(x => x.GroupDate == revisionDate);
            if (nowSubRevisiontree != null)
            {
                DeletePrisonCourtRegisterSubscriptionRevisions(nowSubRevisiontree);
            }
            nowSubRevisiontree = treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsRevision.FirstOrDefault(x => x.GroupDate == revisionDate);
            if (nowSubRevisiontree != null)
            {
                DeleteInformantRegisterSubscriptionRevisions(nowSubRevisiontree);
            }
        }

        private void RemoveFixedListOrphans()
        {
            var toDelete = new List<FixedList>();
            var distinctPrompts = treeModel.AllData.ResultPromptRules.GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt);
            foreach (var item in treeModel.AllData.FixedLists)
            {
                var matched = distinctPrompts.Where(x=>x.FixedList != null && x.FixedList.UUID == item.UUID) != null;
                if (!matched)
                {
                    toDelete.Add(item);
                }
            }
            foreach (var item in toDelete)
            {
                treeModel.AllData.FixedLists.RemoveAll(x => x.UUID == item.UUID);
            }
        }

        private void RemoveRpWordGroupOrphans()
        {
            var toDelete = new List<ResultPromptWordGroup>();
            var distinctPrompts = treeModel.AllData.ResultPromptRules.GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt);
            foreach (var item in treeModel.AllData.ResultPromptWordGroups)
            {
                var matched = false;
                foreach (var rp in distinctPrompts.Where(x => x.ResultPromptWordGroups != null && x.ResultPromptWordGroups.Any()))
                {
                    matched = rp.ResultPromptWordGroups.FirstOrDefault(x => x.UUID == item.UUID) != null;
                    if (matched) { break; }
                }
                if (!matched)
                {
                    toDelete.Add(item);
                }
            }
            foreach (var item in toDelete)
            {
                treeModel.AllData.ResultPromptWordGroups.RemoveAll(x => x.UUID == item.UUID);
            }
        }

        private void RemoveRdWordGroupOrphans()
        {
            var toDelete = new List<ResultDefinitionWordGroup>();
            foreach(var item in treeModel.AllData.ResultDefinitionWordGroups)
            {
                var matched = false;
                foreach(var rd in treeModel.AllData.ResultDefinitions.Where(x=>x.WordGroups != null && x.WordGroups.Any()))
                {
                    foreach(var rwg in rd.WordGroups)
                    {
                        matched = rwg.ResultDefinitionWordGroups.FirstOrDefault(x => x.UUID == item.UUID) != null;
                        if(matched) { break; }
                    }
                    if (matched) { break; }
                }
                if (!matched)
                {
                    toDelete.Add(item);
                }
            }
            foreach(var item in toDelete)
            {
                treeModel.AllData.ResultDefinitionWordGroups.RemoveAll(x => x.UUID == item.UUID);
            }
        }

        private bool IsAssociatedToRevisedRd(Guid? resultDefinitionUUID, DateTime revisionDate)
        {
            if (resultDefinitionUUID == null) { return false; }

            var match = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == resultDefinitionUUID && x.CalculatedPublishedStatus == PublishedStatus.Revision && x.PublishedStatusDate != null && x.PublishedStatusDate.Value.Date == revisionDate.Date);
            if (match == null) { return false; }
            return true;
        }
    }
}
