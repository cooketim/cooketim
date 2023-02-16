using System.Windows.Input;
using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Command;

namespace Models.ViewModels
{
    public class PublicationTagsModel : ViewModelBase
    {
        private ViewModelBase parentModel;
        private ICommand addTagCommand, addNewTagCommand, removeTagCommand;

        //command for adding a tag to the collection of selected tags
        public ICommand AddTagCommand
        {
            get
            {
                if (addTagCommand == null)
                {
                    addTagCommand = new RelayCommand<string>(x => AddTag(x));
                }
                return addTagCommand;
            }
        }

        //command for adding a new tag to the collection of selected tags and available tags
        public ICommand AddNewTagCommand
        {
            get
            {
                if (addNewTagCommand == null)
                {
                    addNewTagCommand = new RelayCommand(AddNewTag, AddNewTagEnabled);
                }
                return addNewTagCommand;
            }
        }

        //command for removing a tag from the collection of selected tags
        public ICommand RemoveTagCommand
        {
            get
            {
                if (removeTagCommand == null)
                {
                    removeTagCommand = new RelayCommand(this.RemoveTag, RemoveTagEnabled);
                }
                return removeTagCommand;
            }
        }

        public PublicationTagsModel(DataPatchByTagViewModel parentModel, List<string> pendingTags)
        {
            this.parentModel = parentModel;

            //create the collection of selected tags 
            if (parentModel != null && parentModel.PublicationTags != null)
            {
                SelectedPublicationTags = new SilentObservableCollection<string>(parentModel.PublicationTags.OrderBy(x => x));
            }
            else
            {
                SelectedPublicationTags = new SilentObservableCollection<string>();
            }

            //create the collection for the collection of available tags based on the avilable pending tags
            AvailablePublicationTags = pendingTags == null ? new SilentObservableCollection<string>() :
                    new SilentObservableCollection<string>(pendingTags.Except(SelectedPublicationTags).OrderBy(x => x));

            SelectedPublicationTags.CollectionChanged += selectedPublicationTags_CollectionChanged;
        }

        public PublicationTagsModel(ITreeModel treeModel, ViewModelBase parentModel)
        {
            this.treeModel = treeModel;
            this.parentModel = parentModel;            

            //create the collection of selected tags 
            if (parentModel != null && parentModel.PublicationTags != null)
            {
                SelectedPublicationTags = new SilentObservableCollection<string>(parentModel.PublicationTags.OrderBy(x=>x));
            }
            else
            {
                SelectedPublicationTags = new SilentObservableCollection<string>();
            }

            //when the parent model is draft, enable events and collections for editting
            if (parentModel == null || parentModel.PublishedStatus == PublishedStatus.Draft)
            {
                //load a copy of all the publication tags, excluding the selected tags, UI will bind to this to make its selections
                AvailablePublicationTags = treeModel.AllPublicationTags == null ? new SilentObservableCollection<string>() :
                    new SilentObservableCollection<string>(treeModel.AllPublicationTags.Except(SelectedPublicationTags).OrderBy(x => x));

                SelectedPublicationTags.CollectionChanged += selectedPublicationTags_CollectionChanged;

                //listen for changes to the master collection of tags to observe when a tag is added or removed and synchronise the AllTags collection
                if (treeModel.AllPublicationTags != null)
                {
                    treeModel.AllPublicationTags.CollectionChanged += masterPublicationTags_CollectionChanged;
                }
            }
        }

        public bool RemoveTagEnabled
        {
            get => selectedTag != null ? true : false;
        }

        public bool AddNewTagEnabled
        {
            get => newTag != null ? true : false;
        }

        public bool DraftOKEnabled
        {
            get => SelectedPublicationTags.Count > 0;
        }

        public bool AddTagEnabled
        {
            get => selectedAvailableTag != null ? true : false;
        }

        private string selectedAvailableTag = null;
        public string SelectedAvailableTag 
        {
            get => selectedAvailableTag;
            set
            {
                SetProperty(ref selectedAvailableTag, value);
                OnPropertyChanged("AddTagEnabled");
            }
        }

        private string newTag = null;
        public string NewTag
        {
            get => newTag;
            set
            {
                SetProperty(ref newTag, value);
                OnPropertyChanged("AddNewTagEnabled");
            }
        }

        private string selectedTag = null;

        public string SelectedTag
        {
            get => selectedTag;
            set
            {
                SetProperty(ref selectedTag, value);
                OnPropertyChanged("RemoveTagEnabled");
            }
        }

        public SilentObservableCollection<string> AvailablePublicationTags { get; set; }

        public SilentObservableCollection<string> SelectedPublicationTags { get; set; }

        private void masterPublicationTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var tag = (string)item;
                    //ensure that the tag is not already selected
                    var match = SelectedPublicationTags.FirstOrDefault(x => x.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                    if (match == null)
                    {
                        AvailablePublicationTags.Add(tag);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var tag = (string)item;
                    AvailablePublicationTags.Remove(tag);
                    SelectedPublicationTags.Remove(tag);
                }
            }

            OnPropertyChanged("AvailablePublicationTags");
            OnPropertyChanged("AddTagEnabled");
        }

        private void selectedPublicationTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var tag = (string)item;
                    if (parentModel != null)
                    {
                        if (parentModel.PublicationTags == null)
                        {
                            parentModel.PublicationTags = new List<string>() { tag };
                        }
                        else
                        {
                            parentModel.PublicationTags.Add(tag);
                        }
                    }

                    //remove from the available tags
                    AvailablePublicationTags.Remove(tag);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var tag = (string)item;
                    if (parentModel != null)
                    {
                        if (parentModel.PublicationTags != null) { parentModel.PublicationTags.Remove(tag); }
                        if (parentModel.PublicationTags.Count() == 0) { parentModel.PublicationTags = null; }
                    }

                    //add to the available tags
                    AvailablePublicationTags.Add(tag);
                }
            }

            OnPropertyChanged("SelectedPublicationTags");
            OnPropertyChanged("DraftOKEnabled");
        }

        private void AddTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && !SelectedPublicationTags.Contains(tag))
            {
                SelectedPublicationTags.Add(tag);
                return;
            }
            if (!string.IsNullOrEmpty(selectedAvailableTag) && !SelectedPublicationTags.Contains(selectedAvailableTag))
            {
                SelectedPublicationTags.Add(selectedAvailableTag);
            }
        }

        private void AddNewTag()
        {
            if (newTag != null)
            {
                var match = AvailablePublicationTags.FirstOrDefault(x => x.Equals(newTag,StringComparison.InvariantCultureIgnoreCase));
                if (match == null)
                {
                    //add to the selected items
                    var matchedSelection = SelectedPublicationTags.FirstOrDefault(x => x.Equals(newTag, StringComparison.InvariantCultureIgnoreCase));
                    if (matchedSelection == null)
                    {
                        SelectedPublicationTags.Add(newTag);
                    }
                }
            }
        }

        private void RemoveTag()
        {
            if (selectedTag != null)
            {
                SelectedPublicationTags.Remove(selectedTag);
            }
        }
    }
}
