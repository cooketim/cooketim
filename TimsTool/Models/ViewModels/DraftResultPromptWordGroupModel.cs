using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public class DraftResultPromptWordGroupModel
    {
        private bool changesSet;
        private bool publicationCommentSet;
        private ITreeModel treeModel;

        public DraftResultPromptWordGroupModel(ITreeModel treeModel, ResultPromptWordGroupViewModel source)
        {
            this.treeModel = treeModel;
            DraftResultPromptWordGroup = source;
        }

        public bool IsChanged { get; private set; }
        public bool IsChangedWithWordChanges { get; private set; }
        public ResultPromptWordGroupViewModel DraftResultPromptWordGroup { get; }
        public ResultPromptWordGroupViewModel ExistingPublishedResultPromptWordGroup { get; private set; }
        public string ChangeReport { get; private set; }

        public object ParentResultPrompts
        {
            get
            {
                return (from rpr in treeModel.AllResultPromptViewModel.Prompts
                           join rdPub in treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished
                             on rpr.ResultDefinitionViewModel.UUID equals rdPub.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                           where rpr.ResultPromptViewModel.WordGroups != null
                              && rpr.ResultPromptViewModel.IsDeleted == false
                              && rpr.IsDeleted == false
                              && rpr.ResultPromptViewModel.WordGroups.Select(x => x.MasterUUID).Contains(DraftResultPromptWordGroup.MasterUUID)
                              && rpr.ResultDefinitionViewModel.IsDeleted == false
                        select new { PublishedResultDefinitionTree = rdPub, PublishedResultPromptViewModel = rpr.ResultPromptViewModel })
                           .OrderBy(x=>x.PublishedResultDefinitionTree.ResultRuleViewModel.ChildResultDefinitionViewModel.Label).ToList();
            }
        }

        internal void PrepareForPublication()
        {
            DraftResultPromptWordGroup.PublishedStatus = PublishedStatus.PublishedPending;

            foreach (var synonymn in DraftResultPromptWordGroup.Synonyms)
            {
                synonymn.IsReadOnly = true;
            }

            if (ExistingPublishedResultPromptWordGroup != null)
            {
                ExistingPublishedResultPromptWordGroup.PublishedStatus = PublishedStatus.RevisionPending;
            }

            SetPublicationComment();
        }

        internal void SetChanges()
        {
            if (changesSet) { return; }

            changesSet = true;

            //ensure that the word group is not deleted
            if (DraftResultPromptWordGroup.IsDeleted)
            {
                IsChanged = false;
            }


            //find a published revision
            ExistingPublishedResultPromptWordGroup = treeModel.AllResultPromptWordGroupViewModel.WordGroups.FirstOrDefault(
                x => x.MasterUUID == DraftResultPromptWordGroup.MasterUUID &&
                    (x.PublishedStatus == PublishedStatus.Published ||
                            x.PublishedStatus == PublishedStatus.PublishedPending)
                );

            if (ExistingPublishedResultPromptWordGroup == null)
            {
                //new item
                ChangeReport = "New Result Prompt Word Group Added";
                IsChanged = true;
                IsChangedWithWordChanges = true;
                return;
            }

            var publishedData = ExistingPublishedResultPromptWordGroup.ResultPromptWordGroup;
            var draftData = DraftResultPromptWordGroup.ResultPromptWordGroup;

            if (publishedData != draftData)
            {
                //changes made, make a change report, set the result as changed
                ChangeReport = publishedData.GetChangedProperties(draftData);
                IsChanged = true;

                if (!string.Equals(publishedData.ResultPromptWord,draftData.ResultPromptWord, StringComparison.InvariantCulture))
                {
                    IsChangedWithWordChanges = true;
                }
            }
        }

        internal void SetPublicationComment()
        {
            if (publicationCommentSet) { return; }

            //mark this prompt to indicate that the comment has been set
            publicationCommentSet = true;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Draft revision published pending at {0}.", DraftResultPromptWordGroup.PublishedStatusDate.ToString("d MMM yyyy h:mm:ss tt")));
            sb.AppendLine(string.Format("New revision identifier: '{0}'.", DraftResultPromptWordGroup.UUID));
            if (DraftResultPromptWordGroup.PublicationTags != null && DraftResultPromptWordGroup.PublicationTags.Count > 0)
            {
                sb.AppendLine("The following publication tags were applied...");
                foreach (var tag in DraftResultPromptWordGroup.PublicationTags)
                {
                    sb.AppendLine(tag);
                }
            }
            sb.AppendLine();
            sb.AppendLine("The following property changes were applied...");
            sb.AppendLine(ChangeReport);

            //create a new comment 
            var newComment = new CommentViewModel(DraftResultPromptWordGroup);

            //remove double spacing and set the comment note and system type
            newComment.Note = Regex.Replace(sb.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            newComment.SystemCommentType = "Publication";

            //add comment to the parents collection of comments
            DraftResultPromptWordGroup.AssociatedCommentsModel.AddCommentCommand.Execute(newComment);
        }
    }
}
