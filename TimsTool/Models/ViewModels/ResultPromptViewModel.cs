using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DataLib;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace Models.ViewModels
{
    public class ResultPromptViewModel : ViewModelBase
    {
        AllResultPromptWordGroupViewModel allResultPromptWordGroups;
        private ICommand addDurationElementCommand, deleteDurationElementCommand;

        public ResultPromptViewModel(ITreeModel treeModel, ResultPrompt resultPrompt, AllResultPromptWordGroupViewModel allResultPromptWordGroups)
        {
            this.treeModel = treeModel;
            ResultPrompt = resultPrompt;
            SetDurationElement();
            this.allResultPromptWordGroups = allResultPromptWordGroups;
            LoadAssociatedModels();
        }

        public AssociatedCommentsModel AssociatedCommentsModel { get; set; }

        public ICommand AddDurationElementCommand
        {
            get
            {
                if (addDurationElementCommand == null)
                {
                    addDurationElementCommand = new RelayCommand<DurationElementViewModel>(x => this.AddDurationElement(x));
                }
                return addDurationElementCommand;
            }
        }
        public ICommand DeleteDurationElementCommand
        {
            get
            {
                if (deleteDurationElementCommand == null)
                {
                    deleteDurationElementCommand = new RelayCommand<DurationElementViewModel>(x => this.DeleteDurationElement(x));
                }
                return deleteDurationElementCommand;
            }
        }

        private void AddDurationElement(DurationElementViewModel deVM)
        {
            if (deVM == null)
            {
                deVM = CreateNewDurationElement();
            }
            DurationElements.Add(deVM);

            LastModifiedDate = DateTime.Now;
        }

        private DurationElementViewModel CreateNewDurationElement()
        {
            //ensure that label is unique
            string val = "New Duration";
            var duplicateValue = false;
            int i = 1;
            do
            {
                duplicateValue = DurationElements.FirstOrDefault(x => x.DurationElement == val) != null;
                if (duplicateValue)
                {
                    i++;
                    val = string.Format("New Duration ({0})", i);
                }

            } while (duplicateValue);

            var data = new ResultPromptDuration() { DurationElement = val };
            var vm = new DurationElementViewModel(data, this);

            return vm;
        }

        private void DeleteDurationElement(DurationElementViewModel deVM)
        {
            if (deVM == null)
            {
                deVM = SelectedDurationElement;
            }
            if (deVM != null)
            {
                DurationElements.Remove(deVM);
            }
            SelectedDurationElement = null;
            OnPropertyChanged("SelectedDurationElement");

            LastModifiedDate = DateTime.Now;
        }

        private void ClearDurationElements()
        {
            if (DurationElements == null || !DurationElements.Any())
            {
                return;
            }
            DurationElements.Clear();

            LastModifiedDate = DateTime.Now;
        }

        private SilentObservableCollection<DurationElementViewModel> durationElements;

        public SilentObservableCollection<DurationElementViewModel> DurationElements
        {
            get => durationElements;
            set
            {
                durationElements = value;
                durationElements.CollectionChanged += durationElements_CollectionChanged;
            }
        }
        private void SetDurationElement()
        {
            durationElements = new SilentObservableCollection<DurationElementViewModel>();

            if (ResultPrompt.DurationElements != null)
            {
                foreach (var item in ResultPrompt.DurationElements)
                {
                    durationElements.Add(new DurationElementViewModel(item, this));
                }
            }
            durationElements.CollectionChanged += durationElements_CollectionChanged;
        }

        private void durationElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var deVM = (DurationElementViewModel)item;
                    if (ResultPrompt.DurationElements == null)
                    {
                        ResultPrompt.DurationElements = new List<ResultPromptDuration> { deVM.Duration };
                    }
                    else
                    {
                        ResultPrompt.DurationElements.Add(deVM.Duration);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var deVM = (DurationElementViewModel)item;
                    if (ResultPrompt.DurationElements != null) { ResultPrompt.DurationElements.Remove(deVM.Duration); }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                var resetElements = durationElements.Select(x => x.Duration).ToList();
                ResultPrompt.DurationElements = resetElements.Any() ? resetElements : null;
            }

            OnPropertyChanged("DurationElements");
        }

        public DurationElementViewModel SelectedDurationElement { get; set; }

        #region Associated Models

        private void LoadAssociatedModels()
        {
            if (ResultPrompt.FixedList != null && !ResultPrompt.FixedList.DeletedDate.HasValue)
            {
                FixedList = new FixedListViewModel(treeModel, ResultPrompt.FixedList);
            }

            if (ResultPrompt.ResultPromptWordGroups != null)
            {
                foreach (ResultPromptWordGroup group in ResultPrompt.ResultPromptWordGroups.FindAll(x => x.DeletedDate == null))
                {
                    //find the view model for the given word group
                    var matchedWordGroup = allResultPromptWordGroups.WordGroups.FirstOrDefault(x => x.UUID == group.UUID);
                    if (matchedWordGroup != null)
                    {
                        if (WordGroups == null) { WordGroups = new List<ResultPromptWordGroupViewModel>(); };
                        WordGroups.Add(matchedWordGroup);

                        //set the parent relationship
                        matchedWordGroup.ParentPrompts.Add(this);
                    }
                }
                if (WordGroups != null)
                {
                    //sort alphabetically 
                    WordGroups = WordGroups.OrderBy(x => x.ResultPromptWord).ToList();
                }
            }

            //set up the comments model
            AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, this);
        }

        public List<ResultPromptWordGroupViewModel> WordGroups { get; set; }

        private FixedListViewModel fixedList = null;
        public FixedListViewModel FixedList 
        {
            get => fixedList;
            set
            {
                if (value != null)
                {
                    //only set the result prompt as modified when the master id has changed or when we are setting a model that is different to the underlying data value on ResultPrompt
                    if (ResultPrompt.FixedListUUID == null || value.MasterUUID != ResultPrompt.FixedListUUID)
                    {
                        LastModifiedDate = DateTime.Now;
                    }

                    //only set the result prompt data when the id has changed or when the existing value is null
                    var originalFixedList = ResultPrompt.FixedList;
                    if (ResultPrompt.FixedList == null || value.UUID != ResultPrompt.FixedList.UUID)
                    {
                        ResultPrompt.FixedList = value.FixedList;
                    }

                    //only set the view model when the current value is null or when the original data value has a different id
                    if (fixedList == null || originalFixedList == null || value.UUID != originalFixedList.UUID)
                    {
                        fixedList = value;
                    }
                }

                if (value == null && fixedList != null)
                {
                    ResultPrompt.FixedList = null;
                    fixedList = null;
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #endregion Associated Models

        public ResultPrompt ResultPrompt
        {
            get { return data as ResultPrompt; }
            private set { data = value; }
        }

        public string Cachable
        {
            get => ResultPrompt.Cacheable == null ? CacheableEnum.notCached.GetDescription() : ResultPrompt.Cacheable.GetDescription();
            set
            {
                CacheableEnum? val = value == null ? (CacheableEnum?)null : value.GetValueFromDescription<CacheableEnum>();
                if (val != null && val == CacheableEnum.notCached)
                {
                    val = null;
                }
                if (SetProperty(() => ResultPrompt.Cacheable == val, () => ResultPrompt.Cacheable = val))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string CacheDataPath
        {
            get => ResultPrompt.CacheDataPath;
            set
            {
                if (SetProperty(() => ResultPrompt.CacheDataPath == value, () => ResultPrompt.CacheDataPath = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Label
        {
            get => ResultPrompt.Label;
            set
            {
                if (SetProperty(() => ResultPrompt.Label == value, () => ResultPrompt.Label = value))
                {
                    LastModifiedDate = DateTime.Now;

                    //null out the welsh translation since this item is now required to be retranslated
                    WelshLabel = null;
                }
            }
        }

        public string RegularExpression
        {
            get => ResultPrompt.RegularExpression;
            set
            {
                if (SetProperty(() => ResultPrompt.RegularExpression == value, () => ResultPrompt.RegularExpression = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsAvailableForCourtExtract
        {
            get => ResultPrompt.IsAvailableForCourtExtract;
            set
            {
                if (SetProperty(() => ResultPrompt.IsAvailableForCourtExtract == value, () => ResultPrompt.IsAvailableForCourtExtract = value))
                {
                    LastModifiedDate = DateTime.Now;

                    //cannot be hidden and also available for court extract
                    if (value)
                    {
                        Hidden = false;
                    }
                }
            }
        }

        public string WelshLabel
        {
            get => ResultPrompt.WelshLabel;
            set
            {
                if (SetProperty(() => ResultPrompt.WelshLabel == value, () => ResultPrompt.WelshLabel = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Jurisdiction
        {
            get => ResultPrompt.Jurisdiction;
            set
            {
                if (SetProperty(() => ResultPrompt.Jurisdiction == value, () => ResultPrompt.Jurisdiction = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool Financial
        {
            get => ResultPrompt.Financial;
            set
            {
                if (SetProperty(() => ResultPrompt.Financial == value, () => ResultPrompt.Financial = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool Hidden
        {
            get => ResultPrompt.Hidden;
            set
            {
                if (SetProperty(() => ResultPrompt.Hidden == value, () => ResultPrompt.Hidden = value))
                {
                    LastModifiedDate = DateTime.Now;

                    //cannot be hidden and also available for court extract
                    if(value)
                    {
                        IsAvailableForCourtExtract = false;
                    }
                }
            }
        }

        public bool JudicialValidation
        {
            get => ResultPrompt.JudicialValidation;
            set
            {
                if (SetProperty(() => ResultPrompt.JudicialValidation == value, () => ResultPrompt.JudicialValidation = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool AssociateToReferenceData
        {
            get => ResultPrompt.AssociateToReferenceData;
            set
            {
                if (SetProperty(() => ResultPrompt.AssociateToReferenceData == value, () => ResultPrompt.AssociateToReferenceData = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Handles binding in the xaml to wrap text entries for a int? by wrapping DurationSequence as a string
        /// </summary>
        public string DurationSequenceString
        {
            get
            {
                if (ResultPrompt.DurationSequence == null)
                {
                    return null;
                }
                var val = ResultPrompt.DurationSequence.ToString();
                if (string.IsNullOrEmpty(val))
                {
                    return null;
                }
                return val;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    DurationSequence = null;
                }
                else
                {
                    int val = 0;
                    if (int.TryParse(value, out val))
                    {
                        DurationSequence = val;
                    };
                }
            }
        }

        public int? DurationSequence
        {
            get => ResultPrompt.DurationSequence;
            set
            {
                if (SetProperty(() => ResultPrompt.DurationSequence == value, () => ResultPrompt.DurationSequence = value))
                {
                    LastModifiedDate = DateTime.Now;

                    OnPropertyChanged("DurationSequenceString");
                }
            }
        }

        public bool IsDurationStartDate
        {
            get => ResultPrompt.IsDurationStartDate;
            set
            {
                if (SetProperty(() => ResultPrompt.IsDurationStartDate == value, () => ResultPrompt.IsDurationStartDate = value))
                {
                    if (value)
                    {
                        IsDurationEndDate = false;
                    }
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsDurationEndDate
        {
            get => ResultPrompt.IsDurationEndDate;
            set
            {
                if (SetProperty(() => ResultPrompt.IsDurationEndDate == value, () => ResultPrompt.IsDurationEndDate = value))
                {
                    if (value)
                    {
                        IsDurationStartDate = false;
                    }
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string WelshWording
        {
            get => ResultPrompt.WelshWording;
            set
            {
                if (SetProperty(() => ResultPrompt.WelshWording == value, () => ResultPrompt.WelshWording = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string PromptType
        {
            get => ResultPrompt.PromptType;
            set
            {
                if (SetProperty(() => ResultPrompt.PromptType == value, () => ResultPrompt.PromptType = value))
                {
                    if (value == "NAMEADDRESS")
                    {
                        NameAddressType = "Organisation";
                    }
                    else
                    {
                        NameAddressType = null;
                    }

                    if (value != "INT")
                    {
                        //remove any duration element values
                        ClearDurationElements();

                        //reset the sequence
                        DurationSequence = null;
                    }

                    if (value != "DATE")
                    {
                        //set any duration dates to false
                        IsDurationEndDate = false;
                        IsDurationStartDate = false;
                    }

                    LastModifiedDate = DateTime.Now;

                    OnPropertyChanged("NameAddressVisibility");
                    OnPropertyChanged("IsIntPromptType");
                    OnPropertyChanged("IsDatePromptType");
                }
            }
        }

        public bool IsIntPromptType
        {
            get => !string.IsNullOrEmpty(PromptType) && PromptType == "INT" ? true : false;
        }

        public bool IsDatePromptType
        {
            get => !string.IsNullOrEmpty(PromptType) && PromptType == "DATE" ? true : false;
        }

        public bool NameAddressVisibility
        {
            get => !string.IsNullOrEmpty(PromptType) && PromptType == "NAMEADDRESS" ? true : false;
        }

        public string NameAddressType
        {
            get => ResultPrompt.NameAddressType;
            set
            {
                if (SetProperty(() => ResultPrompt.NameAddressType == value, () => ResultPrompt.NameAddressType = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool NameEmail
        {
            get => ResultPrompt.NameEmail;
            set
            {
                if (SetProperty(() => ResultPrompt.NameEmail == value, () => ResultPrompt.NameEmail = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }        

        public Guid? FixedListMasterUUID
        {
            get => ResultPrompt.FixedListUUID;
        }

        public bool FixedListUUIDVisibility
        {
            get => FixedListMasterUUID != null ? true : false;
        }

        public string FixedListLabel
        {
            get => ResultPrompt.FixedListLabel;
        }

        public bool FixedListLabelVisibility
        {
            get => !string.IsNullOrEmpty(FixedListLabel) ? true : false;
        }

        public string Min
        {
            get => ResultPrompt.Min;
            set
            {
                if (SetProperty(() => ResultPrompt.Min == value, () => ResultPrompt.Min = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Max
        {
            get => ResultPrompt.Max;
            set
            {
                if (SetProperty(() => ResultPrompt.Max == value, () => ResultPrompt.Max = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Qual
        {
            get => ResultPrompt.Qual;
            set
            {
                if (SetProperty(() => ResultPrompt.Qual == value, () => ResultPrompt.Qual = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Wording
        {
            get => ResultPrompt.Wording;
            set
            {
                if (SetProperty(() => ResultPrompt.Wording == value, () => ResultPrompt.Wording = value))
                {
                    LastModifiedDate = DateTime.Now;

                    //null out the welsh translation since this item is now required to be retranslated
                    WelshWording = null;
                }
            }
        }

        public string ResultPromptGroup
        {
            get => ResultPrompt.ResultPromptGroup;
            set
            {
                if (SetProperty(() => ResultPrompt.ResultPromptGroup == value, () => ResultPrompt.ResultPromptGroup = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string PromptReference
        {
            get => ResultPrompt.PromptReference;
            set
            {
                if (SetProperty(() => ResultPrompt.PromptReference == value, () => ResultPrompt.PromptReference = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #region Copy Members

        public ResultPromptViewModel Draft()
        {
            ResultPromptViewModel res = (ResultPromptViewModel)this.MemberwiseClone();

            //make new data
            res.ResultPrompt = res.ResultPrompt.Draft();

            //when required determine if there is a draft version of the fixed list view model
            if (res.ResultPrompt.FixedList != null)
            {
                var draftPromptWithDraftFixedList = treeModel.AllResultPromptViewModel.Prompts.FirstOrDefault
                    (
                        x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft &&
                        x.ResultPromptViewModel.FixedList != null &&
                        x.ResultPromptViewModel.FixedList.PublishedStatus == PublishedStatus.Draft &&
                        x.ResultPromptViewModel.FixedListMasterUUID == res.FixedListMasterUUID
                    );
                if (draftPromptWithDraftFixedList != null)
                {
                    //set the vm
                    res.FixedList = draftPromptWithDraftFixedList.ResultPromptViewModel.FixedList;

                    //set the data
                    res.ResultPrompt.FixedList = draftPromptWithDraftFixedList.ResultPromptViewModel.FixedList.FixedList;
                }
            }

            // make new view collections for the word groups
            var newWordGroups = new List<ResultPromptWordGroupViewModel>();
            if (res.WordGroups != null)
            {
                foreach (var group in res.WordGroups)
                {
                    //determine if there is a draft result prompt word group model already
                    var matchedRpwg = allResultPromptWordGroups.WordGroups.FirstOrDefault(x => x.MasterUUID == group.MasterUUID && x.PublishedStatus == PublishedStatus.Draft);
                    if (matchedRpwg != null)
                    {
                        newWordGroups.Add(matchedRpwg);
                        continue;
                    }

                    //keep the existing published group model                    
                    newWordGroups.Add(group);
                }
            }

            if (newWordGroups != null)
            {
                //sort alphabetically 
                res.WordGroups = newWordGroups.OrderBy(x => x.ResultPromptWord).ToList();

                //set the data in-line with the collection of view models
                res.ResultPrompt.ResultPromptWordGroups = res.WordGroups.Select(x => x.ResultPromptWordGroup).ToList();
            }

            //reset the associated comments model so that the model reflects the new parent view and also sets its context model according to the new publication status
            res.AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, res);

            return res;
        }

        public ResultPromptViewModel Copy()
        {
            ResultPromptViewModel res = (ResultPromptViewModel)this.MemberwiseClone();

            //make new data view model
            res.ResultPrompt = res.ResultPrompt.Copy();

            //keeps the same fixed list view model

            //reset the associated comments model so that the model reflects the new parent view and also sets its context model according to the new publication status
            res.AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, res);

            return res;
        }
        
        #endregion Copy Members
    }
}