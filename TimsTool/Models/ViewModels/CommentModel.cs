using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Identity;
using DataLib;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a Comment object.
    /// </summary>
    public class CommentViewModel : ViewModelBase
    {
        readonly ViewModelBase parent;

        public CommentViewModel(Comment comment, ViewModelBase parent)
        {
            Comment = comment;
            this.parent = parent;
        }

        public CommentViewModel(ViewModelBase parent)
        {
            Comment = new Comment(parent.UUID,parent.MasterUUID);
            Comment.Note = "please enter details of the comment ....";
            this.parent = parent;
            PublishedStatus = PublishedStatus.Draft;
        }

        public Comment Comment
        {
            get { return data as Comment; }
            private set { data = value; }
        }

        #region Comment Properties

        public string Note
        {    
            get => Comment.Note;
            set
                {
                    if (SetProperty(() => Comment.Note == value, () => Comment.Note = value))
                    {
                        LastModifiedDate = DateTime.Now;
                        OnPropertyChanged("MadeBy");
                    }
                }
        }

        public string SystemCommentType
        {
            get => Comment.SystemCommentType;
            set
            {
                if (SetProperty(() => Comment.SystemCommentType == value, () => Comment.SystemCommentType = value))
                {
                    LastModifiedDate = DateTime.Now;
                    OnPropertyChanged("MadeBy");
                    OnPropertyChanged("IsReadOnly");
                }
            }
        }

        public bool IsSystemGenerated
        {
            get
            {
                return !string.IsNullOrEmpty(SystemCommentType);
            }
        }

        public string MadeBy
        {
            get
            {
                return string.Format("{0}{1}Created: {2}{3}Modified: {4}", IsSystemGenerated ? "System Generated" : LastModifiedUser,
                    Environment.NewLine,
                    CreatedDate.Value.ToString("d MMM yyyy h:mm:ss tt"),
                    Environment.NewLine,
                    LastModifiedDate.Value.ToString("d MMM yyyy h:mm:ss tt"));
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if (Comment.ParentUUID == parent.UUID
                    && parent.PublishedStatus == PublishedStatus.Draft
                    && string.IsNullOrEmpty(SystemCommentType) 
                    && Comment.CreatedUser == IdentityHelper.SignedInUser.Email)
                {
                    return false;
                }
                return true;
            }
        }

        #endregion // Comment Properties
    }
}