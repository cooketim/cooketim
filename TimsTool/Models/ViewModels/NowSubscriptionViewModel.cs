using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DataLib;
using Models.Commands;
using Serilog;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a Now object.
    /// </summary>
    public class NowSubscriptionViewModel : ViewModelBase
    {
        public NowSubscriptionViewModel(ITreeModel treeModel, NowSubscription nowSubscription)
        {
            this.treeModel = treeModel;
            NowSubscription = nowSubscription;
            LoadAssociatedModels();
        }

        #region Associated Models

        private void LoadAssociatedModels()
        {
            if (NowSubscription.IsNow || NowSubscription.IsEDT)
            {
                //add the child subscriptions
                if (NowSubscription.ChildNowSubscriptions != null && NowSubscription.ChildNowSubscriptions.Count > 0)
                {
                    foreach (NowSubscription child in NowSubscription.ChildNowSubscriptions)
                    {
                        if (ChildNowSubscriptions == null) { ChildNowSubscriptions = new List<NowSubscriptionViewModel>(); }

                        ChildNowSubscriptions.Add(new NowSubscriptionViewModel(treeModel, child));
                    }
                }

                //set up the user group variant selection
                //load the user groups for this view model by cloning the master list of user groups
                AllUserGroups = new List<ComboBoxItemString>();
                treeModel.UserGroupsAsCombo.ForEach(x => AllUserGroups.Add(x.Copy()));

                //load the selected user group variants
                if (NowSubscription.UserGroupVariants != null)
                {
                    SelectedUserGroupVariants = new SilentObservableCollection<string>(NowSubscription.UserGroupVariants);
                    foreach(var userGroup in NowSubscription.UserGroupVariants)
                    {
                        //select the user group
                        var item = AllUserGroups.FirstOrDefault(x => x.CodeString == userGroup);
                        if (item != null)
                        {
                            item.CheckStatus = true;
                        }
                    }
                }
                else
                {
                    SelectedUserGroupVariants = new SilentObservableCollection<string>();
                }

                SelectedUserGroupVariants.CollectionChanged += selectedUserGroupVariants_CollectionChanged;
            }

            //Only court register
            if (NowSubscription.IsCourtRegister)
            {
                //load the court houses for this view model by cloning the master list of court houses
                AllCourtHouses = new List<ComboBoxItemString>();
                treeModel.CourtHouses.ForEach(x => AllCourtHouses.Add(x.Copy()));

                //load the selected court houses
                SelectedCourtHouses = new SilentObservableCollection<SelectedCourtHouse>();
                if (NowSubscription.SelectedCourtHouses != null)
                {
                    foreach (var item in NowSubscription.SelectedCourtHouses)
                    {
                        //get the court house
                        var ch = AllCourtHouses.FirstOrDefault(x => x.CodeString.Trim().ToLowerInvariant() == item.Trim().ToLowerInvariant());
                        if (ch != null)
                        {
                            SelectedCourtHouses.Add(new SelectedCourtHouse() { Code = ch.CodeString, CourtHouse = ch.ValueString });

                            //set this court house as being selected
                            ch.CheckStatus = true;
                        }
                    }
                }

                SelectedCourtHouses.CollectionChanged += selectedCourtHouses_CollectionChanged;
            }

            //vocabulary
            ExcludedPrompts = new SilentObservableCollection<NowSubscriptionPromptViewModel>();
            if (NowSubscription.SubscriptionVocabulary.ExcludedPromptRules != null && NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Count > 0)
            {
                NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.RemoveAll(x => x==null);
                //set the view model for the excluded prompts
                foreach (var item in NowSubscription.SubscriptionVocabulary.ExcludedPromptRules)
                {
                    ExcludedPrompts.Add(new NowSubscriptionPromptViewModel(item, this));
                }
            }
            ExcludedPrompts.CollectionChanged += excludedPrompts_CollectionChanged;

            IncludedPrompts = new SilentObservableCollection<NowSubscriptionPromptViewModel>();
            if (NowSubscription.SubscriptionVocabulary.IncludedPromptRules != null && NowSubscription.SubscriptionVocabulary.IncludedPromptRules.Count > 0)
            {
                NowSubscription.SubscriptionVocabulary.IncludedPromptRules.RemoveAll(x => x == null);
                //set the view model for the included prompts
                foreach (var item in NowSubscription.SubscriptionVocabulary.IncludedPromptRules)
                {
                    IncludedPrompts.Add(new NowSubscriptionPromptViewModel(item, this));
                }
            }
            IncludedPrompts.CollectionChanged += includedPrompts_CollectionChanged;

            ExcludedResults = new SilentObservableCollection<NowSubscriptionResultViewModel>();
            if (NowSubscription.SubscriptionVocabulary.ExcludedResults != null && NowSubscription.SubscriptionVocabulary.ExcludedResults.Count > 0)
            {
                NowSubscription.SubscriptionVocabulary.ExcludedResults.RemoveAll(x => x == null);
                //set the view model for the excluded Results
                foreach (var item in NowSubscription.SubscriptionVocabulary.ExcludedResults)
                {
                    ExcludedResults.Add(new NowSubscriptionResultViewModel(item, this));
                }
            }
            ExcludedResults.CollectionChanged += excludedResults_CollectionChanged;

            IncludedResults = new SilentObservableCollection<NowSubscriptionResultViewModel>();
            if (NowSubscription.SubscriptionVocabulary.IncludedResults != null && NowSubscription.SubscriptionVocabulary.IncludedResults.Count > 0)
            {
                NowSubscription.SubscriptionVocabulary.IncludedResults.RemoveAll(x => x == null);
                //set the view model for the included Results
                foreach (var item in NowSubscription.SubscriptionVocabulary.IncludedResults)
                {
                    IncludedResults.Add(new NowSubscriptionResultViewModel(item, this));
                }
            }
            IncludedResults.CollectionChanged += includedResults_CollectionChanged;

            //set up the publication tag model
            PublicationTagsModel = new PublicationTagsModel(treeModel, this);

            //set up the comments model
            AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, this);
        }

        public PublicationTagsModel PublicationTagsModel { get; set; }

        public AssociatedCommentsModel AssociatedCommentsModel { get; set; }

        public bool HasExcludedPromptsContextActions
        {
            get
            {
                if (HasExcludedPrompts) { return true; }

                return ValidateSourceIsPrompt();
            }
        }

        public bool HasExcludedPrompts
        {
            get => ExcludedPrompts.Count > 0;
        }

        public SilentObservableCollection<NowSubscriptionPromptViewModel> ExcludedPrompts { get; private set; }

        private void excludedPrompts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var selectedPrompt = (NowSubscriptionPromptViewModel)item;
                    if (NowSubscription.SubscriptionVocabulary.ExcludedPromptRules == null)
                    {
                        NowSubscription.SubscriptionVocabulary.ExcludedPromptRules = new List<ResultPromptRule>() { selectedPrompt.ResultPrompt };
                    }
                    else
                    {
                        if (NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.FirstOrDefault(x => x.UUID == selectedPrompt.ResultPrompt.UUID) == null)
                        {
                            NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Add(selectedPrompt.ResultPrompt);
                        }
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var selectedPrompt = (NowSubscriptionPromptViewModel)item;
                    if (NowSubscription.SubscriptionVocabulary.ExcludedPromptRules != null) { NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Remove(selectedPrompt.ResultPrompt); }
                }
            }

            OnPropertyChanged("ExcludedPrompts");
            OnPropertyChanged("HasExcludedPrompts");
            OnPropertyChanged("HasExcludedPromptsContextActions");
        }

        public bool HasIncludedPromptsContextActions
        {
            get
            {
                if (HasIncludedPrompts) { return true; }

                return ValidateSourceIsPrompt();
            }
        }

        private bool ValidateSourceIsPrompt()
        {
            if (treeModel.CopiedTreeViewModel != null && treeModel.CopiedTreeViewModel is ResultPromptTreeViewModel)
            {
                var item = (ResultPromptTreeViewModel)treeModel.CopiedTreeViewModel;
                if (item.ResultPromptRuleViewModel.IsDeleted || item.ResultPromptRuleViewModel.ResultPromptViewModel.IsDeleted)
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public bool HasIncludedPrompts
        {
            get => IncludedPrompts.Count > 0;
        }

        public SilentObservableCollection<NowSubscriptionPromptViewModel> IncludedPrompts { get; private set; }

        private void includedPrompts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var selectedPrompt = (NowSubscriptionPromptViewModel)item;
                    if (NowSubscription.SubscriptionVocabulary.IncludedPromptRules == null)
                    {
                        NowSubscription.SubscriptionVocabulary.IncludedPromptRules = new List<ResultPromptRule>() { selectedPrompt.ResultPrompt };
                    }
                    else
                    {
                        NowSubscription.SubscriptionVocabulary.IncludedPromptRules.Add(selectedPrompt.ResultPrompt);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var selectedPrompt = (NowSubscriptionPromptViewModel)item;
                    if (NowSubscription.SubscriptionVocabulary.IncludedPromptRules != null) { NowSubscription.SubscriptionVocabulary.IncludedPromptRules.Remove(selectedPrompt.ResultPrompt); }
                }
            }

            OnPropertyChanged("IncludedPrompts");
            OnPropertyChanged("HasIncludedPrompts");
            OnPropertyChanged("HasIncludedPromptsContextActions");
        }

        public bool HasExcludedResultsContextActions
        {
            get
            {
                if (HasExcludedResults) { return true; }

                return ValidateSourceIsResult();
            }
        }

        public bool HasExcludedResults
        {
            get => ExcludedResults.Count > 0;
        }

        public SilentObservableCollection<NowSubscriptionResultViewModel> ExcludedResults { get; private set; }

        private void excludedResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Log.Information("Subscription Excluded Results Changed");
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var selectedResult = (NowSubscriptionResultViewModel)item;
                    if (selectedResult != null)
                    {
                        Log.Information("SelectedResult : {0}", selectedResult.ResultDefinition == null ? "NULL" : selectedResult.ResultDefinition.Label);
                    }
                    Log.Information("Subscription Excluded Results Changed - Adding selected result");
                    if (NowSubscription.SubscriptionVocabulary.ExcludedResults == null)
                    {
                        NowSubscription.SubscriptionVocabulary.ExcludedResults = new List<ResultDefinition>() { selectedResult.ResultDefinition };
                    }
                    else
                    {
                        NowSubscription.SubscriptionVocabulary.ExcludedResults.Add(selectedResult.ResultDefinition);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var selectedResult = (NowSubscriptionResultViewModel)item;
                    if (NowSubscription.SubscriptionVocabulary.ExcludedResults != null) { NowSubscription.SubscriptionVocabulary.ExcludedResults.Remove(selectedResult.ResultDefinition); }
                }
            }

            OnPropertyChanged("ExcludedResults");
            OnPropertyChanged("HasExcludedResults");
            OnPropertyChanged("HasExcludedResultsContextActions");
        }

        public bool HasIncludedResultsContextActions
        {
            get
            {
                if (HasIncludedResults) { return true; }

                return ValidateSourceIsResult();
            }
        }

        private bool ValidateSourceIsResult()
        {
            if (treeModel.CopiedTreeViewModel != null && treeModel.CopiedTreeViewModel is ResultDefinitionTreeViewModel)
            {
                var item = (ResultDefinitionTreeViewModel)treeModel.CopiedTreeViewModel;
                if (item.ResultRuleViewModel.IsDeleted || item.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted)
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public bool HasIncludedResults
        {
            get => IncludedResults.Count > 0;
        }

        public SilentObservableCollection<NowSubscriptionResultViewModel> IncludedResults { get; private set; }

        private void includedResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Log.Information("Subscription Included Results Changed");
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var selectedResult = (NowSubscriptionResultViewModel)item;
                    if (selectedResult != null)
                    {
                        Log.Information("SelectedResult : {0}", selectedResult.ResultDefinition == null ? "NULL" : selectedResult.ResultDefinition.Label);
                    }
                    Log.Information("Subscription Included Results Changed - Adding selected result");
                    if (NowSubscription.SubscriptionVocabulary.IncludedResults == null)
                    {
                        NowSubscription.SubscriptionVocabulary.IncludedResults = new List<ResultDefinition>() { selectedResult.ResultDefinition };
                    }
                    else
                    {
                        NowSubscription.SubscriptionVocabulary.IncludedResults.Add(selectedResult.ResultDefinition);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var selectedResult = (NowSubscriptionResultViewModel)item;
                    if (NowSubscription.SubscriptionVocabulary.IncludedResults != null) { NowSubscription.SubscriptionVocabulary.IncludedResults.Remove(selectedResult.ResultDefinition); }
                }
            }

            OnPropertyChanged("IncludedResults");
            OnPropertyChanged("HasIncludedResults");
            OnPropertyChanged("HasIncludedResultsContextActions");
        }

        private void selectedUserGroupVariants_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var selectedUserGroup = (string)item;
                    if (NowSubscription.UserGroupVariants == null)
                    {
                        NowSubscription.UserGroupVariants = new List<string>() { selectedUserGroup };
                    }
                    else
                    {
                        NowSubscription.UserGroupVariants.Add(selectedUserGroup);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var selectedUserGroup = (string)item;
                    if (NowSubscription.UserGroupVariants != null) { NowSubscription.UserGroupVariants.Remove(selectedUserGroup); }
                }
            }

            OnPropertyChanged("SelectedUserGroupVariants");
        }

        public List<ComboBoxItemString> AllUserGroups { get; set; }

        public SilentObservableCollection<string> SelectedUserGroupVariants { get; set; }

        private void selectedCourtHouses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var selectedCourtHouse = (SelectedCourtHouse)item;
                    if (NowSubscription.SelectedCourtHouses == null)
                    {
                        NowSubscription.SelectedCourtHouses = new List<string>() { selectedCourtHouse.Code };
                    }
                    else
                    {
                        NowSubscription.SelectedCourtHouses.Add(selectedCourtHouse.Code);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var selectedCourtHouse = (SelectedCourtHouse)item;
                    if (NowSubscription.SelectedCourtHouses != null) { NowSubscription.SelectedCourtHouses.Remove(selectedCourtHouse.Code); }
                }
            }

            OnPropertyChanged("SelectedCourtHouses");
        }

        public List<ComboBoxItemString> AllCourtHouses { get; set; }

        public SilentObservableCollection<SelectedCourtHouse> SelectedCourtHouses { get; set; }

        public List<NowSubscriptionViewModel> ChildNowSubscriptions { get; set; }

        public NowSubscription ParentNowSubscription
        {
            get => NowSubscription.ParentNowSubscription;
        }

        #endregion Associated Models

        #region Now Excluded Result Add/Delete

        NowSubscriptionResultViewModel selectedNowExcludedResultValue = null;
        public NowSubscriptionResultViewModel SelectedNowExcludedResultValue
        {
            get => selectedNowExcludedResultValue;
            set
            {
                SetProperty(ref selectedNowExcludedResultValue, value);
                OnPropertyChanged("ExcludedResultSelected");
            }
        }

        public bool ExcludedResultSelected
        {
            get
            {
                return selectedNowExcludedResultValue != null;
            }
        }

        #endregion Now Excluded Result Add/Delete

        #region Now Included Result Add/Delete

        NowSubscriptionResultViewModel selectedNowIncludedResultValue = null;
        public NowSubscriptionResultViewModel SelectedNowIncludedResultValue
        {
            get => selectedNowIncludedResultValue;
            set
            {
                SetProperty(ref selectedNowIncludedResultValue, value);
                OnPropertyChanged("IncludedResultSelected");
            }
        }
        public bool IncludedResultSelected
        {
            get
            {
                return selectedNowIncludedResultValue != null;
            }
        }

        #endregion Now Included Result Add/Delete

        #region Now Excluded Prompt Add/Delete

        NowSubscriptionPromptViewModel selectedNowExcludedPromptValue = null;
        public NowSubscriptionPromptViewModel SelectedNowExcludedPromptValue
        {
            get => selectedNowExcludedPromptValue;
            set
            {
                SetProperty(ref selectedNowExcludedPromptValue, value);
                OnPropertyChanged("ExcludedResultPromptSelected");
            }
        }
        public bool ExcludedResultPromptSelected
        {
            get
            {
                return selectedNowExcludedPromptValue != null;
            }
        }

        #endregion Now Excluded Prompt Add/Delete

        #region Now Included Prompt Add/Delete

        NowSubscriptionPromptViewModel selectedNowIncludedPromptValue = null;
        public NowSubscriptionPromptViewModel SelectedNowIncludedPromptValue
        {
            get => selectedNowIncludedPromptValue;
            set
            {
                SetProperty(ref selectedNowIncludedPromptValue, value);
                OnPropertyChanged("IncludedResultPromptSelected");
            }
        }
        public bool IncludedResultPromptSelected
        {
            get
            {
                return selectedNowIncludedPromptValue != null;
            }
        }

        #endregion Now Included Prompt Add/Delete

        public NowSubscription NowSubscription
        {
            get { return data as NowSubscription; }
            private set { data = value; }
        }

        public string SubscriptionType
        {
            get
            {
                if (IsCourtRegister)
                {
                    return "Court Register";
                }
                if (IsPrisonCourtRegister)
                {
                    return "Prison Court Register";
                }
                if (IsInformantRegister)
                {
                    return "Informant Register";
                }
                if (IsNow)
                {
                    return "NOW";
                }
                return "EDT";
            }
        }

        public List<Now> IncludedNOWS
        {
            get => NowSubscription.IncludedNOWS;
        }

        public List<Now> ExcludedNOWS
        {
            get => NowSubscription.ExcludedNOWS;
        }

        #region Now Subscription Properties

        public bool IsNow
        {
            get => NowSubscription.IsNow;
        }

        public bool IsEDT
        {
            get => NowSubscription.IsEDT;
        }

        public bool IsInformantRegister
        {
            get => NowSubscription.IsInformantRegister;
        }

        public bool IsCourtRegister
        {
            get => NowSubscription.IsCourtRegister;
        }

        public bool IsPrisonCourtRegister
        {
            get => NowSubscription.IsPrisonCourtRegister;
        }

        public string Name
        {    
            get => NowSubscription.Name;
            set
                {
                    if (SetProperty(() => NowSubscription.Name == value, () => NowSubscription.Name = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool IsFirstClassLetter
        {
            get => NowSubscription.IsFirstClassLetter;
            set
            {
                if (SetProperty(() => NowSubscription.IsFirstClassLetter == value, () => NowSubscription.IsFirstClassLetter = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsSecondClassLetter
        {
            get => NowSubscription.IsSecondClassLetter;
            set
            {
                if (SetProperty(() => NowSubscription.IsSecondClassLetter == value, () => NowSubscription.IsSecondClassLetter = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsEmail
        {
            get => NowSubscription.IsEmail;
            set
            {
                if (SetProperty(() => NowSubscription.IsEmail == value, () => NowSubscription.IsEmail = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        //default the email template
                        if (IsCourtRegister)
                        {
                            EmailTemplateName = "cr_standard";
                        }
                        if (IsInformantRegister)
                        {
                            EmailTemplateName = "ir_standard";
                        }
                        if (IsPrisonCourtRegister)
                        {
                            EmailTemplateName = "pcr_standard";
                        }
                        if (IsNow)
                        {
                            EmailTemplateName = "now_sla_template";
                        }
                    }
                    else
                    {
                        EmailTemplateName = null;
                    }
                }
            }
        }

        public bool IsCPSNotificationAPI
        {
            get => NowSubscription.IsCPSNotificationAPI;
            set
            {
                if (SetProperty(() => NowSubscription.IsCPSNotificationAPI == value, () => NowSubscription.IsCPSNotificationAPI = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsForDistribution
        {
            get => NowSubscription.IsForDistribution;
            set
            {
                if (SetProperty(() => NowSubscription.IsForDistribution == value, () => NowSubscription.IsForDistribution = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if(value)
                    {
                        //default to second class letter
                        IsSecondClassLetter = true;
                    }
                    else
                    {
                        //remove the distribution channels
                        IsFirstClassLetter = false;
                        IsSecondClassLetter = false;
                        IsCPSNotificationAPI = false;
                        IsEmail = false;
                    }

                    OnPropertyChanged("DistributionOptionsVisibility");
                    OnPropertyChanged("CPSAPIDistributionVisibility");
                }
            }
        }

        public bool IsForDistributionEnabled
        {
            get => !NoRecipient;        
        }

        public bool CaseRecipient
        {
            get => NowSubscription.CaseRecipient;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipient == value, () => NowSubscription.CaseRecipient = value))
                {
                    LastModifiedDate = DateTime.Now;
                    if (NowSubscription.NowRecipient != null)
                    {
                        NowSubscription.NowRecipient.RecipientFromCase = value;
                    }

                    if (value)
                    {
                        if (NowSubscription.NowRecipient == null)
                        {
                            NowSubscription.NowRecipient = new NowRecipient(NowSubscription);
                            NowSubscription.NowRecipient.RecipientFromCase = true;
                        }

                        SubscriptionRecipient = false;
                        ResultsRecipient = false;
                        NoRecipient = false;
                    }
                    else
                    {
                        //null out the from case details
                        IsApplyApplicantDetails = false;
                        IsApplyRespondentDetails = false;
                        IsApplyDefendantDetails = false;
                        IsApplyDefenceOrganisationDetails = false;
                        IsApplyParentGuardianDetails = false;
                        IsApplyDefendantCustodyDetails = false;
                        IsApplyProsecutionAuthorityDetails = false;
                    }

                    if (NowSubscription.IsPrisonCourtRegister)
                    {
                        //can only ever be apply defendant custody details
                        IsApplyDefendantCustodyDetails = value;
                    }

                    OnPropertyChanged("RecipientFromCaseVisibility");
                }
            }
        }

        public bool SubscriptionRecipient
        {
            get => NowSubscription.SubscriptionRecipient;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionRecipient == value, () => NowSubscription.SubscriptionRecipient = value))
                {
                    LastModifiedDate = DateTime.Now;
                    if (NowSubscription.NowRecipient != null)
                    {
                        NowSubscription.NowRecipient.RecipientFromSubscription = value;
                    }

                    if (value)
                    {
                        if (NowSubscription.NowRecipient == null)
                        {
                            NowSubscription.NowRecipient = new NowRecipient(NowSubscription);
                            NowSubscription.NowRecipient.RecipientFromSubscription = true;
                        }
                        CaseRecipient = false;
                        ResultsRecipient = false;
                        NoRecipient = false;
                    }
                    else
                    {
                        //null out the from subscription details
                        SetPersonNameNull();
                        SetOrganisationNameNull();
                        SetAddressNull();
                    }

                    OnPropertyChanged("RecipientFromSubscriptionVisibility");
                }
            }
        }

        public bool ResultsRecipient
        {
            get => NowSubscription.ResultRecipient;
            set
            {
                if (SetProperty(() => NowSubscription.ResultRecipient == value, () => NowSubscription.ResultRecipient = value))
                {
                    LastModifiedDate = DateTime.Now;
                    if (NowSubscription.NowRecipient != null)
                    {
                        NowSubscription.NowRecipient.RecipientFromResults = value;
                    }

                    if (value)
                    {
                        if (NowSubscription.NowRecipient == null)
                        {
                            NowSubscription.NowRecipient = new NowRecipient(NowSubscription);
                            NowSubscription.NowRecipient.RecipientFromResults = true;
                        }
                        CaseRecipient = false;
                        SubscriptionRecipient = false;
                        NoRecipient = false;
                    }
                    else
                    {
                        //null out the from Results details
                        SetPersonNameResultPromptsNull();
                        SetOrganisationNameResultPromptsNull();
                        SetAddressResultPromptsNull();
                    }

                    OnPropertyChanged("RecipientFromResultsVisibility");
                }
            }
        }

        public bool NoRecipient
        {
            get => NowSubscription.NoRecipient;
            set
            {
                if (SetProperty(() => NowSubscription.NoRecipient == value, () => NowSubscription.NoRecipient = value))
                {
                    LastModifiedDate = DateTime.Now;   

                    if (value)
                    {
                        CaseRecipient = false;
                        SubscriptionRecipient = false;
                        ResultsRecipient = false;

                        //now null out the recipient
                        NowSubscription.NowRecipient = null;

                        //Now Ensure that there is no distribution
                        IsForDistribution = false;
                    }

                    OnPropertyChanged("IsForDistributionEnabled");
                }
            }
        }

        public bool CaseRecipientProsecution
        {
            get => NowSubscription.CaseRecipientProsecution;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientProsecution == value, () => NowSubscription.CaseRecipientProsecution = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyProsecutionAuthorityDetails = value;

                    if (value)
                    {
                        CaseRecipientThirdParty = false;
                        CaseRecipientDefenceOrganisation = false;
                        CaseRecipientParentGuardian = false;
                        CaseRecipientApplicant = false;
                        CaseRecipientRespondent = false;
                        CaseRecipientCustodyLocation = false;
                        CaseRecipientDefendant = false;
                    }
                }
            }
        }

        public bool CaseRecipientDefendant
        {
            get => NowSubscription.CaseRecipientDefendant;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientDefendant == value, () => NowSubscription.CaseRecipientDefendant = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyDefendantDetails = value;

                    if (value)
                    {
                        CaseRecipientThirdParty = false;
                        CaseRecipientDefenceOrganisation = false;
                        CaseRecipientParentGuardian = false;
                        CaseRecipientApplicant = false;
                        CaseRecipientRespondent = false;
                        CaseRecipientCustodyLocation = false;
                        CaseRecipientProsecution = false;
                    }
                }
            }
        }

        public bool CaseRecipientDefenceOrganisation
        {
            get => NowSubscription.CaseRecipientDefenceOrganisation;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientDefenceOrganisation == value, () => NowSubscription.CaseRecipientDefenceOrganisation = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyDefenceOrganisationDetails = value;

                    if (value)
                    {
                        CaseRecipientThirdParty = false;
                        CaseRecipientDefendant = false;
                        CaseRecipientParentGuardian = false;
                        CaseRecipientApplicant = false;
                        CaseRecipientRespondent = false;
                        CaseRecipientCustodyLocation = false;
                        CaseRecipientProsecution = false;
                    }
                }
            }
        }

        public bool CaseRecipientCustodyLocation
        {
            get => NowSubscription.CaseRecipientCustodyLocation;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientCustodyLocation == value, () => NowSubscription.CaseRecipientCustodyLocation = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyDefendantCustodyDetails = value;

                    if (value)
                    {
                        CaseRecipientThirdParty = false;
                        CaseRecipientDefendant = false;
                        CaseRecipientParentGuardian = false;
                        CaseRecipientApplicant = false;
                        CaseRecipientRespondent = false;
                        CaseRecipientDefenceOrganisation = false;
                        CaseRecipientProsecution = false;
                    }
                }
            }
        }

        public bool CaseRecipientParentGuardian
        {
            get => NowSubscription.CaseRecipientParentGuardian;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientParentGuardian == value, () => NowSubscription.CaseRecipientParentGuardian = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyParentGuardianDetails = value;

                    if (value)
                    {
                        CaseRecipientThirdParty = false;
                        CaseRecipientDefendant = false;
                        CaseRecipientDefenceOrganisation = false;
                        CaseRecipientApplicant = false;
                        CaseRecipientRespondent = false;
                        CaseRecipientCustodyLocation = false;
                        CaseRecipientProsecution = false;
                    }
                }
            }
        }

        public bool CaseRecipientApplicant
        {
            get => NowSubscription.CaseRecipientApplicant;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientApplicant == value, () => NowSubscription.CaseRecipientApplicant = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyApplicantDetails = value;

                    if (value)
                    {
                        CaseRecipientThirdParty = false;
                        CaseRecipientDefendant = false;
                        CaseRecipientDefenceOrganisation = false;
                        CaseRecipientParentGuardian = false;
                        CaseRecipientRespondent = false;
                        CaseRecipientCustodyLocation = false;
                        CaseRecipientProsecution = false;
                    }
                }
            }
        }

        public bool CaseRecipientRespondent
        {
            get => NowSubscription.CaseRecipientRespondent;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientRespondent == value, () => NowSubscription.CaseRecipientRespondent = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyRespondentDetails = value;

                    if (value)
                    {
                        CaseRecipientThirdParty = false;
                        CaseRecipientDefendant = false;
                        CaseRecipientDefenceOrganisation = false;
                        CaseRecipientParentGuardian = false;
                        CaseRecipientApplicant = false;
                        CaseRecipientCustodyLocation = false;
                        CaseRecipientProsecution = false;
                    }
                }
            }
        }

        public bool CaseRecipientThirdParty
        {
            get => NowSubscription.CaseRecipientThirdParty;
            set
            {
                if (SetProperty(() => NowSubscription.CaseRecipientThirdParty == value, () => NowSubscription.CaseRecipientThirdParty = value))
                {
                    LastModifiedDate = DateTime.Now;
                    NowSubscription.NowRecipient.IsApplyThirdPartyDetails = value;

                    if (value)
                    {
                        CaseRecipientRespondent = false;
                        CaseRecipientDefendant = false;
                        CaseRecipientDefenceOrganisation = false;
                        CaseRecipientParentGuardian = false;
                        CaseRecipientApplicant = false;
                        CaseRecipientCustodyLocation = false;
                        CaseRecipientProsecution = false;
                    }
                }
            }
        }

        

        public bool RecipientFromResultsVisibility
        {
            get
            {
                return NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.RecipientFromResults;
            }
        }

        public bool RecipientFromCaseVisibility
        {
            get
            {
                return NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.RecipientFromCase; 
            }
        }

        public bool RecipientFromSubscriptionVisibility
        {
            get
            {
                return NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.RecipientFromSubscription;
            }
        }

        public bool InformantRegisterVisibility
        {
            get
            {
                return InformantRegisterRecipient;
            }
        }

        public bool CourtRegisterVisibility
        {
            get
            {
                return CourtRegisterRecipient;
            }
        }

        public bool DistributionOptionsVisibility
        {
            get
            {
                return IsForDistribution;
            }
        }

        public bool CPSAPIDistributionVisibility
        {
            get
            {
                return IsForDistribution && IsProsecutedByCPS;
            }
        }

        public bool SubscriptionRulesVisibility
        {
            get
            {
                return ApplySubscriptionRules;
            }
        }

        #region Registers

        public string InformantCode
        {
            get => NowSubscription.InformantCode;
            set
            {
                if (SetProperty(() => NowSubscription.InformantCode == value, () => NowSubscription.InformantCode = value))
                {

                    //set the email address to the default for this informant

                    var informant = treeModel.Informants.FirstOrDefault(x => x.CodeString == value);
                    if (informant != null)
                    {
                        EmailAddress1 = informant.EmailAddressString;                        
                        OrganisationName = informant.ValueString;
                    }
                    else
                    {
                        EmailAddress1 = null;
                        OrganisationName = null;
                    }

                    OnPropertyChanged("EmailAddress1");
                    OnPropertyChanged("OrganisationName");

                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string YOTsCode
        {
            get => NowSubscription.YOTsCode;
            set
            {
                if (SetProperty(() => NowSubscription.YOTsCode == value, () => NowSubscription.YOTsCode = value))
                {

                    //set the email address to the default for this yot

                    var yot = treeModel.Yots.FirstOrDefault(x => x.CodeString == value);
                    if (yot != null)
                    {
                        EmailAddress1 = yot.EmailAddressString;
                        OrganisationName = yot.ValueString;
                    }
                    else
                    {
                        EmailAddress1 = null;
                        OrganisationName = null;
                    }

                    OnPropertyChanged("EmailAddress1");
                    OnPropertyChanged("OrganisationName");

                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #endregion Registers

        #region Subscription rules
        public bool ApplySubscriptionRules
        {
            get => NowSubscription.ApplySubscriptionRules;
            set
            {
                if (SetProperty(() => NowSubscription.ApplySubscriptionRules == value, () => NowSubscription.ApplySubscriptionRules = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        //default the settings
                        AnyAppearance = true;
                        AdultOrYouthDefendant = true;
                        AdultOrYouthDefendant = true;
                        AnyCourtHearing = true;
                        IgnoreCustody = true;
                        IgnoreResults = true;
                    }
                    else
                    {
                        //undo the settings
                        AnyAppearance = false;
                        AppearedByVideoLink = false;
                        AppearedInPerson = false;
                        AdultDefendant = false;
                        YouthDefendant = false;
                        AdultOrYouthDefendant = false;
                        WelshCourtHearing = false;
                        EnglishCourtHearing = false;
                        AnyCourtHearing = true;
                        IsProsecutedByCPS = false;
                        AtleastOneCustodialResult = false;
                        AtleastOneNonCustodialResult = false;
                        AllNonCustodialResults = false;
                        InCustody = false;
                        IgnoreCustody = false;
                        IgnoreResults = false;
                        CustodyLocationIsPolice = false;
                        CustodyLocationIsPrison = false;
                        ExcludedPrompts.Clear();
                        IncludedPrompts.Clear();
                    }

                    OnPropertyChanged("SubscriptionRulesVisibility");
                }
            }
        }

        public bool WelshCourtHearing
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.WelshCourtHearing;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.WelshCourtHearing == value, () => NowSubscription.SubscriptionVocabulary.WelshCourtHearing = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        EnglishCourtHearing = false;
                        AnyCourtHearing = false;
                    }
                }
            }
        }

        public bool EnglishCourtHearing
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.EnglishCourtHearing;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.EnglishCourtHearing == value, () => NowSubscription.SubscriptionVocabulary.EnglishCourtHearing = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        WelshCourtHearing = false;
                        AnyCourtHearing = false;
                    }
                }
            }
        }

        public bool AnyCourtHearing
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AnyCourtHearing;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AnyCourtHearing == value, () => NowSubscription.SubscriptionVocabulary.AnyCourtHearing = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        WelshCourtHearing = false;
                        EnglishCourtHearing = false;
                    }
                }
            }
        }

        public bool IsProsecutedByCPS
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.IsProsecutedByCPS;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.IsProsecutedByCPS == value, () => NowSubscription.SubscriptionVocabulary.IsProsecutedByCPS = value))
                {
                    LastModifiedDate = DateTime.Now;
                    if (value)
                    {
                        //default the distribution to cps api
                        IsForDistribution = true;
                        IsCPSNotificationAPI = true;
                    }
                    else
                    {
                        //undo the cps distribution API
                        IsCPSNotificationAPI = false;
                        if (IsForDistribution)
                        {
                            if (!IsFirstClassLetter && !IsSecondClassLetter && !IsEmail)
                            {
                                IsForDistribution = false;
                            }
                        }
                    }
                    OnPropertyChanged("CPSAPIDistributionVisibility");
                }                
            }
        }

        public bool AnyAppearance
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AnyAppearance;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AnyAppearance == value, () => NowSubscription.SubscriptionVocabulary.AnyAppearance = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        AppearedInPerson = false;
                        AppearedByVideoLink = false;
                    }
                }
            }
        }

        public bool AppearedByVideoLink
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AppearedByVideoLink;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AppearedByVideoLink == value, () => NowSubscription.SubscriptionVocabulary.AppearedByVideoLink = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        AppearedInPerson = false;
                        AnyAppearance = false;
                    }
                }
            }
        }

        public bool AppearedInPerson
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AppearedInPerson;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AppearedInPerson == value, () => NowSubscription.SubscriptionVocabulary.AppearedInPerson = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        AppearedByVideoLink = false;
                        AnyAppearance = false;
                    }
                }
            }
        }

        public bool AllNonCustodialResults
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AllNonCustodialResults;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AllNonCustodialResults == value, () => NowSubscription.SubscriptionVocabulary.AllNonCustodialResults = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        AtleastOneCustodialResult = false;
                        IgnoreResults = false;
                    }
                }
            }
        }

        public bool AtleastOneNonCustodialResult
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AtleastOneNonCustodialResult;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AtleastOneNonCustodialResult == value, () => NowSubscription.SubscriptionVocabulary.AtleastOneNonCustodialResult = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        IgnoreResults = false;
                    }
                }
            }
        }

        public bool AtleastOneCustodialResult
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AtleastOneCustodialResult;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AtleastOneCustodialResult == value, () => NowSubscription.SubscriptionVocabulary.AtleastOneCustodialResult = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        AllNonCustodialResults = false;
                        IgnoreResults = false;
                    }
                }
            }
        }

        public bool IgnoreResults
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.IgnoreResults;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.IgnoreResults == value, () => NowSubscription.SubscriptionVocabulary.IgnoreResults = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        AllNonCustodialResults = false;
                        AtleastOneCustodialResult = false;
                        AtleastOneNonCustodialResult = false;
                    }
                }
            }
        }

        public bool ProsecutorMajorCreditor
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.ProsecutorMajorCreditor;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.ProsecutorMajorCreditor == value, () => NowSubscription.SubscriptionVocabulary.ProsecutorMajorCreditor = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        NonProsecutorMajorCreditor = false;
                        AnyMajorCreditor = false;
                    }
                }
            }
        }

        public bool NonProsecutorMajorCreditor
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.NonProsecutorMajorCreditor;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.NonProsecutorMajorCreditor == value, () => NowSubscription.SubscriptionVocabulary.NonProsecutorMajorCreditor = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        ProsecutorMajorCreditor = false;
                        AnyMajorCreditor = false;
                    }
                }
            }
        }

        public bool AnyMajorCreditor
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AnyMajorCreditor;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AnyMajorCreditor == value, () => NowSubscription.SubscriptionVocabulary.AnyMajorCreditor = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        ProsecutorMajorCreditor = false;
                        NonProsecutorMajorCreditor = false;
                    }
                }
            }
        }

        public bool InCustody
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.InCustody;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.InCustody == value, () => NowSubscription.SubscriptionVocabulary.InCustody = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (!value)
                    {
                        CustodyLocationIsPrison = false;
                        CustodyLocationIsPolice = false;
                        IgnoreCustody = false;
                    }
                }
            }
        }

        public bool CustodyLocationIsPrison
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.CustodyLocationIsPrison;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.CustodyLocationIsPrison == value, () => NowSubscription.SubscriptionVocabulary.CustodyLocationIsPrison = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        InCustody = true;
                        CustodyLocationIsPolice = false;
                        IgnoreCustody = false;
                    }
                }
            }
        }

        public bool CustodyLocationIsPolice
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.CustodyLocationIsPolice;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.CustodyLocationIsPolice == value, () => NowSubscription.SubscriptionVocabulary.CustodyLocationIsPolice = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        InCustody = true;
                        CustodyLocationIsPrison = false;
                        IgnoreCustody = false;
                    }
                }
            }
        }

        public bool IgnoreCustody
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.IgnoreCustody;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.IgnoreCustody == value, () => NowSubscription.SubscriptionVocabulary.IgnoreCustody = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        CustodyLocationIsPrison = false;
                        InCustody = false;
                        CustodyLocationIsPolice = false;
                    }
                }
            }
        }

        public bool YouthDefendant
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.YouthDefendant;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.YouthDefendant == value, () => NowSubscription.SubscriptionVocabulary.YouthDefendant = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        AdultDefendant = false;
                        AdultOrYouthDefendant = false;
                    }
                }
            }
        }

        public bool AdultDefendant
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AdultDefendant;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AdultDefendant == value, () => NowSubscription.SubscriptionVocabulary.AdultDefendant = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        YouthDefendant = false;
                        AdultOrYouthDefendant = false;
                    }
                }
            }
        }

        public bool AdultOrYouthDefendant
        {
            get => NowSubscription.SubscriptionVocabulary == null ? false : NowSubscription.SubscriptionVocabulary.AdultOrYouthDefendant;
            set
            {
                if (SetProperty(() => NowSubscription.SubscriptionVocabulary != null && NowSubscription.SubscriptionVocabulary.AdultOrYouthDefendant == value, () => NowSubscription.SubscriptionVocabulary.AdultOrYouthDefendant = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (value)
                    {
                        YouthDefendant = false;
                        AdultDefendant = false;
                    }
                }
            }
        }

        #endregion

        #region Recipient 

        public bool InformantRegisterRecipient
        {
            get => NowSubscription.IsInformantRegister;
        }

        public bool CourtRegisterRecipient
        {
            get => NowSubscription.IsCourtRegister;
        }

        public bool IsApplyDefenceOrganisationDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyDefenceOrganisationDetails;
            private set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyDefenceOrganisationDetails == value, () => NowSubscription.NowRecipient.IsApplyDefenceOrganisationDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsApplyParentGuardianDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyParentGuardianDetails;
            private set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyParentGuardianDetails == value, () => NowSubscription.NowRecipient.IsApplyParentGuardianDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsApplyApplicantDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyApplicantDetails;
            private set
            {
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyApplicantDetails == value, () => NowSubscription.NowRecipient.IsApplyApplicantDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsApplyDefendantDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyDefendantDetails;
            private set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyDefendantDetails == value, () => NowSubscription.NowRecipient.IsApplyDefendantDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsApplyThirdPartyDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyThirdPartyDetails;
            private set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyThirdPartyDetails == value, () => NowSubscription.NowRecipient.IsApplyThirdPartyDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsApplyRespondentDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyRespondentDetails;
            private set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyRespondentDetails == value, () => NowSubscription.NowRecipient.IsApplyRespondentDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsApplyDefendantCustodyDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyDefendantCustodyDetails;
            private set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyDefendantCustodyDetails == value, () => NowSubscription.NowRecipient.IsApplyDefendantCustodyDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsApplyProsecutionAuthorityDetails
        {
            get => NowSubscription.NowRecipient == null ? false : NowSubscription.NowRecipient.IsApplyProsecutionAuthorityDetails;
            private set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.IsApplyProsecutionAuthorityDetails == value, () => NowSubscription.NowRecipient.IsApplyProsecutionAuthorityDetails = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }        

        #region Name
        private void SetPersonNameNull()
        {
            Title = null;
            FirstName = null;
            MiddleName = null;
            LastName = null;
        }

        private void SetOrganisationNameNull()
        {
            OrganisationName = null;
        }

        private void SetPersonNameResultPromptsNull()
        {
            TitleResultPrompt = null;
            FirstNameResultPrompt = null;
            MiddleNameResultPrompt = null;
            LastNameResultPrompt = null;
        }

        private void SetOrganisationNameResultPromptsNull()
        {
            OrganisationNameResultPrompt = null;
        }

        private void SetAddressNull()
        {
            Address1 = null;
            Address2 = null;
            Address3 = null;
            Address4 = null;
            Address5 = null;
            PostCode = null;
            EmailAddress1 = null;
            EmailAddress2 = null;
        }

        private void SetAddressResultPromptsNull()
        {
            Address1ResultPrompt = null;
            Address2ResultPrompt = null;
            Address3ResultPrompt = null;
            Address4ResultPrompt = null;
            Address5ResultPrompt = null;
            PostCodeResultPrompt = null;
            EmailAddress1ResultPrompt = null;
            EmailAddress2ResultPrompt = null;
        }

        public string Title
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Title;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Title == value, () => NowSubscription.NowRecipient.Title = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(value))
                    {
                        NowSubscription.NowRecipient.OrganisationName = null;
                        OnPropertyChanged("OrganisationName");
                    }
                }
            }
        }

        public string FirstName
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.FirstName;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.FirstName == value, () => NowSubscription.NowRecipient.FirstName = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if(!string.IsNullOrEmpty(value))
                    {
                        NowSubscription.NowRecipient.OrganisationName = null;
                        OnPropertyChanged("OrganisationName");
                    }
                }
            }
        }

        public string MiddleName
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.MiddleName;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.MiddleName == value, () => NowSubscription.NowRecipient.MiddleName = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(value))
                    {
                        NowSubscription.NowRecipient.OrganisationName = null;
                        OnPropertyChanged("OrganisationName");
                    }
                }
            }
        }

        public string LastName
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.LastName;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.LastName == value, () => NowSubscription.NowRecipient.LastName = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(value))
                    {
                        NowSubscription.NowRecipient.OrganisationName = null;
                        OnPropertyChanged("OrganisationName");
                    }
                }
            }
        }

        public string OrganisationName
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.OrganisationName;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.OrganisationName == value, () => NowSubscription.NowRecipient.OrganisationName = value))
                {
                    LastModifiedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(value))
                    {
                        NowSubscription.NowRecipient.Title = null;
                        OnPropertyChanged("Title");
                        NowSubscription.NowRecipient.FirstName = null;
                        OnPropertyChanged("FirstName");
                        NowSubscription.NowRecipient.MiddleName = null;
                        OnPropertyChanged("MiddleName");
                        NowSubscription.NowRecipient.LastName = null;
                        OnPropertyChanged("LastName");
                    }
                }
            }
        }


        public string TitleResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.TitleResultPromptReference;
        }
        public ResultPromptRule TitleResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.TitleResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.TitleResultPrompt == value, () => NowSubscription.NowRecipient.TitleResultPrompt = value))
                {
                    OnPropertyChanged("TitleResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string FirstNameResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.FirstNameResultPromptReference;
        }
        public ResultPromptRule FirstNameResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.FirstNameResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.FirstNameResultPrompt == value, () => NowSubscription.NowRecipient.FirstNameResultPrompt = value))
                {
                    OnPropertyChanged("FirstNameResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string MiddleNameResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.MiddleNameResultPromptReference;
        }
        public ResultPromptRule MiddleNameResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.MiddleNameResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.MiddleNameResultPrompt == value, () => NowSubscription.NowRecipient.MiddleNameResultPrompt = value))
                {
                    OnPropertyChanged("MiddleNameResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string LastNameResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.LastNameResultPromptReference;
        }
        public ResultPromptRule LastNameResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.LastNameResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.LastNameResultPrompt == value, () => NowSubscription.NowRecipient.LastNameResultPrompt = value))
                {
                    OnPropertyChanged("LastNameResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string OrganisationNameResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.OrganisationNameResultPromptReference;
        }
        public ResultPromptRule OrganisationNameResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.OrganisationNameResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.OrganisationNameResultPrompt == value, () => NowSubscription.NowRecipient.OrganisationNameResultPrompt = value))
                {
                    OnPropertyChanged("OrganisationNameResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #endregion Name

        #region Address

        public string Address1
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address1;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address1 == value, () => NowSubscription.NowRecipient.Address1 = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address2
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address2;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address2 == value, () => NowSubscription.NowRecipient.Address2 = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address3
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address3;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address3 == value, () => NowSubscription.NowRecipient.Address3 = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address4
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address4;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address4 == value, () => NowSubscription.NowRecipient.Address4 = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address5
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address5;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address5 == value, () => NowSubscription.NowRecipient.Address5 = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string PostCode
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.PostCode;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.PostCode == value, () => NowSubscription.NowRecipient.PostCode = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string EmailAddress1
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.EmailAddress1;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.EmailAddress1 == value, () => NowSubscription.NowRecipient.EmailAddress1 = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string EmailAddress2
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.EmailAddress2;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.EmailAddress2 == value, () => NowSubscription.NowRecipient.EmailAddress2 = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address1ResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address1ResultPromptReference;
        }
        public ResultPromptRule Address1ResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address1ResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address1ResultPrompt == value, () => NowSubscription.NowRecipient.Address1ResultPrompt = value))
                {
                    OnPropertyChanged("Address1ResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address2ResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address2ResultPromptReference;
        }
        public ResultPromptRule Address2ResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address2ResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address2ResultPrompt == value, () => NowSubscription.NowRecipient.Address2ResultPrompt = value))
                {
                    OnPropertyChanged("Address2ResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address3ResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address3ResultPromptReference;
        }
        public ResultPromptRule Address3ResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address3ResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address3ResultPrompt == value, () => NowSubscription.NowRecipient.Address3ResultPrompt = value))
                {
                    OnPropertyChanged("Address3ResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address4ResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address4ResultPromptReference;
        }
        public ResultPromptRule Address4ResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address4ResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address4ResultPrompt == value, () => NowSubscription.NowRecipient.Address4ResultPrompt = value))
                {
                    OnPropertyChanged("Address4ResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Address5ResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address5ResultPromptReference;
        }
        public ResultPromptRule Address5ResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.Address5ResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.Address5ResultPrompt == value, () => NowSubscription.NowRecipient.Address5ResultPrompt = value))
                {
                    OnPropertyChanged("Address5ResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string PostCodeResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.PostCodeResultPromptReference;
        }
        public ResultPromptRule PostCodeResultPrompt
        {
            get => NowSubscription.NowRecipient.PostCodeResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.PostCodeResultPrompt == value, () => NowSubscription.NowRecipient.PostCodeResultPrompt = value))
                {
                    OnPropertyChanged("PostCodeResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string EmailAddress1ResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.EmailAddress1ResultPromptReference;
        }
        public ResultPromptRule EmailAddress1ResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.EmailAddress1ResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.EmailAddress1ResultPrompt == value, () => NowSubscription.NowRecipient.EmailAddress1ResultPrompt = value))
                {
                    OnPropertyChanged("EmailAddress1ResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string EmailAddress2ResultPromptDisplay
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.EmailAddress2ResultPromptReference;
        }
        public ResultPromptRule EmailAddress2ResultPrompt
        {
            get => NowSubscription.NowRecipient == null ? null : NowSubscription.NowRecipient.EmailAddress2ResultPrompt;
            set
            {
                if (NowSubscription.NowRecipient == null) { NowSubscription.NowRecipient = new NowRecipient(NowSubscription); }
                if (SetProperty(() => NowSubscription.NowRecipient.EmailAddress2ResultPrompt == value, () => NowSubscription.NowRecipient.EmailAddress2ResultPrompt = value))
                {
                    OnPropertyChanged("EmailAddress2ResultPromptDisplay");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #endregion Address

        #endregion Recipient

        public string EmailTemplateName
        {
            get => NowSubscription.EmailTemplateName;
            set
            {
                if (SetProperty(() => NowSubscription.EmailTemplateName == value, () => NowSubscription.EmailTemplateName = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string BilingualEmailTemplateName
        {
            get => NowSubscription.BilingualEmailTemplateName;
            set
            {
                if (SetProperty(() => NowSubscription.BilingualEmailTemplateName == value, () => NowSubscription.BilingualEmailTemplateName = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public DateTime? StartDate
        {
            get => NowSubscription.StartDate;
            set
            {
                if (SetProperty(() => NowSubscription.StartDate == value, () => NowSubscription.StartDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public DateTime? EndDate
        {
            get => NowSubscription.EndDate;
            set
            {
                if (SetProperty(() => NowSubscription.EndDate == value, () => NowSubscription.EndDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #endregion // Now Subscription Properties

        #region Copy Members

        public NowSubscriptionViewModel Copy()
        {
            NowSubscriptionViewModel res = (NowSubscriptionViewModel)MemberwiseClone();

            //make new data
            var newData = res.NowSubscription.Copy();
            res.NowSubscription = newData;

            //make view models
            res.LoadAssociatedModels();

            return res;
        }

        public NowSubscriptionViewModel Draft(List<string> publicationTags)
        {
            NowSubscriptionViewModel res = (NowSubscriptionViewModel)MemberwiseClone();

            //make new data
            var newData = res.NowSubscription.Draft();

            //replace any dependant children with draft versions where applicable
            ResetNowRecipientRulesToDraft(newData.NowRecipient);
            newData.IncludedNOWS = ResetNowsToDraft(newData.IncludedNOWS);
            newData.ExcludedNOWS = ResetNowsToDraft(newData.ExcludedNOWS);
            ResetVocabularyChildrenToDraft(newData.SubscriptionVocabulary);
            newData.ChildNowSubscriptions = ResetChildNowSubscriptionsToDraft(newData.ChildNowSubscriptions);

            //set the publication tags and set the data on the view model
            newData.PublicationTags = publicationTags;
            res.NowSubscription = newData;

            //make view models by reloading the associated models
            res.LoadAssociatedModels();

            return res;
        }

        public void ResetRules(ResultDefinitionViewModel rd)
        {
            if (rd == null) { return; }

            //set the recipient prompt rules
            ResetNowRecipientRules(rd.Prompts.ToList());

            //set the included, excluded results
            ResetResultRules(rd);

            //set the included excluded prompt rules
            ResetResultPromptRules(rd.Prompts.ToList());

            //reload the models
            LoadAssociatedModels();
        }

        private void ResetResultRules(ResultDefinitionViewModel rd)
        {
            if (rd == null || NowSubscription.SubscriptionVocabulary == null) { return; }

            if (NowSubscription.SubscriptionVocabulary.IncludedResults != null)
            {
                var matched = NowSubscription.SubscriptionVocabulary.IncludedResults.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (rd.MasterUUID ?? rd.UUID));
                if (matched != null)
                {
                    var index = NowSubscription.SubscriptionVocabulary.IncludedResults.IndexOf(matched);
                    if (index == -1) { return; }
                    NowSubscription.SubscriptionVocabulary.IncludedResults.RemoveAt(index);
                    NowSubscription.SubscriptionVocabulary.IncludedResults.Insert(index, rd.ResultDefinition);
                    LastModifiedDate = DateTime.Now;
                }
            }
            if (NowSubscription.SubscriptionVocabulary.ExcludedResults != null)
            {
                var matched = NowSubscription.SubscriptionVocabulary.ExcludedResults.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (rd.MasterUUID ?? rd.UUID));
                if (matched != null)
                {
                    var index = NowSubscription.SubscriptionVocabulary.ExcludedResults.IndexOf(matched);
                    if (index == -1) { return; }
                    NowSubscription.SubscriptionVocabulary.ExcludedResults.RemoveAt(index);
                    NowSubscription.SubscriptionVocabulary.ExcludedResults.Insert(index, rd.ResultDefinition);
                    LastModifiedDate = DateTime.Now;
                }
            }
        }
        
        private void ResetResultPromptRules(List<ResultPromptRuleViewModel> rules)
        {
            if (rules == null || NowSubscription.SubscriptionVocabulary == null) { return; }

            if (NowSubscription.SubscriptionVocabulary.IncludedPromptRules != null)
            {
                var matched = (from inc in NowSubscription.SubscriptionVocabulary.IncludedPromptRules
                               join rule in rules on inc.MasterUUID ?? inc.UUID equals rule.MasterUUID ?? rule.UUID
                               select new { inc, rule }).ToList();

                if (matched.Any())
                {
                    foreach (var item in matched)
                    {
                        var index = NowSubscription.SubscriptionVocabulary.IncludedPromptRules.IndexOf(item.inc);
                        if (index == -1) { continue; }
                        NowSubscription.SubscriptionVocabulary.IncludedPromptRules.RemoveAt(index);
                        NowSubscription.SubscriptionVocabulary.IncludedPromptRules.Insert(index, item.rule.ResultPromptRule);

                    }
                    LastModifiedDate = DateTime.Now;
                }
            }

            if (NowSubscription.SubscriptionVocabulary.ExcludedPromptRules != null)
            {
                var matched = (from inc in NowSubscription.SubscriptionVocabulary.ExcludedPromptRules
                               join rule in rules on inc.MasterUUID ?? inc.UUID equals rule.MasterUUID ?? rule.UUID
                               select new { inc, rule }).ToList();

                if (matched.Any())
                {
                    foreach (var item in matched)
                    {
                        var index = NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.IndexOf(item.inc);
                        if (index == -1) { continue; }
                        NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.RemoveAt(index);
                        NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Insert(index, item.rule.ResultPromptRule);

                    }
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        private void ResetNowRecipientRules(List<ResultPromptRuleViewModel> rules)
        {
            if (NowSubscription.NowRecipient == null || rules == null) { return; }

            var matched = MatchNowRecipientPromptRule(TitleResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.TitleResultPrompt = matched))
            {
                OnPropertyChanged("TitleResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(FirstNameResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.FirstNameResultPrompt = matched))
            {
                OnPropertyChanged("FirstNameResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(MiddleNameResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.MiddleNameResultPrompt = matched))
            {
                OnPropertyChanged("MiddleNameResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(LastNameResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.LastNameResultPrompt = matched))
            {
                OnPropertyChanged("LastNameResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(OrganisationNameResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.OrganisationNameResultPrompt = matched))
            {
                OnPropertyChanged("OrganisationNameResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(Address1ResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.Address1ResultPrompt = matched))
            {
                OnPropertyChanged("Address1ResultPromptResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(Address2ResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.Address2ResultPrompt = matched))
            {
                OnPropertyChanged("Address2ResultPromptResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(Address3ResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.Address3ResultPrompt = matched))
            {
                OnPropertyChanged("Address3ResultPromptResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(Address4ResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.Address4ResultPrompt = matched))
            {
                OnPropertyChanged("Address4ResultPromptResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(Address5ResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.Address5ResultPrompt = matched))
            {
                OnPropertyChanged("Address5ResultPromptResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(EmailAddress1ResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.EmailAddress1ResultPrompt = matched))
            {
                OnPropertyChanged("EmailAddress1ResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
            matched = MatchNowRecipientPromptRule(EmailAddress2ResultPrompt, rules);
            if (SetProperty(() => matched != null, () => NowSubscription.NowRecipient.EmailAddress2ResultPrompt = matched))
            {
                OnPropertyChanged("EmailAddress2ResultPromptDisplay");
                LastModifiedDate = DateTime.Now;
            }
        }

        private ResultPromptRule MatchNowRecipientPromptRule(ResultPromptRule target, List<ResultPromptRuleViewModel> rules)
        {
            if (target == null || rules == null) { return null; }
            var matched = rules.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (target.MasterUUID ?? target.UUID));
            if (matched == null) { return null; }
            return matched.ResultPromptRule;
        }

        private void ResetNowRecipientRulesToDraft(NowRecipient nowRecipient)
        {
            if (nowRecipient == null) { return; }
            nowRecipient.TitleResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.TitleResultPrompt);
            nowRecipient.FirstNameResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.FirstNameResultPrompt);
            nowRecipient.MiddleNameResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.MiddleNameResultPrompt);
            nowRecipient.LastNameResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.LastNameResultPrompt);
            nowRecipient.OrganisationNameResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.OrganisationNameResultPrompt);
            nowRecipient.Address1ResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.Address1ResultPrompt);
            nowRecipient.Address2ResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.Address2ResultPrompt);
            nowRecipient.Address3ResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.Address3ResultPrompt);
            nowRecipient.Address4ResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.Address4ResultPrompt);
            nowRecipient.Address5ResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.Address5ResultPrompt);
            nowRecipient.EmailAddress1ResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.EmailAddress1ResultPrompt);
            nowRecipient.EmailAddress2ResultPrompt = ResetNowRecipientPromptRuleToDraft(nowRecipient.EmailAddress2ResultPrompt);
        }

        private ResultPromptRule ResetNowRecipientPromptRuleToDraft(ResultPromptRule rpr)
        {
            if (rpr == null) { return null; }
            var matchedDraft = treeModel.AllResultPromptViewModel.Prompts.FirstOrDefault(
                        x => x.PublishedStatus == PublishedStatus.Draft
                        && (x.MasterUUID ?? x.UUID) == (rpr.MasterUUID ?? rpr.UUID)
                        );
            if (matchedDraft == null) { return rpr; }
            return matchedDraft.ResultPromptRule ?? rpr;
        }

        private List<NowSubscription> ResetChildNowSubscriptionsToDraft(List<NowSubscription> source)
        {
            var newChildren = new List<NowSubscription>();
            if (source == null) { return newChildren; }

            var draftSubscriptions = (from dns in treeModel.AllNowSubscriptionViewModel.NowSubscriptions
                              join ns in source on dns.MasterUUID ?? dns.UUID equals ns.MasterUUID ?? ns.UUID
                              where dns.PublishedStatus == PublishedStatus.Draft
                              select dns).ToList();

            //rebuild the child subscription collection
            if (draftSubscriptions.Any())
            {
                foreach (var rpr in source)
                {
                    var match = draftSubscriptions.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (rpr.MasterUUID ?? rpr.UUID));
                    if (match == null)
                    {
                        newChildren.Add(rpr);
                    }
                    else
                    {
                        newChildren.Add(match.NowSubscription);
                    }
                }

                return newChildren;
            }
            else
            {
                return source;
            }            
        }

        private void ResetVocabularyChildrenToDraft(SubscriptionVocabulary subscriptionVocabulary)
        {
            if (subscriptionVocabulary == null) { return; }

            subscriptionVocabulary.IncludedResults = ResetResultsToDraft(subscriptionVocabulary.IncludedResults);
            subscriptionVocabulary.ExcludedResults = ResetResultsToDraft(subscriptionVocabulary.ExcludedResults);
            subscriptionVocabulary.IncludedPromptRules = ResetPromptRulesToDraft(subscriptionVocabulary.IncludedPromptRules);
            subscriptionVocabulary.ExcludedPromptRules = ResetPromptRulesToDraft(subscriptionVocabulary.ExcludedPromptRules);
        }

        private List<ResultPromptRule> ResetPromptRulesToDraft(List<ResultPromptRule> source)
        {
            var newRules = new List<ResultPromptRule>();
            if (source == null) { return newRules; }

            var draftRules = (from dpr in treeModel.AllResultPromptViewModel.Prompts
                                join rpr in source on dpr.MasterUUID ?? dpr.UUID equals rpr.MasterUUID ?? rpr.UUID
                                where dpr.PublishedStatus == PublishedStatus.Draft
                                select dpr).ToList();

            //rebuild the rules collection
            if (draftRules.Any())
            {
                foreach (var rpr in source)
                {
                    var match = draftRules.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (rpr.MasterUUID ?? rpr.UUID));
                    if (match == null)
                    {
                        newRules.Add(rpr);
                    }
                    else
                    {
                        newRules.Add(match.ResultPromptRule);
                    }
                }

                return newRules;
            }
            else
            {
                return source;
            }            
        }

        private List<ResultDefinition> ResetResultsToDraft(List<ResultDefinition> source)
        {
            var newResults = new List<ResultDefinition>();
            if (source == null) { return newResults; }

            var draftResults = (from dr in treeModel.AllResultDefinitionsViewModel.Definitions
                             join rd in source on dr.MasterUUID ?? dr.UUID equals rd.MasterUUID ?? rd.UUID
                             where dr.PublishedStatus == PublishedStatus.Draft
                             select dr).ToList();

            //rebuild the results collection
            if (draftResults.Any())
            {
                foreach (var rd in source)
                {
                    var match = draftResults.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (rd.MasterUUID ?? rd.UUID));
                    if (match == null)
                    {
                        newResults.Add(rd);
                    }
                    else
                    {
                        newResults.Add(match.ResultDefinition);
                    }
                }
                return newResults;
            }
            else
            {
                return source;
            }            
        }

        private List<Now> ResetNowsToDraft(List<Now> source)
        {
            var newNows = new List<Now>();
            if (source == null) { return newNows; }

            var draftNOWs = (from dn in treeModel.AllNowsViewModel.Nows
                                     join now in source on dn.MasterUUID ?? dn.UUID equals now.MasterUUID ?? now.UUID
                                     where dn.PublishedStatus == PublishedStatus.Draft
                                     select dn).ToList();

            //rebuild the nows collection
            if (draftNOWs.Any())
            {
                foreach (var now in source)
                {
                    var match = draftNOWs.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (now.MasterUUID ?? now.UUID));
                    if (match == null)
                    {
                        newNows.Add(now);
                    }
                    else
                    {
                        newNows.Add(match.Now);
                    }
                }

                return newNows;
            }
            else
            {
                return source;
            }            
        }

        internal void SelectedFocusReceived()
        {
            OnPropertyChanged("ExcludedPrompts");
            OnPropertyChanged("HasExcludedPrompts");
            OnPropertyChanged("HasExcludedPromptsContextActions");
            OnPropertyChanged("IncludedPrompts");
            OnPropertyChanged("HasIncludedPrompts");
            OnPropertyChanged("HasIncludedPromptsContextActions");
        }

        #endregion Copy Members


        public void ResetStatus()
        {
            OnPropertyChanged("IsNowSubscriptionPublishedPending");
        }
        public bool IsNowSubscriptionPublishedPending
        {
            get
            {
                if (IsDeleted) { return false; }
                return IsPublishedPending;
            }
        }
    }

    public class SelectedCourtHouse
    {
        public string Code { get; set; }
        public string CourtHouse { get; set; }
    }
}