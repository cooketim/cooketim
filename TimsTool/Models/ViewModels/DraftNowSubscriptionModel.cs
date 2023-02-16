using DataLib;
using Models.Commands;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public class DraftNowSubscriptionModel
    {
        public NowSubscriptionTreeViewModel DraftNowSub { get; }
        public NowSubscriptionTreeViewModel ExistingPublishedNowSub { get; }
        public bool SavePending { get; }
        public string ChangeReport { get; private set; }
        public bool IsChanged { get; private set; }

        private ITreeModel treeModel;

        public DraftNowSubscriptionModel(ITreeModel treeModel, NowSubscriptionTreeViewModel draftNowSub, NowSubscriptionTreeViewModel existingPublishedNowSub, bool savePending)
        {
            this.treeModel = treeModel;
            DraftNowSub = draftNowSub;
            ExistingPublishedNowSub = existingPublishedNowSub;
            SavePending = savePending;
            SetChanges();
        }

        private void SetChanges()
        {
            //Set any changes
            if (ExistingPublishedNowSub == null)
            {
                //draft must be a new item
                IsChanged = true;
                ChangeReport = string.Format("New {0} Subscription Added", DraftNowSub.NowSubscriptionViewModel.SubscriptionType);
                return;
            }

            var publishedData = ExistingPublishedNowSub.NowSubscriptionViewModel.NowSubscription;
            var draftData = DraftNowSub.NowSubscriptionViewModel.NowSubscription;
            var changes = publishedData.GetChangedProperties(draftData);
            if (!string.IsNullOrEmpty(changes))
            {
                ChangeReport = changes.ToString().Trim();
                IsChanged = true;
            }
        }

        internal void PrepareForPublication()
        {
            if (ExistingPublishedNowSub != null)
            {
                //existing published moves to revision status
                ExistingPublishedNowSub.NowSubscriptionViewModel.PublishedStatus = PublishedStatus.RevisionPending;
            }

            //new draft moves to published pending status
            DraftNowSub.NowSubscriptionViewModel.PublishedStatus = PublishedStatus.PublishedPending;

            //Set the publication comment
            SetPublicationComment();

            //when required, i.e. when the draft Subscription has been created via a usage, save the now to the relevant collection
            if (!SubscriptionViewExists(DraftNowSub.NowSubscriptionViewModel))
            {
                var nowSubCmd = treeModel.DraftNowSubscriptionCommand as DraftNowSubscriptionCommand;
                nowSubCmd.SaveDraft(DraftNowSub, false);
            }
        }

        private bool SubscriptionViewExists(NowSubscriptionViewModel item)
        {
            if (item.IsNow)
            {
                return treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsEDT)
            {
                return treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsCourtRegister)
            {
                return treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsPrisonCourtRegister)
            {
                return treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsInformantRegister)
            {
                return treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            return false;
        }

        private void SetPublicationComment()
        {
            //set the publication comment
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Draft revision published pending at {0}.", DraftNowSub.NowSubscriptionViewModel.PublishedStatusDate.ToString("d MMM yyyy h:mm:ss tt")));
            sb.AppendLine(string.Format("New revision identifier: '{0}'.", DraftNowSub.NowSubscriptionViewModel.UUID));
            if (DraftNowSub.NowSubscriptionViewModel.PublicationTags != null && DraftNowSub.NowSubscriptionViewModel.PublicationTags.Count > 0)
            {
                sb.AppendLine("The following publication tags were applied...");
                foreach (var tag in DraftNowSub.NowSubscriptionViewModel.PublicationTags)
                {
                    sb.AppendLine(tag);
                }
            }
            sb.AppendLine();
            sb.AppendLine("The following property changes were applied...");
            sb.AppendLine(ChangeReport);

            //create a new comment 
            var newComment = new CommentViewModel(DraftNowSub.NowSubscriptionViewModel);

            //remove double spacing and set the comment note and system type
            newComment.Note = Regex.Replace(sb.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            newComment.SystemCommentType = "Publication";

            //add comment to the parents collection of comments
            DraftNowSub.NowSubscriptionViewModel.AssociatedCommentsModel.AddCommentCommand.Execute(newComment);

            //reset the children on the draft so that they are rebuilt
            DraftNowSub.ResetChildren();
        }
    }
}
