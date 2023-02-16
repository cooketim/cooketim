using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using DataLib;
using GalaSoft.MvvmLight.Command;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a ResultDefinition object.
    /// </summary>
    public class ResultDefinitionViewModel : ViewModelBase
    {
        readonly AllResultPromptViewModel allPrompts;
        readonly AllResultDefinitionWordGroupViewModel allDefinitionWordGroups;
        readonly List<string> allUserGroups;
        readonly AllResultRuleViewModel allResultRuleViewModel;
        private ICommand addSecondaryCJSCodeCommand, deleteSecondaryCJSCodeCommand;

        public ResultDefinitionViewModel(ITreeModel treeModel, ResultDefinition resultDefinition, AllResultPromptViewModel allPrompts, 
            AllResultDefinitionWordGroupViewModel allDefinitionWordGroups, 
            List<string> allUserGroups, AllResultRuleViewModel allResultRuleViewModel)
        {
            this.treeModel = treeModel;
            ResultDefinition = resultDefinition;
            this.allResultRuleViewModel = allResultRuleViewModel;            
            this.allPrompts = allPrompts;
            this.allDefinitionWordGroups = allDefinitionWordGroups;
            this.allUserGroups = allUserGroups;            
            LoadAssociatedModels();            
        }

        #region Associated Models

        private void LoadAssociatedModels()
        {
            //make a result rule for this result definition that represents the root 
            allResultRuleViewModel.Rules.Add(new ResultRuleViewModel(treeModel, this, null));

            //add the prompts for this result definition
            if (ResultDefinition.ResultPrompts != null && ResultDefinition.ResultPrompts.Count > 0)
            {
                foreach (ResultPromptRule prompt in ResultDefinition.ResultPrompts)
                { 
                    //find the view model for the given result prompt
                    var matchedPrompt = allPrompts.Prompts.FirstOrDefault(x => x.UUID == prompt.UUID);
                    if (matchedPrompt != null)
                    {
                        if (prompts == null) { prompts = new SilentObservableCollection<ResultPromptRuleViewModel>(); }
                        if(matchedPrompt.ResultDefinitionViewModel == null)
                        {
                            matchedPrompt.ResultDefinitionViewModel = this;
                        }
                        prompts.Add(matchedPrompt);
                    }
                }
                if (prompts != null)
                {
                    //sort into prompt sequence order 
                    prompts = new SilentObservableCollection<ResultPromptRuleViewModel>(prompts.OrderBy(x => x.PromptSequence).ThenBy(x => x.ResultPromptViewModel.Label));
                }
            }
            else
            {
                prompts = new SilentObservableCollection<ResultPromptRuleViewModel>();
            }
            prompts.CollectionChanged += prompts_CollectionChanged;

            //apply the word groups
            if (ResultDefinition.WordGroups != null && ResultDefinition.WordGroups.Count > 0)
            {
                foreach (var group in ResultDefinition.WordGroups)
                {
                    //make a view model
                    var vm = new ResultWordGroupViewModel(group, allDefinitionWordGroups);
                    if (!vm.IsEmptyGroup)
                    {
                        if (WordGroups == null) { WordGroups = new List<ResultWordGroupViewModel>(); };

                        vm.ParentResultDefinitionVM = this;
                        WordGroups.Add(vm);
                    }
                }
                if (WordGroups != null)
                {
                    //sort alphabetically 
                    WordGroups = WordGroups.OrderBy(x => x.WordGroupName).ToList();
                }
            }

            userGroups = ResultDefinition.UserGroups == null ? new SilentObservableCollection<string>() : new SilentObservableCollection<string>(ResultDefinition.UserGroups.OrderBy(x => x).ToList());
            userGroups.CollectionChanged += userGroups_CollectionChanged;

            //create a partial build of the rule view model, setting the associated result definition when available
            if (ResultDefinition.ResultDefinitionRules != null)
            {
                Rules = new List<ResultRuleViewModel>();
                foreach (var rule in ResultDefinition.ResultDefinitionRules)
                {
                    //create a rule view model 
                    var ruleVM = new ResultRuleViewModel(treeModel, rule, this);
                    Rules.Add(ruleVM);
                    allResultRuleViewModel.Rules.Add(ruleVM);
                }
            }

            //apply the secondary cjs codes
            secondaryCJSCodes = new SilentObservableCollection<SecondaryCJSCodeViewModel>();
            if (ResultDefinition.SecondaryCJSCodes != null && ResultDefinition.SecondaryCJSCodes.Count > 0)
            {
                foreach (var code in ResultDefinition.SecondaryCJSCodes)
                {
                    //make a view model
                    var vm = new SecondaryCJSCodeViewModel(this, code);
                    SecondaryCJSCodes.Add(vm);
                }
            }
            secondaryCJSCodes.CollectionChanged += secondaryCJSCodes_CollectionChanged;

            //set up the publication tag model
            PublicationTagsModel = new PublicationTagsModel(treeModel, this);

            //set up the comments model
            AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, this);
        }

        public PublicationTagsModel PublicationTagsModel { get; set; }

        public AssociatedCommentsModel AssociatedCommentsModel { get; set; }

        public SilentObservableCollection<string> SelectedUserGroupVariants { get; set; }

        private void userGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var user = (string)item;
                    if (ResultDefinition.UserGroups == null)
                    {
                        ResultDefinition.UserGroups = new List<string>() { user };
                    }
                    else
                    {
                        ResultDefinition.UserGroups.Add(user);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var user = (string)item;
                    if (ResultDefinition.UserGroups != null) { ResultDefinition.UserGroups.Remove(user); }
                }
            }

            OnPropertyChanged("AvailableUserGroups");
        }

        internal void SetWordGroupsChanged()
        {
            OnPropertyChanged("IsShortCodeReadOnly");
        }

        public bool IsShortCodeReadOnly
        {
            get => HasSingleWordGroup();
        }

        public bool HasSingleWordGroup()
        {
            if (WordGroups != null && WordGroups.Count > 0 && DeletedDate == null)
            {
                var singleWordGroups = WordGroups.Where(x => x.ResultDefinitionWordGroups != null && x.ResultDefinitionWordGroups.FindAll(y => y.DeletedDate == null).Count == 1).ToList();
                if (singleWordGroups.Count == 0) { return false; }

                //determine if the short code matches any of the single word groups
                var matchedShortCode = singleWordGroups.FirstOrDefault(x => x.ResultDefinitionWordGroups[0].ResultDefinitionWord == ShortCode);

                if (matchedShortCode == null)
                {
                    //set the short code to the definition word of the first single word group
                    ShortCode = singleWordGroups[0].ResultDefinitionWordGroups[0].ResultDefinitionWord;
                }
                return true;
            }
            return false;
        }

        private SilentObservableCollection<ResultPromptRuleViewModel> prompts;

        public SilentObservableCollection<ResultPromptRuleViewModel> Prompts
        {
            get => prompts;
            set
            {
                prompts = value;
                prompts.CollectionChanged += prompts_CollectionChanged;
            }
        }
        public bool UserGroupsVisibility
        {
            get { return prompts != null && prompts.Where(x=>!x.IsDeleted).Any() ? false : true; }
        }

        private void prompts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var prompt = (ResultPromptRuleViewModel)item;
                    if (ResultDefinition.ResultPrompts == null)
                    {
                        ResultDefinition.ResultPrompts = new List<ResultPromptRule>() { prompt.ResultPromptRule };
                    }
                    else
                    {
                        ResultDefinition.ResultPrompts.Add(prompt.ResultPromptRule);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var prompt = (ResultPromptRuleViewModel)item;
                    if (ResultDefinition.ResultPrompts != null) { ResultDefinition.ResultPrompts.Remove(prompt.ResultPromptRule); }
                }
            }

            OnPropertyChanged("UserGroupsVisibility");
        }

        public List<ResultWordGroupViewModel> WordGroups { get; set; }        

        public List<ResultRuleViewModel> Rules { get; set; }

        #endregion Associated Models

        #region ResultDefinition Properties

        public ResultDefinition ResultDefinition
        {
            get { return data as ResultDefinition; }
            private set { data = value; }
        }

        public SilentObservableCollection<string> AvailableUserGroups
        {
            get
            {
                var unselected = allUserGroups.Except(UserGroups).ToList();
                return new SilentObservableCollection<string>(unselected.OrderBy(x => x).ToList());
            }
        }

        private SilentObservableCollection<string> userGroups;
        public SilentObservableCollection<string> UserGroups 
        {
            get => userGroups;
            set
            {
                userGroups = value;
                userGroups.CollectionChanged += userGroups_CollectionChanged;
            }
        }

        public string Cachable
        {
            get => ResultDefinition.Cacheable == null ? CacheableEnum.notCached.GetDescription() : ResultDefinition.Cacheable.GetDescription();
            set
            {
                CacheableEnum? val = value == null ? (CacheableEnum?)null : value.GetValueFromDescription<CacheableEnum>();
                if (val != null && val == CacheableEnum.notCached)
                {
                    val = null;
                }
                if (SetProperty(() => ResultDefinition.Cacheable == val, () => ResultDefinition.Cacheable = val))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }
        public string CacheDataPath
        {
            get => ResultDefinition.CacheDataPath;
            set
            {
                if (SetProperty(() => ResultDefinition.CacheDataPath == value, () => ResultDefinition.CacheDataPath = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }
        public string TriggeredApplicationCode
        {
            get => ResultDefinition.TriggeredApplicationCode;
            set
            {
                if (SetProperty(() => ResultDefinition.TriggeredApplicationCode == value, () => ResultDefinition.TriggeredApplicationCode = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Label
        {    
            get => ResultDefinition.Label;
            set
                {
                    if (SetProperty(() => ResultDefinition.Label == value, () => ResultDefinition.Label = value))
                    {
                        LastModifiedDate = DateTime.Now;

                        //null out the welsh translation since the label is now required to be retranslated
                        WelshLabel = null;
                    }
                }
        }

        public string Level
        {
            get => ResultDefinition.Level;
            set
                {
                    if (SetProperty(() => ResultDefinition.Level == value, () => ResultDefinition.Level = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string ShortCode
        {
            get => ResultDefinition.ShortCode;
            set
                {
                    if (SetProperty(() => ResultDefinition.ShortCode == value, () => ResultDefinition.ShortCode = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool Adjournment
        {
            get => ResultDefinition.Adjournment;
            set
                {
                    if (SetProperty(() => ResultDefinition.Adjournment == value, () => ResultDefinition.Adjournment = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string Category
        {
            get => ResultDefinition.Category;
            set
                {
                    if (SetProperty(() => ResultDefinition.Category == value, () => ResultDefinition.Category = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string Jurisdiction
        {
            get => ResultDefinition.Jurisdiction;
            set
            {
                if (value == null) { return; }
                if (SetProperty(() => ResultDefinition.Jurisdiction == value, () => ResultDefinition.Jurisdiction = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool Urgent
        {
            get => ResultDefinition.Urgent;
            set
                {
                    if (SetProperty(() => ResultDefinition.Urgent == value, () => ResultDefinition.Urgent = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool Unscheduled
        {
            get => ResultDefinition.Unscheduled;
            set
            {
                if (SetProperty(() => ResultDefinition.Unscheduled == value, () => ResultDefinition.Unscheduled = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool Financial
        {
            get => ResultDefinition.Financial;
            set
                {
                    if (SetProperty(() => ResultDefinition.Financial == value, () => ResultDefinition.Financial = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public override PublishedStatus PublishedStatus
        {
            get => base.PublishedStatus;
            set
            {
                if (data.PublishedStatus != value)
                {
                    base.PublishedStatus = value;
                    OnPropertyChanged("PublishedForNowsEnabled");
                    OnPropertyChanged("PublishedAsAPromptEnabled");
                    OnPropertyChanged("RollupPromptsEnabled");
                }
            }
        }

        public bool PublishedForNowsEnabled
        {
            get
            {
                return !AlwaysPublished && IsEnabled;
            }
        }

        public bool PublishedAsAPromptEnabled
        {
            get
            {
                return !IsBooleanResult && IsEnabled;
            }
        }

        public bool PublishedAsAPrompt
        {
            get => ResultDefinition.IsPublishedAsAPrompt;
            set
            {
                if (SetProperty(() => ResultDefinition.IsPublishedAsAPrompt == value, () => ResultDefinition.IsPublishedAsAPrompt = value))
                {
                    LastModifiedDate = DateTime.Now;
                    if (value)
                    {
                        ExcludedFromResults = false;
                        AlwaysPublished = false;
                        RollupPrompts = false;
                    }
                    InformNowRequirementView();
                }
            }
        }

        public bool ExcludedFromResults
        {
            get => ResultDefinition.IsExcludedFromResults;
            set
            {
                if (SetProperty(() => ResultDefinition.IsExcludedFromResults == value, () => ResultDefinition.IsExcludedFromResults = value))
                {
                    LastModifiedDate = DateTime.Now;
                    if (value)
                    {
                        PublishedAsAPrompt = false;
                        AlwaysPublished = false;
                        RollupPrompts = false;

                        //remove any result text template settings
                        ResultTextTemplate = null;
                    }
                    InformNowRequirementView();

                    OnPropertyChanged("ResultTextTemplateVisibility");                    
                }
            }
        }

        public bool AlwaysPublished
        {
            get => ResultDefinition.IsAlwaysPublished;
            set
            {
                if (SetProperty(() => ResultDefinition.IsAlwaysPublished == value, () => ResultDefinition.IsAlwaysPublished = value))
                {

                    LastModifiedDate = DateTime.Now;

                    //if the result is being published then there is no need to publish the result for NOWs
                    if (value)
                    {
                        PublishedAsAPrompt = false;
                        ExcludedFromResults = false;
                        RollupPrompts = false;

                        PublishedForNows = false;
                    }
                    InformNowRequirementView();

                    OnPropertyChanged("PublishedForNowsEnabled");
                }
            }
        }

        public bool RollupPrompts
        {
            get => ResultDefinition.IsRollupPrompts;
            set
            {
                if (SetProperty(() => ResultDefinition.IsRollupPrompts == value, () => ResultDefinition.IsRollupPrompts = value))
                {
                    LastModifiedDate = DateTime.Now;
                    if (value)
                    {
                        PublishedAsAPrompt = false;
                        ExcludedFromResults = false;
                        AlwaysPublished = false;
                    }
                    InformNowRequirementView();
                }
            }
        }

        public bool RollupPromptsEnabled
        {
            get
            {
                return !IsBooleanResult && IsEnabled;
            }
        }

        public bool PublishedForNows
        {
            get => ResultDefinition.IsPublishedForNows;
            set
            {
                if (SetProperty(() => ResultDefinition.IsPublishedForNows == value, () => ResultDefinition.IsPublishedForNows = value))
                {
                    LastModifiedDate = DateTime.Now;
                    InformNowRequirementView();
                }
            }
        }

        private void InformNowRequirementView()
        {
            treeModel.AllNowRequirementsViewModel.NowRequirements.Where(x => x.ResultDefinition.UUID == this.UUID).ToList().ForEach(x => x.ResultPublishingRulesChanged());
        }

        public bool IsLifeDuration
        {
            get => ResultDefinition.IsLifeDuration;
            set
                {
                    if (SetProperty(() => ResultDefinition.IsLifeDuration == value, () => ResultDefinition.IsLifeDuration = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool JudicialValidation
        {
            get => ResultDefinition.JudicialValidation;
            set
            {
                if (SetProperty(() => ResultDefinition.JudicialValidation == value, () => ResultDefinition.JudicialValidation = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// The objective of a boolean result is to model a set of children that are conditional based on a positice response to a boolean entry
        /// </summary>
        public bool IsBooleanResult
        {
            get => ResultDefinition.IsBooleanResult;
            set
            {
                if (SetProperty(() => ResultDefinition.IsBooleanResult == value, () => ResultDefinition.IsBooleanResult = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        ExcludedFromResults = true;

                        //make it a mandatory child of any parents
                        var parents = allResultRuleViewModel.Rules.Where(x => x.DeletedDate == null && x.ChildResultDefinitionViewModel.UUID == UUID && x.ParentUUID != null);
                        foreach (var parent in parents)
                        {
                            parent.RuleType = ResultDefinitionRuleType.mandatory;
                        }
                    }

                    OnPropertyChanged("RollupPromptsEnabled");
                    OnPropertyChanged("PublishedAsAPromptEnabled");
                }
            }
        }

        public bool TerminatesOffenceProceedings
        {
            get => ResultDefinition.TerminatesOffenceProceedings;
            set
                {
                    if (SetProperty(() => ResultDefinition.TerminatesOffenceProceedings == value, () => ResultDefinition.TerminatesOffenceProceedings = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool Convicted
        {
            get => ResultDefinition.Convicted;
            set
                {
                    if (SetProperty(() => ResultDefinition.Convicted == value, () => ResultDefinition.Convicted = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool D20
        {
            get => ResultDefinition.D20;
            set
                {
                    if (SetProperty(() => ResultDefinition.D20 == value, () => ResultDefinition.D20 = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool CanBeSubjectOfBreach
        {
            get => ResultDefinition.CanBeSubjectOfBreach;
            set
            {
                if (SetProperty(() => ResultDefinition.CanBeSubjectOfBreach == value, () => ResultDefinition.CanBeSubjectOfBreach = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool CanBeSubjectOfVariation
        {
            get => ResultDefinition.CanBeSubjectOfVariation;
            set
            {
                if (SetProperty(() => ResultDefinition.CanBeSubjectOfVariation == value, () => ResultDefinition.CanBeSubjectOfVariation = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string ResultDefinitionGroup
        {
            get => ResultDefinition.ResultDefinitionGroup;
            set
                {
                    if (SetProperty(() => ResultDefinition.ResultDefinitionGroup == value, () => ResultDefinition.ResultDefinitionGroup = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string DVLACode
        {
            get => ResultDefinition.DVLACode;
            set
                {
                    if (SetProperty(() => ResultDefinition.DVLACode == value, () => ResultDefinition.DVLACode = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string PointsDisqualificationCode
        {
            get => ResultDefinition.PointsDisqualificationCode;
            set
            {
                if (SetProperty(() => ResultDefinition.PointsDisqualificationCode == value, () => ResultDefinition.PointsDisqualificationCode = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string CJSCode
        {
            get => ResultDefinition.CJSCode;
            set
            {
                if (SetProperty(() => ResultDefinition.CJSCode == value, () => ResultDefinition.CJSCode = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (string.IsNullOrEmpty(value))
                    {
                        SecondaryCJSCodes = null;
                    }

                    OnPropertyChanged("SecondaryCJSCodesVisibility");
                }
            }
        }

        public bool SecondaryCJSCodesVisibility
        {
            get { return !string.IsNullOrEmpty(CJSCode) ? true : false; }
        }

        public ICommand AddSecondaryCJSCodeCommand
        {
            get
            {
                if (addSecondaryCJSCodeCommand == null)
                {
                    addSecondaryCJSCodeCommand = new RelayCommand<SecondaryCJSCodeViewModel>(x => AddSecondaryCJSCode(x));
                }
                return addSecondaryCJSCodeCommand;
            }
        }
        public ICommand DeleteSecondaryCJSCodeCommand
        {
            get
            {
                if (deleteSecondaryCJSCodeCommand == null)
                {
                    deleteSecondaryCJSCodeCommand = new RelayCommand<SecondaryCJSCodeViewModel>(x => DeleteSecondaryCJSCode(x));
                }
                return deleteSecondaryCJSCodeCommand;
            }
        }

        private void AddSecondaryCJSCode(SecondaryCJSCodeViewModel deVM)
        {
            if (deVM == null)
            {
                deVM = CreateNewSecondaryCJSCode();
            }
            SecondaryCJSCodes.Add(deVM);

            LastModifiedDate = DateTime.Now;
        }

        private SecondaryCJSCodeViewModel CreateNewSecondaryCJSCode()
        {
            //Make a new item 
            var data = new SecondaryCJSCode();
            var vm = new SecondaryCJSCodeViewModel(this, data);

            //ensure that code is unique
            var duplicateCode = false;
            var code = int.Parse(data.CJSCode);
            int i = 1;
            do
            {
                duplicateCode = SecondaryCJSCodes.FirstOrDefault(x => x.CJSCode == data.CJSCode) != null;
                if (duplicateCode)
                {
                    data.CJSCode = (code + i).ToString();
                    data.Text = string.Format("new text (code {0})", data.CJSCode);
                    i++;
                }

            } while (duplicateCode);

            return vm;
        }

        private void DeleteSecondaryCJSCode(SecondaryCJSCodeViewModel deVM)
        {
            if (deVM == null)
            {
                deVM = SelectedSecondaryCJSCode;
            }
            if (deVM != null)
            {
                SecondaryCJSCodes.Remove(deVM);
            }
            SelectedSecondaryCJSCode = null;
            OnPropertyChanged("SelectedSecondaryCJSCode");

            LastModifiedDate = DateTime.Now;
        }

        private SilentObservableCollection<SecondaryCJSCodeViewModel> secondaryCJSCodes;
        public SilentObservableCollection<SecondaryCJSCodeViewModel> SecondaryCJSCodes
        {
            get => secondaryCJSCodes;
            set
            {
                if (value == null)
                {
                    secondaryCJSCodes.Clear();
                }
                else
                {
                    secondaryCJSCodes = value;
                    secondaryCJSCodes.CollectionChanged += secondaryCJSCodes_CollectionChanged;
                }
            }
        }

        private void secondaryCJSCodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var secondaryCJSCode = (SecondaryCJSCodeViewModel)item;
                    if (ResultDefinition.SecondaryCJSCodes == null)
                    {
                        ResultDefinition.SecondaryCJSCodes = new List<SecondaryCJSCode>() { secondaryCJSCode.SecondaryCJSCode };
                    }
                    else
                    {
                        var index = 0;
                        if (e.NewStartingIndex != -1)
                        {
                            index = e.NewStartingIndex;
                        }
                        ResultDefinition.SecondaryCJSCodes.Insert(index, secondaryCJSCode.SecondaryCJSCode);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var secondaryCJSCode = (SecondaryCJSCodeViewModel)item;
                    if (ResultDefinition.SecondaryCJSCodes != null) { ResultDefinition.SecondaryCJSCodes.Remove(secondaryCJSCode.SecondaryCJSCode); }
                    if (ResultDefinition.SecondaryCJSCodes.Count() == 0) { ResultDefinition.SecondaryCJSCodes = null; }
                }
            }

            OnPropertyChanged("SecondaryCJSCodes");
        }

        public SecondaryCJSCodeViewModel SelectedSecondaryCJSCode { get; set; }

        public string Qualifier
        {
            get => ResultDefinition.Qualifier;
            set
                {
                    if (SetProperty(() => ResultDefinition.Qualifier == value, () => ResultDefinition.Qualifier = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string PostHearingCustodyStatus
        {
            get => ResultDefinition.PostHearingCustodyStatus;
            set
                {
                    if (SetProperty(() => ResultDefinition.PostHearingCustodyStatus == value, () => ResultDefinition.PostHearingCustodyStatus = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public int? Rank
        {
            get => ResultDefinition.Rank;
            set
                {
                    if (SetProperty(() => ResultDefinition.Rank == value, () => ResultDefinition.Rank = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }
        public string StipulatedDrivingTestType
        {
            get => ResultDefinition.StipulatedDrivingTestType == null ? DrivingTestTypeEnum.noTest.GetDescription() : ResultDefinition.StipulatedDrivingTestType.GetDescription();
            set
            {
                DrivingTestTypeEnum? val = value == null ? (DrivingTestTypeEnum?)null : value.GetValueFromDescription<DrivingTestTypeEnum>();
                if (val != null && val == DrivingTestTypeEnum.noTest)
                {
                    val = null;
                }
                if (SetProperty(() => ResultDefinition.StipulatedDrivingTestType == val, () => ResultDefinition.StipulatedDrivingTestType = val))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }


        public string LibraCode
        {
            get => ResultDefinition.LibraCode;
            set
                {
                    if (SetProperty(() => ResultDefinition.LibraCode == value, () => ResultDefinition.LibraCode = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string LCode
        {
            get => ResultDefinition.LCode;
            set
                {
                    if (SetProperty(() => ResultDefinition.LCode == value, () => ResultDefinition.LCode = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public DateTime? StartDate
        {
            get => ResultDefinition.StartDate;
            set
            {
                if (SetProperty(() => ResultDefinition.StartDate == value, () => ResultDefinition.StartDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public DateTime? EndDate
        {
            get => ResultDefinition.EndDate;
            set
            {
                if (SetProperty(() => ResultDefinition.EndDate == value, () => ResultDefinition.EndDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool ResultTextTemplateVisibility
        {
            //get { return ExcludedFromResults ? false : true; }
            //always on
            get { return true; }
        }

        public string ResultTextTemplate
        {
            get => ResultDefinition.ResultTextTemplate;
            set
            {
                if (SetProperty(() => ResultDefinition.ResultTextTemplate == value, () => ResultDefinition.ResultTextTemplate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsBusinessResult
        {
            get => ResultDefinition.IsBusinessResult == null ? false : ResultDefinition.IsBusinessResult.Value;
            set
            {
                bool? setVal = value ? (bool?)true : null;
                if (SetProperty(() => ResultDefinition.IsBusinessResult == setVal, () => ResultDefinition.IsBusinessResult = setVal))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string DependantResultDefinitionGroup
        {
            get => ResultDefinition.DependantResultDefinitionGroup;
            set
            {
                if (SetProperty(() => ResultDefinition.DependantResultDefinitionGroup == value, () => ResultDefinition.DependantResultDefinitionGroup = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string ResultWording
        {
            get => ResultDefinition.ResultWording;
            set
                {
                    if (SetProperty(() => ResultDefinition.ResultWording == value, () => ResultDefinition.ResultWording = value))
                    {
                        LastModifiedDate = DateTime.Now;

                        //null out the welsh translation since this item is now required to be retranslated
                        WelshResultWording = null;
                    }
                }
        }

        public string WelshResultWording
        {
            get => ResultDefinition.WelshResultWording;
            set
                {
                    if (SetProperty(() => ResultDefinition.WelshResultWording == value, () => ResultDefinition.WelshResultWording = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool IsAvailableForCourtExtract
        {
            get => ResultDefinition.IsAvailableForCourtExtract;
            set
                {
                    if (SetProperty(() => ResultDefinition.IsAvailableForCourtExtract == value, () => ResultDefinition.IsAvailableForCourtExtract = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string WelshLabel
        {
            get => ResultDefinition.WelshLabel;
            set
                {
                    if (SetProperty(() => ResultDefinition.WelshLabel == value, () => ResultDefinition.WelshLabel = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        #endregion // ResultDefinition Properties

        #region Copy Members
        public ResultDefinitionViewModel Draft(List<string> publicationTags)
        {
            ResultDefinitionViewModel res = (ResultDefinitionViewModel)this.MemberwiseClone();

            //make new data
            var newData = res.ResultDefinition.Draft();
            newData.PublicationTags = publicationTags;
            res.ResultDefinition = newData;

            //make new view models
            MakeViewModels(res, newData, true);

            return res;
        }

        public ResultDefinitionViewModel Copy()
        {
            ResultDefinitionViewModel res = (ResultDefinitionViewModel)this.MemberwiseClone();
            //make new data
            var newData = res.ResultDefinition.Copy();
            res.ResultDefinition = newData;
            MakeViewModels(res, newData, false);

            return res;
        }

        public ResultDefinitionViewModel CopyAsDraft()
        {
            ResultDefinitionViewModel res = (ResultDefinitionViewModel)this.MemberwiseClone();
            //make new data
            var newData = res.ResultDefinition.CopyAsDraft();
            res.ResultDefinition = newData;
            MakeViewModels(res, newData, true);

            return res;
        }

        private void MakeViewModels(ResultDefinitionViewModel res, ResultDefinition newData, bool asDraft)
        {
            //make new view collections for the child results
            var newRules = new List<ResultRuleViewModel>();
            if (newData.ResultDefinitionRules != null)
            {                
                foreach (var rule in newData.ResultDefinitionRules)
                {
                    var matchedVM = res.Rules.FirstOrDefault(x => x.ChildResultDefinitionViewModel.UUID == rule.ChildResultDefinitionUUID);
                    var rdVM = matchedVM.ChildResultDefinitionViewModel;

                    if (asDraft)
                    {
                        //determine if there is already a draft result view model
                        var draftResultVMs = allResultRuleViewModel.Rules.Where(x => x.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft &&
                                                       !x.ChildResultDefinitionViewModel.IsDeleted &&
                                                        x.ChildResultDefinitionViewModel.MasterUUID == matchedVM.ChildResultDefinitionViewModel.MasterUUID).OrderByDescending(x => x.CreatedDate).ToList();
                        if (draftResultVMs.Count > 0)
                        {
                            rdVM = draftResultVMs.First().ChildResultDefinitionViewModel;
                            //update the draft rule data
                            rule.ResultDefinition = draftResultVMs.First().ChildResultDefinitionViewModel.ResultDefinition;
                        }
                        var rdrVM = new ResultRuleViewModel(treeModel, rdVM, res, rule);
                        newRules.Add(rdrVM);
                        allResultRuleViewModel.Rules.Add(rdrVM);
                    }
                    else
                    {
                        var rdrVM = new ResultRuleViewModel(treeModel, rdVM, res, rule);
                        newRules.Add(rdrVM);
                        allResultRuleViewModel.Rules.Add(rdrVM);
                    }
                }                
            }
            res.Rules = newRules;

            //make new view collections for the prompts
            var newPrompts = new List<ResultPromptRuleViewModel>();
            if (newData.ResultPrompts != null)
            {                
                foreach (var rule in newData.ResultPrompts)
                {
                    var matchedVM = res.Prompts.FirstOrDefault(x => x.ResultPromptViewModel.UUID == rule.ResultPromptUUID);
                    var rpVM = matchedVM.ResultPromptViewModel;

                    if (asDraft)
                    {
                        //first determine if there is already a matching draft view model
                        var existingDrafts = allPrompts.Prompts.Where(x => x.PublishedStatus == PublishedStatus.Draft && 
                                                    x.MasterUUID == matchedVM.MasterUUID && !x.IsDeleted)
                                                    .OrderByDescending(x => x.CreatedDate).ToList();
                        if (existingDrafts.Count > 0)
                        {
                            newPrompts.Add(existingDrafts.First());
                        }
                        else
                        {
                            //determine if there is already a draft prompt view model                            
                            var draftResultPromptVMs = allPrompts.Prompts.Where(x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft &&
                                                           !x.ResultPromptViewModel.IsDeleted && !x.IsDeleted &&
                                                            x.ResultPromptViewModel.MasterUUID == matchedVM.ResultPromptViewModel.MasterUUID).OrderByDescending(x => x.CreatedDate).ToList();
                            if (draftResultPromptVMs.Count > 0)
                            {
                                rpVM = draftResultPromptVMs.First().ResultPromptViewModel;
                            }
                            var rprVM = new ResultPromptRuleViewModel(treeModel, rpVM, rule, treeModel.UserGroups, res);
                            newPrompts.Add(rprVM);
                            allPrompts.Prompts.Add(rprVM);
                        }
                    }
                    else
                    {
                        var rprVM = new ResultPromptRuleViewModel(treeModel, rpVM, rule, treeModel.UserGroups, res);
                        newPrompts.Add(rprVM);
                        allPrompts.Prompts.Add(rprVM);
                    }
                }                
            }
            res.Prompts = new SilentObservableCollection<ResultPromptRuleViewModel>(newPrompts.OrderBy(x=>x.PromptSequence));

            //make new view collections for the user groups
            if (newData.UserGroups != null)
            {
                res.UserGroups = new SilentObservableCollection<string>(newData.UserGroups);
            }
            else
            {
                res.UserGroups = new SilentObservableCollection<string>();
            }

            //make new view collections for the word groups
            var newWordGroups = new List<ResultWordGroupViewModel>();
            if (newData.WordGroups != null)
            {
                foreach (var group in newData.WordGroups)
                {
                    //make a view model
                    var vm = new ResultWordGroupViewModel(group, allDefinitionWordGroups);
                    if (!vm.IsEmptyGroup)
                    {
                        if (asDraft)
                        {
                            //determine if there are any draft result definition word groups in preference to those set as part of the constructor above
                            var resultDefinitionWordGroups = new List<ResultDefinitionWordGroupViewModel>();
                            foreach (var rdwg in vm.ResultDefinitionWordGroups)
                            {
                                var matchedRdwg = allDefinitionWordGroups.WordGroups.FirstOrDefault(x => x.MasterUUID == rdwg.MasterUUID && x.PublishedStatus == PublishedStatus.Draft);
                                if (matchedRdwg != null)
                                {
                                    resultDefinitionWordGroups.Add(matchedRdwg);
                                }
                                else
                                {
                                    resultDefinitionWordGroups.Add(rdwg);
                                }
                            }
                            //set the vm to the alphabetically sorted collection
                            vm.ResultDefinitionWordGroups = resultDefinitionWordGroups.OrderBy(x => x.ResultDefinitionWord).ToList();

                            //set the data in-line with the collection of view models
                            vm.ResultWordGroup.ResultDefinitionWordGroups = vm.ResultDefinitionWordGroups.Select(x=>x.ResultDefinitionWordGroup).OrderBy(x => x.ResultDefinitionWord).ToList();

                            //set the parent
                            vm.ParentResultDefinitionVM = res;
                            newWordGroups.Add(vm);
                        }
                        else
                        {
                            vm.ParentResultDefinitionVM = res;
                            newWordGroups.Add(vm);
                        }
                    }
                    if (newWordGroups != null)
                    {
                        //sort alphabetically 
                        res.WordGroups = newWordGroups.OrderBy(x => x.WordGroupName).ToList();
                    }
                }
            }
            res.WordGroups = newWordGroups;

            //make new view collections for the secondary CJS Codes
            var newSecondaryCJSCodes = new SilentObservableCollection<SecondaryCJSCodeViewModel>();
            if (newData.SecondaryCJSCodes != null)
            {
                foreach (var code in newData.SecondaryCJSCodes)
                {
                    //make a view model
                    var vm = new SecondaryCJSCodeViewModel(res, code);
                    newSecondaryCJSCodes.Add(vm);
                }
            }
            res.SecondaryCJSCodes = newSecondaryCJSCodes;

            //reset the publication tags model so that the model reflects the new parent view and also sets its event listeners according to the new publication status
            res.PublicationTagsModel = new PublicationTagsModel(treeModel, res);

            //reset the associated comments model so that the model reflects the new parent view and also sets its context model according to the new publication status
            res.AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, res);
        }                

        #endregion Copy Members
    }
}