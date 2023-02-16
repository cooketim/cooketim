using DataLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public class DraftFixedListModel
    {
        private bool changesSet;
        private bool publicationCommentSet;
        private ITreeModel treeModel;

        public DraftFixedListModel(ITreeModel treeModel, FixedListViewModel source)
        {
            this.treeModel = treeModel;
            DraftFixedList = source;
        }

        public bool IsChanged { get; private set; }
        public FixedListViewModel DraftFixedList { get; }
        public FixedListViewModel ExistingPublishedFixedList { get; private set; }
        public string ChangeReport { get; private set; }
        public List<ResultPromptRuleViewModel> PublishedParentResultPromptRules
        { 
            get
            {
                return treeModel.AllResultPromptViewModel.Prompts.Where(
                        x => x.ResultPromptViewModel.FixedList != null &&
                            x.ResultPromptViewModel.FixedListMasterUUID == DraftFixedList.MasterUUID &&
                            (x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Published ||
                            x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending)).ToList();
            }
        }

        internal void PrepareForPublication()
        {
            DraftFixedList.PublishedStatus = PublishedStatus.PublishedPending;
            if (ExistingPublishedFixedList != null)
            {
                ExistingPublishedFixedList.PublishedStatus = PublishedStatus.RevisionPending;
            }

            SetPublicationComment();
        }

        internal void SetChanges(ResultPromptRuleViewModel published)
        {
            if (changesSet) { return; }

            changesSet = true;

            //ensure that the fixed list is not deleted
            if (DraftFixedList.IsDeleted)
            {
                IsChanged = false;
            }

            if (published == null || published.ResultPromptViewModel.FixedList == null)
            {
                //new item
                IsChanged = true;
                ChangeReport = "New Fixed List Added";
                return;
            }

            ExistingPublishedFixedList = published.ResultPromptViewModel.FixedList;

            var publishedFixedListData = ExistingPublishedFixedList.FixedList;
            var draftFixedListData = DraftFixedList.FixedList;

            if (publishedFixedListData != draftFixedListData)
            {
                //changes made, make a change report, set the result as changed
                ChangeReport = publishedFixedListData.GetChangedProperties(draftFixedListData);
                IsChanged = true;
            }
        }

        internal void SetPublicationComment()
        {
            if (publicationCommentSet) { return; }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Draft revision published pending at {0}.", DraftFixedList.PublishedStatusDate.ToString("d MMM yyyy h:mm:ss tt")));
            sb.AppendLine(string.Format("New revision identifier: '{0}'.", DraftFixedList.UUID));
            if (DraftFixedList.PublicationTags != null && DraftFixedList.PublicationTags.Count > 0)
            {
                sb.AppendLine("The following publication tags were applied...");
                foreach (var tag in DraftFixedList.PublicationTags)
                {
                    sb.AppendLine(tag);
                }
            }
            sb.AppendLine();
            sb.AppendLine("The following property changes were applied...");
            sb.AppendLine(ChangeReport);

            //create a new comment 
            var newComment = new CommentViewModel(DraftFixedList);

            //remove double spacing and set the comment note and system type
            newComment.Note = Regex.Replace(sb.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            newComment.SystemCommentType = "Publication";

            //add comment to the parents collection of comments
            DraftFixedList.AssociatedCommentsModel.AddCommentCommand.Execute(newComment);

            //mark this prompt to indicate that the comment has been set
            publicationCommentSet = true;
        }
    }
}
