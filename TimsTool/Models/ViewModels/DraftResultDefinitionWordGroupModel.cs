using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public class DraftResultDefinitionWordGroupModel
    {
        private bool publicationCommentSet;
        public DraftResultDefinitionWordGroupModel(
            ResultDefinitionWordGroupViewModel draft, 
            ResultDefinitionWordGroupViewModel existingPublished, 
            bool isChanged, 
            bool isChangedWithWordChanges,
            ResultWordGroupViewModel draftRwg, 
            string changeReport)
        {
            DraftResultDefinitionWordGroupViewModel = draft;
            ExistingPublished = existingPublished;
            IsChanged = isChanged;
            IsChangedWithWordChanges = isChangedWithWordChanges;
            DraftResultWordGroupViewModel = draftRwg;
            ChangeReport = changeReport;
        }

        public List<ResultDefinitionViewModel> ParentResults
        {
            get
            {
                return new List<ResultDefinitionViewModel>(ExistingPublished.ParentWordGroups.Select(x=>x.ParentResultDefinitionVM));
            }
        }

        public List<ResultWordGroupViewModel> ParentWordGroups
        {
            get
            {
                return ExistingPublished.ParentWordGroups;
            }
        }

        public ResultDefinitionWordGroupViewModel DraftResultDefinitionWordGroupViewModel { get; }

        public ResultDefinitionWordGroupViewModel ExistingPublished { get; }

        public ResultWordGroupViewModel DraftResultWordGroupViewModel { get; }

        public bool IsChanged { get; }

        public bool IsChangedWithWordChanges { get; }

        public string ChangeReport { get; }

        internal void PrepareForPublication()
        {
            DraftResultDefinitionWordGroupViewModel.PublishedStatus = PublishedStatus.PublishedPending;

            foreach (var synonymn in DraftResultDefinitionWordGroupViewModel.Synonyms)
            {
                synonymn.IsReadOnly = true;
            }

            if (ExistingPublished != null)
            {
                ExistingPublished.PublishedStatus = PublishedStatus.RevisionPending;
            }

            SetPublicationComment();
        }

        internal void SetPublicationComment()
        {
            if (publicationCommentSet) { return; }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Draft revision published pending at {0}.", DraftResultDefinitionWordGroupViewModel.PublishedStatusDate.ToString("d MMM yyyy h:mm:ss tt")));
            sb.AppendLine(string.Format("Revision identifier: '{0}'.", DraftResultDefinitionWordGroupViewModel.UUID));
            if (DraftResultDefinitionWordGroupViewModel.PublicationTags != null && DraftResultDefinitionWordGroupViewModel.PublicationTags.Count > 0)
            {
                sb.AppendLine("The following publication tags were applied...");
                foreach (var tag in DraftResultDefinitionWordGroupViewModel.PublicationTags)
                {
                    sb.AppendLine(tag);
                }
            }
            sb.AppendLine();
            sb.AppendLine("The following property changes were applied...");
            sb.AppendLine(ChangeReport);

            //create a new comment 
            var newComment = new CommentViewModel(DraftResultDefinitionWordGroupViewModel);

            //remove double spacing and set the comment note and system type
            newComment.Note = Regex.Replace(sb.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            newComment.SystemCommentType = "Publication";

            //add comment to the parents collection of comments
            DraftResultDefinitionWordGroupViewModel.AssociatedCommentsModel.AddCommentCommand.Execute(newComment);

            //mark this prompt to indicate that the comment has been set
            publicationCommentSet = true;
        }

        internal void ResetPublished()
        {
            var index = DraftResultWordGroupViewModel.ResultDefinitionWordGroups.IndexOf(DraftResultDefinitionWordGroupViewModel);
            DraftResultWordGroupViewModel.ResultDefinitionWordGroups.RemoveAt(index);
            DraftResultWordGroupViewModel.ResultDefinitionWordGroups.Insert(index, ExistingPublished);
        }
    }
}
