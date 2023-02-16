using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using DataLib;
using System.Windows.Input;

namespace Models.ViewModels
{
    public class ResultDefinitionWordGroupViewModel : ViewModelBase
    {
        private ICommand addSynonymCommand, deleteSynonymCommand;
        public ResultDefinitionWordGroupViewModel(ITreeModel treeModel, ResultDefinitionWordGroup resultDefinitionWordGroup)
        {
            this.treeModel = treeModel;
            ResultDefinitionWordGroup = resultDefinitionWordGroup;

            synonyms = new SilentObservableCollection<ResultDefinitionWordSynonymViewModel>(
                from synonym in resultDefinitionWordGroup.Synonyms.FindAll(x => x.DeletedDate == null)
                select new ResultDefinitionWordSynonymViewModel(synonym,IsReadOnly));
            synonyms.CollectionChanged += synonyms_CollectionChanged;

            ParentWordGroups = new List<ResultWordGroupViewModel>();

            //set up the publication tag model
            PublicationTagsModel = new PublicationTagsModel(treeModel, this);

            //set up the comments model
            AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, this);
        }

        public PublicationTagsModel PublicationTagsModel { get; set; }

        public AssociatedCommentsModel AssociatedCommentsModel { get; set; }

        public ICommand AddSynonymCommand
        {
            get
            {
                if (addSynonymCommand == null)
                {
                    addSynonymCommand = new RelayCommand<ResultDefinitionWordSynonymViewModel>(x => this.AddSynonym(x));
                }
                return addSynonymCommand;
            }
        }
        public ICommand DeleteSynonymCommand
        {
            get
            {
                if (deleteSynonymCommand == null)
                {
                    deleteSynonymCommand = new RelayCommand<ResultDefinitionWordSynonymViewModel>(x => this.DeleteSynonym(x));
                }
                return deleteSynonymCommand;
            }
        }

        public ResultDefinitionWordGroup ResultDefinitionWordGroup
        {
            get { return data as ResultDefinitionWordGroup; }
            private set { data = value; }
        }
        public string ResultDefinitionWord
        {
            get => ResultDefinitionWordGroup.ResultDefinitionWord;
            set {

                if (SetProperty(() => ResultDefinitionWordGroup.ResultDefinitionWord == value, () => ResultDefinitionWordGroup.ResultDefinitionWord = value))
                {
                    LastModifiedDate = DateTime.Now;
                    foreach (var parent in ParentWordGroups.Where(x=>x.ParentResultDefinitionVM.PublishedStatus == PublishedStatus.Draft))
                    {
                        parent.SetResultWordChanged();
                    }
                }
            }
        }
        public DateTime? StartDate
        {
            get => ResultDefinitionWordGroup.StartDate;
            set
            {
                if (SetProperty(() => ResultDefinitionWordGroup.StartDate == value, () => ResultDefinitionWordGroup.StartDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }
        internal ResultDefinitionWordGroupViewModel Draft()
        {
            ResultDefinitionWordGroupViewModel res = (ResultDefinitionWordGroupViewModel)MemberwiseClone();

            //make new data
            res.ResultDefinitionWordGroup = res.ResultDefinitionWordGroup.Draft();

            //reset the collection of synonyms
            var newSynonyms = new SilentObservableCollection<ResultDefinitionWordSynonymViewModel>();
            foreach (var item in res.ResultDefinitionWordGroup.Synonyms)
            {
                newSynonyms.Add(new ResultDefinitionWordSynonymViewModel(item,false));
            }
            res.Synonyms = newSynonyms;

            //set a new publication tags model
            res.PublicationTagsModel = new PublicationTagsModel(treeModel, res);

            //Set a new comments model
            res.AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, res);

            //save the vms and data
            treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.Add(res);
            treeModel.AllData.ResultDefinitionWordGroups.Add(res.ResultDefinitionWordGroup);

            return res;
        }
        public DateTime? EndDate
        {
            get => ResultDefinitionWordGroup.EndDate;
            set
            {
                if (SetProperty(() => ResultDefinitionWordGroup.EndDate == value, () => ResultDefinitionWordGroup.EndDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        private SilentObservableCollection<ResultDefinitionWordSynonymViewModel> synonyms;
        public SilentObservableCollection<ResultDefinitionWordSynonymViewModel> Synonyms
        {
            get => synonyms;
            set
            {
                synonyms = value;
                synonyms.CollectionChanged += synonyms_CollectionChanged;
            }
        }

        public ResultDefinitionWordSynonymViewModel SelectedSynonym { get; set; }

        private void synonyms_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var synonymVM = (ResultDefinitionWordSynonymViewModel)item;
                    if (ResultDefinitionWordGroup.Synonyms == null)
                    {
                        ResultDefinitionWordGroup.Synonyms = new List<ResultDefinitionWordSynonym> { synonymVM.ResultDefinitionWordSynonym };
                    }
                    else
                    {
                        ResultDefinitionWordGroup.Synonyms.Add(synonymVM.ResultDefinitionWordSynonym);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var synonymVM = (ResultDefinitionWordSynonymViewModel)item;
                    if (ResultDefinitionWordGroup.Synonyms != null) { ResultDefinitionWordGroup.Synonyms.Remove(synonymVM.ResultDefinitionWordSynonym); }
                }
            }
        }

        private void AddSynonym(ResultDefinitionWordSynonymViewModel syonymVM)
        {
            if (syonymVM == null)
            {
                syonymVM = CreateNewSynonym();
            }
            Synonyms.Add(syonymVM);
        }

        private ResultDefinitionWordSynonymViewModel CreateNewSynonym()
        {
            //Make a new item 
            var data = new ResultDefinitionWordSynonym();
            var newItem = new ResultDefinitionWordSynonymViewModel(data,false);

            //ensure that label is unique
            var duplicateLabel = false;
            int i = 1;
            do
            {
                duplicateLabel = Synonyms.FirstOrDefault(x => x.Synonym == data.Synonym) != null;
                if (duplicateLabel)
                {
                    i++;
                    data.Synonym = string.Format("New Synonym ({0})", i);
                }

            } while (duplicateLabel);

            return newItem;
        }

        private void DeleteSynonym(ResultDefinitionWordSynonymViewModel syonymVM)
        {
            if (syonymVM == null)
            {
                syonymVM = SelectedSynonym;
            }
            if (syonymVM != null)
            {
                Synonyms.Remove(syonymVM);
            }
            SelectedSynonym = null;
            OnPropertyChanged("SelectedSynonym");
        }

        private ResultDefinitionViewModel SelectedResultDefinitionViewModel
        {
            get
            {
                //determine the parent result based on the current selected tree item
                var selectedTreeItem = treeModel.SelectedItem as ResultDefinitionWordGroupTreeViewModel;
                var parentRWGTreeItem = selectedTreeItem.Parent as ResultWordGroupTreeViewModel;
                return parentRWGTreeItem.ResultWordGroupViewModel.ParentResultDefinitionVM;
            }
        }

        public List<ResultWordGroupViewModel> ParentWordGroups { get; }

        #region Copy Members

        public ResultDefinitionWordGroupViewModel Copy()
        {
            ResultDefinitionWordGroupViewModel res = (ResultDefinitionWordGroupViewModel)this.MemberwiseClone();

            //make new data view model
            res.ResultDefinitionWordGroup = res.ResultDefinitionWordGroup.Copy();
            return res;
        }

        #endregion Copy Members
    }
}