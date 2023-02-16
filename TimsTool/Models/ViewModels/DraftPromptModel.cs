using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public class DraftPromptModel
    {
        private bool changesSet;
        private bool publicationCommentSet;
        private ITreeModel treeModel;

        public DraftPromptModel(ITreeModel treeModel, ResultPromptViewModel source, List<DraftPromptModel> allDraftPrompts)
        {
            this.treeModel = treeModel;
            Prompt = source;

            if (source.FixedList != null && source.FixedList.PublishedStatus == PublishedStatus.Draft && !source.FixedList.IsDeleted)
            {
                //see if there is already a draft fixed list model
                var dflPrompt = allDraftPrompts.FirstOrDefault(x => x.DraftFixedList != null && x.DraftFixedList.DraftFixedList.MasterUUID == source.FixedList.MasterUUID);
                if (dflPrompt == null)
                {
                    DraftFixedList = new DraftFixedListModel(treeModel,source.FixedList);
                }
                else
                {
                    DraftFixedList = dflPrompt.DraftFixedList;
                }
            }

            if (source.WordGroups != null && source.WordGroups.Where(x=>x.PublishedStatus == PublishedStatus.Draft && !x.IsDeleted).Any())
            {
                DraftResultPromptWordGroups = new List<DraftResultPromptWordGroupModel>();
                foreach (var rpwg in source.WordGroups.Where(x => x.PublishedStatus == PublishedStatus.Draft && !x.IsDeleted))
                {
                    //see if there is already a draft result prompt word group model
                    var wgPrompt = allDraftPrompts.FirstOrDefault(
                                        x => x.DraftResultPromptWordGroups != null && 
                                             x.DraftResultPromptWordGroups.Select(y=>y.DraftResultPromptWordGroup.MasterUUID).Contains(rpwg.MasterUUID));
                    if (wgPrompt == null)
                    {
                        DraftResultPromptWordGroups.Add(new DraftResultPromptWordGroupModel(treeModel, rpwg));
                    }
                    else
                    {
                        var rpwgModel = wgPrompt.DraftResultPromptWordGroups.First(x => x.DraftResultPromptWordGroup.MasterUUID == rpwg.MasterUUID);
                        DraftResultPromptWordGroups.Add(rpwgModel);
                    }
                }
            }
        }

        public DraftFixedListModel DraftFixedList { get; }

        public List<DraftResultPromptWordGroupModel> DraftResultPromptWordGroups { get; }

        public ResultPromptViewModel Prompt { get; }

        public bool IsChanged { get; set; }

        public bool IsAdded { get; set; }

        public bool IsFixedListChanged 
        { 
            get
            {
                if (DraftFixedList != null && DraftFixedList.IsChanged)
                {
                    return true;
                }
                return false;
            }
        }

        public bool HasResultPromptWordGroupChanges
        {
            get
            {
                if (DraftResultPromptWordGroups != null && DraftResultPromptWordGroups.Where(x=>x.IsChanged).Any())
                {
                    return true;
                }
                return false;
            }
        }

        public string ChangeReport { get; set; }

        public ResultPromptRuleViewModel ExistingPublishedPrompt { get; private set; }

        internal void PrepareForPublication()
        {
            //determine if there is a draft fixed list that is deleted
            if (Prompt.FixedList != null && Prompt.FixedList.PublishedStatus == PublishedStatus.Draft && Prompt.FixedList.IsDeleted)
            {
                //see if there are any other usges
                var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                            x => x.ResultPromptViewModel.FixedList != null &&
                                 x.ResultPromptViewModel.UUID != Prompt.UUID &&
                                 x.ResultPromptViewModel.FixedList.UUID == Prompt.FixedList.UUID);
                if (!otherUsages.Any())
                {
                    //remove draft orphan view model and data
                    treeModel.AllFixedListViewModel.FixedLists.RemoveAll(x => x.UUID == Prompt.FixedList.UUID);
                    treeModel.AllData.FixedLists.RemoveAll(x => x.UUID == Prompt.FixedList.UUID);
                }

                //remove the fixed list from the draft prompt
                Prompt.FixedList = null;
            }

            if (Prompt.WordGroups != null && Prompt.WordGroups.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.IsDeleted).Any())
            {
                foreach (var rpwg in Prompt.WordGroups.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.IsDeleted))
                {
                    //see if there are any other usges
                    var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                                            x => x.ResultPromptViewModel.WordGroups != null &&
                                                 x.ResultPromptViewModel.UUID != Prompt.UUID &&
                                                 x.ResultPromptViewModel.WordGroups.Select(y => y.UUID).Contains(rpwg.UUID));
                    if (!otherUsages.Any())
                    {
                        //remove draft orphan view model and data
                        treeModel.AllResultPromptWordGroupViewModel.WordGroups.RemoveAll(x => x.UUID == rpwg.UUID);
                        treeModel.AllData.ResultPromptWordGroups.RemoveAll(x => x.UUID == rpwg.UUID);
                    }
                }

                //remove deleted word groups from the draft prompt
                Prompt.WordGroups.RemoveAll(x=>x.IsDeleted);
            }

            if (ExistingPublishedPrompt != null)
            {
                //existing published moves to revision status
                ExistingPublishedPrompt.ResultPromptViewModel.PublishedStatus = PublishedStatus.RevisionPending;

                //when required set any associated fixed list from published to revision
                if (ExistingPublishedPrompt.ResultPromptViewModel.FixedList != null)
                {
                    var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                            x => x.ResultPromptViewModel.FixedList != null &&
                                 (x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Published ||
                                 x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending) &&
                                 x.ResultPromptViewModel.FixedList.UUID == Prompt.FixedList.UUID);

                    if (!otherUsages.Any())
                    {
                        ExistingPublishedPrompt.ResultPromptViewModel.FixedList.PublishedStatus = PublishedStatus.RevisionPending;
                    }
                }

                //when required set any associated word groups from published to revision
                if (ExistingPublishedPrompt.ResultPromptViewModel.WordGroups != null)
                {
                    foreach (var rpwg in ExistingPublishedPrompt.ResultPromptViewModel.WordGroups)
                    {
                        //match to a word group for the existing prompt
                        var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                                x => x.ResultPromptViewModel.WordGroups != null &&
                                     (x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Published ||
                                     x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending) &&
                                     x.ResultPromptViewModel.WordGroups.Select(y => y.UUID).Contains(rpwg.UUID));

                        if (!otherUsages.Any())
                        {
                            rpwg.PublishedStatus = PublishedStatus.RevisionPending;
                        }
                    }
                }
            }

            //new draft moves to published pending status
            Prompt.PublishedStatus = PublishedStatus.PublishedPending;
        }

        internal void SetPromptChanges(ResultPromptRuleViewModel published)
        {
            if (changesSet) { return; }

            changesSet = true;

            //trigger the setting of fixed list changes
            if (DraftFixedList != null)
            {
                DraftFixedList.SetChanges(published);
            }

            //trigger the setting of Result Prompt Word Group Changes
            if (DraftResultPromptWordGroups != null)
            {
                foreach(var item in DraftResultPromptWordGroups)
                {
                    item.SetChanges();
                }
            }

            ExistingPublishedPrompt = published;

            if (published == null)
            {
                //new item
                IsChanged = true;
                IsAdded = true;
                ChangeReport = "New Result Prompt Added";
                return;
            }

            var publishedPromptData = published.ResultPromptRule.ResultPrompt;
            var draftPromptData = Prompt.ResultPrompt;

            if (publishedPromptData != draftPromptData)
            {
                //changes made, make a change report, set the prompt as changed
                ChangeReport = publishedPromptData.GetChangedProperties(draftPromptData);
                IsChanged = true;
            }
        }

        internal void SetPublicationComment()
        {
            if (publicationCommentSet) { return; }

            //mark this prompt to indicate that the comment has been set
            publicationCommentSet = true;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Draft revision published pending at {0}.", Prompt.PublishedStatusDate.ToString("d MMM yyyy h:mm:ss tt")));
            sb.AppendLine(string.Format("New revision identifier: '{0}'.", Prompt.UUID));
            if (Prompt.PublicationTags != null && Prompt.PublicationTags.Count > 0)
            {
                sb.AppendLine("The following publication tags were applied...");
                foreach (var tag in Prompt.PublicationTags)
                {
                    sb.AppendLine(tag);
                }
            }
            sb.AppendLine();
            sb.AppendLine("The following property changes were applied...");
            sb.AppendLine(ChangeReport);

            //create a new comment 
            var newComment = new CommentViewModel(Prompt);

            //remove double spacing and set the comment note and system type
            newComment.Note = Regex.Replace(sb.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            newComment.SystemCommentType = "Publication";

            //add comment to the parents collection of comments
            Prompt.AssociatedCommentsModel.AddCommentCommand.Execute(newComment);

            //set the publication comment on the fixed list
            if (IsFixedListChanged)
            {
                DraftFixedList.SetPublicationComment();
            }

            //set the publication comment on the changed result definition word groups
            if (HasResultPromptWordGroupChanges)
            {
                foreach(var item in DraftResultPromptWordGroups.Where(x=>x.IsChanged))
                {
                    item.SetPublicationComment();
                }                
            }
        }
    }
}
