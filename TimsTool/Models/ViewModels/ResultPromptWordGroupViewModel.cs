using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using DataLib;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Text;

namespace Models.ViewModels
{
    public class ResultPromptWordGroupViewModel : ViewModelBase
    {
        private ICommand addSynonymCommand, deleteSynonymCommand;

        public ResultPromptWordGroupViewModel(ITreeModel treeModel, ResultPromptWordGroup resultPromptWordGroup)
        {
            this.treeModel = treeModel;
            ResultPromptWordGroup = resultPromptWordGroup;
            synonyms = new SilentObservableCollection<ResultPromptWordSynonymViewModel>(
                (from synonym in resultPromptWordGroup.Synonyms.FindAll(x => x.DeletedDate == null)
                 select new ResultPromptWordSynonymViewModel(synonym, IsReadOnly))
                .ToList());

            ParentPrompts = new List<ResultPromptViewModel>();

            synonyms.CollectionChanged += synonyms_CollectionChanged;

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
                    addSynonymCommand = new RelayCommand<ResultPromptWordSynonymViewModel>(x => this.AddSynonym(x));
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
                    deleteSynonymCommand = new RelayCommand<ResultPromptWordSynonymViewModel>(x => this.DeleteSynonym(x));
                }
                return deleteSynonymCommand;
            }
        }

        public ResultPromptWordGroup ResultPromptWordGroup
        {
            get { return data as ResultPromptWordGroup; }
            private set { data = value; }
        }

        public string ResultPromptWord
        {
            get => ResultPromptWordGroup.ResultPromptWord;
            set
            {
                if (SetProperty(() => ResultPromptWordGroup.ResultPromptWord == value, () => ResultPromptWordGroup.ResultPromptWord = value))
                {
                    LastModifiedDate = DateTime.Now;

                    //set the parents as modified
                    foreach (var parent in ParentPrompts.Where(x=>x.PublishedStatus == PublishedStatus.Draft))
                    {
                        parent.LastModifiedDate = DateTime.Now;
                    }
                }
            }
        }

        public DateTime? StartDate
        {
            get => ResultPromptWordGroup.StartDate;
            set
            {
                if (SetProperty(() => ResultPromptWordGroup.StartDate == value, () => ResultPromptWordGroup.StartDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        internal ResultPromptWordGroupViewModel Draft()
        {
            ResultPromptWordGroupViewModel res = (ResultPromptWordGroupViewModel)MemberwiseClone();

            //make new data
            res.ResultPromptWordGroup = res.ResultPromptWordGroup.Draft();

            //reset the collection of Synonyms
            var newSynonyms = new SilentObservableCollection<ResultPromptWordSynonymViewModel>();
            foreach (var item in res.ResultPromptWordGroup.Synonyms)
            {
                newSynonyms.Add(new ResultPromptWordSynonymViewModel(item, false));
            }
            res.Synonyms = newSynonyms;

            //set a new publication tags model
            res.PublicationTagsModel = new PublicationTagsModel(treeModel, res);

            //Set a new comments model
            res.AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, res);

            //reset the collection of parent result prompts
            res.ParentPrompts = new List<ResultPromptViewModel>();

            //save the vms and data
            treeModel.AllResultPromptWordGroupViewModel.WordGroups.Add(res);
            treeModel.AllData.ResultPromptWordGroups.Add(res.ResultPromptWordGroup);

            return res;
        }

        public DateTime? EndDate
        {
            get => ResultPromptWordGroup.EndDate;
            set
            {
                if (SetProperty(() => ResultPromptWordGroup.EndDate == value, () => ResultPromptWordGroup.EndDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        private SilentObservableCollection<ResultPromptWordSynonymViewModel> synonyms;

        public List<ResultPromptViewModel> ParentPrompts { get; set; }

        public SilentObservableCollection<ResultPromptWordSynonymViewModel> Synonyms
        {
            get => synonyms;
            set
            {
                synonyms = value;
                synonyms.CollectionChanged += synonyms_CollectionChanged;
            }
        }

        public ResultPromptWordSynonymViewModel SelectedSynonym { get; set; }

        private void synonyms_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var synonymVM = (ResultPromptWordSynonymViewModel)item;
                    if (ResultPromptWordGroup.Synonyms == null)
                    {
                        ResultPromptWordGroup.Synonyms = new List<ResultPromptWordSynonym> { synonymVM.ResultPromptWordSynonym };
                    }
                    else
                    {
                        ResultPromptWordGroup.Synonyms.Add(synonymVM.ResultPromptWordSynonym);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var synonymVM = (ResultPromptWordSynonymViewModel)item;
                    if (ResultPromptWordGroup.Synonyms != null) { ResultPromptWordGroup.Synonyms.Remove(synonymVM.ResultPromptWordSynonym); }
                }
            }
        }

        private void AddSynonym(ResultPromptWordSynonymViewModel syonymVM)
        {
            if (syonymVM == null)
            {
                syonymVM = CreateNewSynonym();
            }
            Synonyms.Add(syonymVM);
        }

        private ResultPromptWordSynonymViewModel CreateNewSynonym()
        {
            //Make a new item 
            var data = new ResultPromptWordSynonym();
            var newItem = new ResultPromptWordSynonymViewModel(data, false);

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

        private void DeleteSynonym(ResultPromptWordSynonymViewModel syonymVM)
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

        #region Copy Members

        public ResultPromptWordGroupViewModel Copy()
        {
            ResultPromptWordGroupViewModel res = (ResultPromptWordGroupViewModel)this.MemberwiseClone();

            //make new data view model
            res.ResultPromptWordGroup = res.ResultPromptWordGroup.Copy();
            return res;
        }

        #endregion Copy Members
    }
}