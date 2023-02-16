using DataLib;
using Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public class DraftNowModel
    {
        public NowTreeViewModel DraftNow { get; }
        public NowTreeViewModel ExistingPublishedNow { get; }
        public bool SavePending { get; }
        public string ChangeReport { get; private set; }
        public bool IsChanged { get; private set; }

        private ITreeModel treeModel;

        public bool IsDeleted
        {
            get
            {
                return DraftNow.IsDeleted;
            }
        }

        public DraftNowModel(ITreeModel treeModel, NowTreeViewModel draftNow, NowTreeViewModel existingPublishedNow, bool savePending)
        {
            this.treeModel = treeModel;
            DraftNow = draftNow;
            ExistingPublishedNow = existingPublishedNow;
            SavePending = savePending;
            SetChanges();
        }

        private void SetChanges()
        {
            if (DraftNow.NowViewModel.IsDeleted)
            {
                //draft is a deleted item intending to remove the current published item
                IsChanged = true;
                ChangeReport = String.Format("{0} Deleted", DraftNow.NowViewModel.IsEDT ? "EDT" : "NOW");
                return;
            }

            if (ExistingPublishedNow == null)
            {
                //draft must be a new item
                IsChanged = true;
                ChangeReport = String.Format("New {0} Added", DraftNow.NowViewModel.IsEDT ? "EDT" : "NOW");
                return;
            }

            var publishedData = ExistingPublishedNow.NowViewModel.Now;
            var draftData = DraftNow.NowViewModel.Now;

            var sb = new StringBuilder();
            //Set any changes 
            var changes = publishedData.GetChangedProperties(draftData);
            if (!string.IsNullOrEmpty(changes))
            {
                sb.AppendLine(changes);
            }

            //deal with any changes to the now requirements
            var draftNowRequirements =  DraftNow.NowViewModel.AllNowRequirements.Where(x=>x.DeletedDate == null).ToList();
            var publishedNowRequirements = ExistingPublishedNow.NowViewModel.AllNowRequirements.Where(x => x.DeletedDate == null).ToList();

            //report new draft requirements that do not have an equivalent published now requirement
            var draftNotPublished = draftNowRequirements
                                        .Select(x => new Tuple<Guid?, Guid?>(x.ResultDefinition.MasterUUID, GetRootParentId(x))).ToList()
                                    .Except(
                                    publishedNowRequirements
                                        .Select(x => new Tuple<Guid?, Guid?>(x.ResultDefinition.MasterUUID, GetRootParentId(x))).ToList()
                                           ).ToList();
            if (draftNotPublished.Any())
            {
                foreach (var draftKeys in draftNotPublished)
                {
                    var draft = draftNowRequirements.First(x => x.ResultDefinition.MasterUUID == draftKeys.Item1 && draftKeys.Item2 == GetRootParentId(x));
                    if (draft.ParentNowRequirement == null)
                    {
                        sb.AppendLine(string.Format("New root now requirement '{0}'", draft.ResultDefinition.Label));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("New child now requirement '{0}', parent '{1}'", draft.ResultDefinition.Label, draft.ParentNowRequirement.ResultDefinition.Label));
                    }
                }
            }

            //report published requirements that do not have an equivalent draft now requirement
            var publishedNotDraft = publishedNowRequirements
                                        .Select(x => new Tuple<Guid?, Guid?>(x.ResultDefinition.MasterUUID, GetRootParentId(x))).ToList()
                                    .Except(
                                    draftNowRequirements
                                        .Select(x => new Tuple<Guid?, Guid?>(x.ResultDefinition.MasterUUID, GetRootParentId(x))).ToList()
                                           ).ToList();
            if (publishedNotDraft.Any())
            {
                foreach (var publishedKeys in publishedNotDraft)
                {
                    var published = publishedNowRequirements.First(x => x.ResultDefinition.MasterUUID == publishedKeys.Item1 && publishedKeys.Item2 == GetRootParentId(x));
                    if (published.ParentNowRequirement == null)
                    {
                        sb.AppendLine(string.Format("Removed root now requirement '{0}'", published.ResultDefinition.Label));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("Removed child now requirement '{0}', parent '{1}'", published.ResultDefinition.Label, published.ParentNowRequirement.ResultDefinition.Label));
                    }
                }
            }

            //report on matched now requirements that have differences
            var common = publishedNowRequirements
                                        .Select(x => new Tuple<Guid?, Guid?>(x.ResultDefinition.MasterUUID, GetRootParentId(x))).ToList()
                                    .Intersect(
                                    draftNowRequirements
                                        .Select(x => new Tuple<Guid?, Guid?>(x.ResultDefinition.MasterUUID, GetRootParentId(x))).ToList()
                                           ).ToList();
            if (common.Any())
            {
                foreach (var commonKeys in common)
                {
                    var draftNr = draftNowRequirements.First(x => x.ResultDefinition.MasterUUID == commonKeys.Item1 && commonKeys.Item2 == GetRootParentId(x));
                    var publishedNr = publishedNowRequirements.First(x => x.ResultDefinition.MasterUUID == commonKeys.Item1 && commonKeys.Item2 == GetRootParentId(x));

                    var changedNr = publishedNr.NowRequirement.GetChangedProperties(draftNr.NowRequirement);
                    if (!string.IsNullOrEmpty(changedNr))
                    {
                        if (draftNr.ParentNowRequirement == null)
                        {
                            sb.AppendLine(string.Format("Changed root now requirement '{0}'", draftNr.ResultDefinition.Label));
                        }
                        else
                        {
                            sb.AppendLine(string.Format("Changed child now requirement '{0}', parent '{1}'", draftNr.ResultDefinition.Label, draftNr.ParentNowRequirement.ResultDefinition.Label));
                        }
                        sb.AppendLine(changedNr);
                    }
                }
            }

            if (sb.ToString().Trim().Length>0)
            {
                ChangeReport = sb.ToString().Trim();
                IsChanged = true;
            }
        }

        private Guid? GetRootParentId(NowRequirementViewModel nr)
        {
            if (nr.RootParentNowRequirement == null)
            {
                return null;
            }
            if (nr.RootParentNowRequirement.ResultDefinition.MasterUUID == null)
            {
                return nr.RootParentNowRequirement.ResultDefinition.UUID;
            }
            return nr.RootParentNowRequirement.ResultDefinition.MasterUUID;
        }

        internal void PrepareForPublication()
        {
            PrepareAssociatedNowSubscriptions();
            if (ExistingPublishedNow != null)
            {
                //existing published moves to revision pending status
                SetNowPublishedStatus(ExistingPublishedNow, PublishedStatus.RevisionPending);
            }

            //new draft moves to published pending status
            SetNowPublishedStatus(DraftNow, PublishedStatus.PublishedPending);

            //Set the publication comment
            SetPublicationComment();

            //when required, i.e. when the draft NOW has been created via a usage, save the now to the relevant collection
            var matchedVM = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.UUID == DraftNow.NowViewModel.UUID);
            if(matchedVM == null)
            {
                var nowCmd = treeModel.DraftNowCommand as DraftNowCommand;
                nowCmd.SaveDraft(DraftNow, false);
            }
        }

        private void PrepareAssociatedNowSubscriptions()
        {
            //find the associated subscriptions that are published, published pending or draft
            var inclSubs = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                    x => x.PublishedStatus != PublishedStatus.Revision && x.PublishedStatus != PublishedStatus.RevisionPending &&
                                         x.NowSubscription.IncludedNOWS != null &&
                                         x.NowSubscription.IncludedNOWS.Select(y => y.MasterUUID ?? y.UUID).Contains(
                                                        DraftNow.NowViewModel.MasterUUID ?? DraftNow.NowViewModel.UUID)
                                        );
            foreach (var sub in inclSubs)
            {
                UpdateAssociatedSubscription(sub, sub.IncludedNOWS);
            }

            var exclSubs = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                    x => x.PublishedStatus != PublishedStatus.Revision && x.PublishedStatus != PublishedStatus.RevisionPending &&
                                         x.NowSubscription.ExcludedNOWS != null &&
                                         x.NowSubscription.ExcludedNOWS.Select(y => y.MasterUUID ?? y.UUID).Contains(
                                                        DraftNow.NowViewModel.MasterUUID ?? DraftNow.NowViewModel.UUID)
                                        );
            foreach (var sub in exclSubs)
            {
                UpdateAssociatedSubscription(sub, sub.ExcludedNOWS);
            }
        }

        private void UpdateAssociatedSubscription(NowSubscriptionViewModel sub, List<Now> target)
        {
            var index = target.Select(x => x.MasterUUID ?? x.UUID).ToList().IndexOf(DraftNow.NowViewModel.MasterUUID ?? DraftNow.NowViewModel.UUID);
            var currentIncluded = target[index];
            if (currentIncluded.UUID != DraftNow.NowViewModel.UUID)
            {
                target.RemoveAt(index);
                target.Insert(index, DraftNow.NowViewModel.Now);
                sub.LastModifiedDate = DateTime.Now;

                //force a refresh of the child tree items
                var treeSub = treeModel.NowSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.UUID == sub.UUID);
                if (treeSub != null)
                {
                    treeSub.ResetChildren();
                }
            }
        }

        private void SetNowPublishedStatus(NowTreeViewModel now, PublishedStatus status)
        {
            //now status
            now.NowViewModel.PublishedStatus = status;

            //cascade each of the now requirements
            SetNowReqPublishedStatus(now.NowViewModel.NowRequirements, status);
        }

        private void SetNowReqPublishedStatus(List<NowRequirementViewModel> reqs, PublishedStatus status)
        {
            if (reqs == null) { return; }

            foreach(var nr in reqs)
            {
                nr.PublishedStatus = status;

                //cascade the status to each of the prompt rules
                if (nr.NowRequirementPromptRules != null)
                {
                    foreach(var rule in nr.NowRequirementPromptRules)
                    {
                        rule.PublishedStatus = status;
                    }
                }

                SetNowReqPublishedStatus(nr.NowRequirements, status);
            }
        }

        private void SetPublicationComment()
        {
            //set the publication comment
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Draft revision published pending at {0}.", DraftNow.NowViewModel.PublishedStatusDate.ToString("d MMM yyyy h:mm:ss tt")));
            sb.AppendLine(string.Format("New revision identifier: '{0}'.", DraftNow.NowViewModel.UUID));
            if (DraftNow.NowViewModel.PublicationTags != null && DraftNow.NowViewModel.PublicationTags.Count > 0)
            {
                sb.AppendLine("The following publication tags were applied...");
                foreach (var tag in DraftNow.NowViewModel.PublicationTags)
                {
                    sb.AppendLine(tag);
                }
            }
            sb.AppendLine();
            sb.AppendLine("The following property changes were applied...");
            sb.AppendLine(ChangeReport);

            //create a new comment 
            var newComment = new CommentViewModel(DraftNow.NowViewModel);

            //remove double spacing and set the comment note and system type
            newComment.Note = Regex.Replace(sb.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            newComment.SystemCommentType = "Publication";

            //add comment to the parents collection of comments
            DraftNow.NowViewModel.AssociatedCommentsModel.AddCommentCommand.Execute(newComment);

            //reset the children on the draft so that they are rebuilt
            DraftNow.ResetChildren();
        }
    }
}
