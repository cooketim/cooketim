using System.Windows.Input;
using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Command;

namespace Models.ViewModels
{
    public class AssociatedCommentsModel : ViewModelBase
    {
        private ViewModelBase parentModel;
        private ICommand addCommentCommand, deleteCommentCommand;

        public ICommand AddCommentCommand
        {
            get
            {
                if (addCommentCommand == null)
                {
                    addCommentCommand = new RelayCommand<CommentViewModel>(x => AddComment(x));
                }
                return addCommentCommand;
            }
        }
        public ICommand DeleteCommentCommand
        {
            get
            {
                if (deleteCommentCommand == null)
                {
                    deleteCommentCommand = new RelayCommand<CommentViewModel>(x => DeleteComment(x));
                }
                return deleteCommentCommand;
            }
        }

        public AssociatedCommentsModel(ITreeModel treeModel, ViewModelBase parentModel)
        {
            this.parentModel = parentModel;
            this.treeModel = treeModel;

            //set up the comments collection
            comments = new SilentObservableCollection<CommentViewModel>();
            if (treeModel.AllData.Comments != null)
            {
                foreach (var item in treeModel.AllData.Comments.Where(x => x.ParentMasterUUID == parentModel.MasterUUID).OrderBy(x => x.LastModifiedDate))
                {
                    comments.Add(new CommentViewModel(item,parentModel));
                }
            }
            comments.CollectionChanged += comments_CollectionChanged;
        }

        private SilentObservableCollection<CommentViewModel> comments;

        public SilentObservableCollection<CommentViewModel> Comments
        {
            get => comments;
            set
            {
                comments = value;
                comments.CollectionChanged += comments_CollectionChanged;
            }
        }

        public CommentViewModel SelectedComment { get; set; }

        private void AddComment(CommentViewModel commentVM)
        {
            if (commentVM == null)
            {
                commentVM = new CommentViewModel(parentModel);
            }
            Comments.Add(commentVM);
        }

        private void DeleteComment(CommentViewModel commentVM)
        {
            if (commentVM == null)
            {
                commentVM = SelectedComment;
            }
            if (commentVM != null)
            {
                Comments.Remove(commentVM);
            }
            SelectedComment = null;
            OnPropertyChanged("SelectedComment");
        }

        private void comments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var commentVM = (CommentViewModel)item;
                    if (treeModel.AllData.Comments == null)
                    {
                        treeModel.AllData.Comments = new List<Comment>() { commentVM.Comment };
                    }
                    else
                    {
                        treeModel.AllData.Comments.Add(commentVM.Comment);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var commentVM = (CommentViewModel)item;
                    if (treeModel.AllData.Comments != null) { treeModel.AllData.Comments.Remove(commentVM.Comment); }
                }
            }
        }

        public override bool IsReadOnly
        {
            get => parentModel.PublishedStatus == DataLib.PublishedStatus.Draft ? false : true;
        }
        public override bool IsEnabled
        {
            get => !IsReadOnly;
        }
    }
}
