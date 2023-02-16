using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;
using DataLib.DataModel;
using Models.Commands;
using DataLib;
using System.Linq;
using Serilog;
using Identity;
using System.Text;
using System.Security.Cryptography;
using Configuration;

namespace Models.ViewModels
{
    public interface ITreeModel
    {
        ICommand AddNewChildResultDefinitionCommand { get; }
        ICommand AddFixedListCommand { get; }
        ICommand AddNewChildNowSubscriptionCommand { get; }
        ICommand AddNewEDTCommand { get; }
        ICommand AddNewNowCommand { get; }
        ICommand AddNewNowSubscriptionCommand { get; }
        ICommand AddNowCommand { get; }
        ICommand AddNowRequirementCommand { get; }
        ICommand AddNowRequirementTextValueCommand { get; }
        ICommand AddNowTextValueCommand { get; }
        bool AddPasteRDWGVisibility { get; }
        bool AddPromptRuleUserGroupEnabled { get; }
        ICommand AddNewResultPromptWordGroupCommand { get; }
        ICommand AddNewResultDefinitionCommand { get; }
        bool AddResultDefinitionUserGroupEnabled { get; }
        ICommand AddNewResultPromptCommand { get; }
        ICommand AddUserGroupCommand { get; }
        ICommand AddNewResultDefinitionWordGroupCommand { get; }
        AllCourtRegisterSubscriptionsTreeViewModel AllCourtRegisterSubscriptionsTreeViewModel { get; }
        AllCourtRegisterSubscriptionViewModel AllCourtRegisterSubscriptionViewModel { get; }
        AllData AllData { get; set; }
        IdMap Map { get; set; }
        AllEDTsTreeViewModel AllEDTsTreeViewModel { get; }
        AllEDTSubscriptionsTreeViewModel AllEDTSubscriptionsTreeViewModel { get; }
        AllEDTSubscriptionViewModel AllEDTSubscriptionViewModel { get; }
        AllEDTsViewModel AllEDTsViewModel { get; }
        AllFixedListViewModel AllFixedListViewModel { get; }
        AllInformantRegisterSubscriptionsTreeViewModel AllInformantRegisterSubscriptionsTreeViewModel { get; }
        AllInformantRegisterSubscriptionViewModel AllInformantRegisterSubscriptionViewModel { get; }
        AllNowRequirementsViewModel AllNowRequirementsViewModel { get; }
        AllNowsTreeViewModel AllNowsTreeViewModel { get; }
        AllNowSubscriptionsTreeViewModel AllNowSubscriptionsTreeViewModel { get; }
        AllNowSubscriptionViewModel AllNowSubscriptionViewModel { get; }
        AllNowsViewModel AllNowsViewModel { get; }
        AllPrisonCourtRegisterSubscriptionsTreeViewModel AllPrisonCourtRegisterSubscriptionsTreeViewModel { get; }
        AllPrisonCourtRegisterSubscriptionViewModel AllPrisonCourtRegisterSubscriptionViewModel { get; }
        SilentObservableCollection<string> AllPublicationTags { get; }
        AllResultDefinitionsTreeViewModel AllResultDefinitionsTreeViewModel { get; }
        AllResultDefinitionsViewModel AllResultDefinitionsViewModel { get; }
        AllResultDefinitionWordGroupViewModel AllResultDefinitionWordGroupViewModel { get; }
        AllResultPromptViewModel AllResultPromptViewModel { get; }
        AllResultPromptWordGroupViewModel AllResultPromptWordGroupViewModel { get; }
        AllResultRuleViewModel AllResultRuleViewModel { get; }
        List<ComboBoxItemString> AvailableFeatureToggles { get; set; }
        List<ComboBoxItemString> CachableOptions { get; }
        string CheckedOutWarning { get; }
        bool CheckInVisibility { get; }
        User CheckOutUser { get; set; }
        TreeViewItemViewModel CopiedTreeViewModel { get; set; }
        ICommand CopyEDTCommand { get; }
        ICommand CopyFixedListCommand { get; }
        ICommand CopyNowCommand { get; }
        ICommand CopyResultPromptWordGroupCommand { get; }
        ICommand CopyResultDefinitionCommand { get; }
        ICommand CopyResultPromptCommand { get; }
        ICommand CopyUserGroupCommand { get; }
        ICommand CopyResultDefinitionWordGroupCommand { get; }
        List<ComboBoxItemString> CourtHouses { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> CourtRegisterSubscriptions { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> CourtRegisterSubscriptionsDraft { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> CourtRegisterSubscriptionsPublished { get; }
        SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> CourtRegisterSubscriptionsRevision { get; }
        ICommand DataPatchByTagCommand { get; }
        ICommand DeleteRevisionsCommand { get; }
        ICommand DeleteEDTCommand { get; }
        ICommand SetDeleteEDTCommand { get; }
        ICommand DeleteFixedListCommand { get; }
        ICommand DeleteNowCommand { get; }
        ICommand SetDeleteNowCommand { get; }
        ICommand DeleteNowRequirementCommand { get; }
        ICommand DeleteNowRequirementTextValueCommand { get; }
        ICommand DeleteNowSubscriptionCommand { get; }
        ICommand SetDeleteNowSubscriptionCommand { get; }
        ICommand DeleteNowTextValueCommand { get; }
        ICommand DeleteResultPromptWordGroupCommand { get; }
        ICommand DeleteResultDefinitionCommand { get; }
        ICommand SetDeleteResultDefinitionCommand { get; }
        ICommand DeleteResultPromptCommand { get; }
        ICommand DeleteUserGroupCommand { get; }
        ICommand DeleteResultDefinitionWordGroupCommand { get; }
        ICommand DraftFixedListCommand { get; }
        ICommand DraftNowCommand { get; }
        ICommand DraftNowSubscriptionCommand { get; }
        ICommand DraftResultDefinitionCommand { get; }
        ICommand DraftResultDefinitionWordGroupCommand { get; }
        ICommand DraftResultPromptCommand { get; }
        ICommand DraftResultPromptWordGroupCommand { get; }
        ICommand EditUserGroupCommand { get; }
        bool EDTPasteVisibility { get; }
        bool EDTPublishVisibility { get; }
        SilentObservableCollection<NowTreeViewModel> EDTs { get; }
        SilentObservableCollection<NowTreeViewModel> EdtsDraft { get; }
        SilentObservableCollection<NowTreeViewModel> EdtsPublished { get; }
        SilentObservableCollection<NowRevisionTreeViewModel> EdtsRevision { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptions { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptionsDraft { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptionsPublished { get; }
        SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> EDTSubscriptionsRevision { get; }
        bool FixedListDeleteVisibility { get; }
        bool FixedListDraftVisibility { get; }
        bool FixedListPasteVisibility { get; }
        bool FixedListUndeleteVisibility { get; }
        ICommand GenerateReportCommand { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> InformantRegisterSubscriptions { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> InformantRegisterSubscriptionsDraft { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> InformantRegisterSubscriptionsPublished { get; }
        SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> InformantRegisterSubscriptionsRevision { get; }
        List<ComboBoxItemString> Informants { get; }
        bool IsLocalChangeMode { get; set; }
        bool IsReadOnly { get; set; }
        bool IsTest { get; }
        bool IsNotTest { get; }
        List<ComboBoxItemString> Jurisdictions { get; }
        string LoggedOnUserText { get; }
        List<ComboBoxItemString> NameAddressTypes { get; }
        bool NowPasteVisibility { get; }
        bool NOWPublishVisibility { get; }
        bool NowRequirementPasteVisibility { get; }
        SilentObservableCollection<NowTreeViewModel> Nows { get; }
        SilentObservableCollection<NowTreeViewModel> NowsDraft { get; }
        SilentObservableCollection<NowTreeViewModel> NowsPublished { get; }
        SilentObservableCollection<NowRevisionTreeViewModel> NowsRevision { get; }
        bool NOWSubscriptionPublishVisibility { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> NowSubscriptions { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> NowSubscriptionsDraft { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> NowSubscriptionsPublished { get; }
        SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> NowSubscriptionsRevision { get; }
        ICommand PasteFixedListCommand { get; }
        ICommand PasteIncludedExcludedSubscriptionCommand { get; }
        ICommand PasteResultPromptWordGroupCommand { get; }
        ICommand PasteResultDefinitionChildCommand { get; }
        ICommand PasteResultDefinitionCommand { get; }
        ICommand PasteResultPromptCommand { get; }
        ICommand PasteUserGroupCommand { get; }
        ICommand PasteResultDefinitionWordGroupCommand { get; }
        List<ComboBoxItemString> PersonTitles { get; }
        List<ComboBoxItemString> PostHearingCustodyStatus { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> PrisonCourtRegisterSubscriptions { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> PrisonCourtRegisterSubscriptionsDraft { get; }
        SilentObservableCollection<NowSubscriptionTreeViewModel> PrisonCourtRegisterSubscriptionsPublished { get; }
        SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> PrisonCourtRegisterSubscriptionsRevision { get; }
        ICommand PublishDraftsCommand { get; }
        bool RemovePromptRuleUserGroupEnabled { get; }
        bool RemoveResultDefinitionUserGroupEnabled { get; }
        ICommand ResequenceCommand { get; }
        bool RevisionsDeleteVisibility { get; }
        List<ComboBoxItemString> ResultDefinitionCategories { get; }
        bool ResultDefinitionChildPasteVisibility { get; }
        bool ResultDefinitionDeleteVisibility { get; }
        bool SetResultDefinitionDeleteVisibility { get; }
        bool ResultDefinitionDraftVisibility { get; }
        List<ComboBoxItemString> ResultDefinitionLevels { get; }
        bool ResultDefinitionPasteVisibility { get; }
        bool ResultDefinitionPublishVisibility { get; }
        SilentObservableCollection<ResultDefinitionTreeViewModel> ResultDefinitionRootsDraft { get; }
        SilentObservableCollection<ResultDefinitionTreeViewModel> ResultDefinitionRootsPublished { get; }
        SilentObservableCollection<ResultDefinitionRevisionTreeViewModel> ResultDefinitionRevision { get; }
        bool ResultDefinitionUndeleteVisibility { get; }
        bool ResultDefinitionWordGroupDraftVisibility { get; }
        bool ResultDefinitionWordGroupPasteVisibility { get; }
        bool ResultPromptDeleteVisibility { get; }
        bool ResultPromptDraftChildActionsVisibility { get; }
        bool ResultPromptDraftVisibility { get; }
        bool ResultPromptPasteVisibility { get; }
        List<ComboBoxItemString> ResultPromptTypes { get; }
        bool ResultPromptUndeleteVisibility { get; }
        bool ResultPromptWordGroupDeleteVisibility { get; }
        bool ResultPromptWordGroupDraftVisibility { get; }
        bool ResultPromptWordGroupPasteVisibility { get; }
        bool ResultPromptWordGroupUndeleteVisibility { get; }
        ICommand SampleNowCommand { get; }
        string SaveErrors { get; set; }
        ICommand SaveToFileCommand { get; }
        bool SaveVisibility { get; }
        IValidateCommand ValidateCommand { get; }
        ICommand SearchItemCommand { get; }
        string SearchText { get; set; }
        string SelectedAvailablePromptRuleUserGroup { get; set; }
        string SelectedAvailableResultDefinitionUserGroup { get; set; }
        TreeViewItemViewModel SelectedEditTreeViewModel { get; set; }
        bool SelectedFixedListVisibility { get; }
        int SelectedIndex { get; set; }
        TreeViewItemViewModel SelectedItem { get; set; }
        bool SelectedNowRequirementResultPromptVisibility { get; }
        bool SelectedNowRequirementVisibility { get; }
        bool SelectedNowSubscriptionVisibility { get; }
        bool SelectedNowVisibility { get; }
        string SelectedPromptRuleUserGroup { get; set; }
        bool SelectedResultDefinitionRuleVisibility { get; }
        string SelectedResultDefinitionUserGroup { get; set; }
        bool SelectedResultDefinitionVisibility { get; }
        bool SelectedResultDefinitionWordGroupVisibility { get; }
        bool SelectedResultPromptVisibility { get; }
        bool SelectedResultPromptWordGroupVisibility { get; }
        ICommand SetSubscriptionFromResultCommand { get; }
        List<ComboBoxItemString> StipulatedDrivingTestOptions { get; }
        ICommand UndeleteFixedListCommand { get; }
        ICommand UndeleteResultDefinitionCommand { get; }
        ICommand UndeleteResultPromptCommand { get; }
        ICommand UndeleteResultPromptWordGroupCommand { get; }
        List<string> UserGroups { get; }
        List<ComboBoxItemString> UserGroupsAsCombo { get; }
        string VersionText { get; }
        List<ComboBoxItemString> Yots { get; }

        ICommand CascadeNowTextCommand { get; }

        void RefreshResults();
        void ResetDraftVisibility();
        void SetRootResultDeleted();
    }

    public sealed class TreeModel : ViewModelBase, ITreeModel
    {
        private AllResultRuleViewModel allResultRuleViewModel = null;
        private AllResultDefinitionsViewModel allResultDefinitionsViewModel;
        private AllResultPromptViewModel allResultPromptViewModel;
        private AllResultDefinitionWordGroupViewModel allResultDefinitionWordGroupViewModel;
        private AllResultPromptWordGroupViewModel allResultPromptWordGroupViewModel;
        private AllNowsViewModel allNowsViewModel;
        private AllEDTsViewModel allEDTsViewModel;
        private AllNowSubscriptionViewModel allNowSubscriptionsViewModel;
        private AllEDTSubscriptionViewModel allEDTSubscriptionsViewModel;
        private AllInformantRegisterSubscriptionViewModel allInformantRegisterSubscriptionsViewModel;
        private AllCourtRegisterSubscriptionViewModel allCourtRegisterSubscriptionsViewModel;
        private AllPrisonCourtRegisterSubscriptionViewModel allPrisonCourtRegisterSubscriptionsViewModel;
        private AllNowRequirementsViewModel allNowRequirementsViewModel;
        private AllFixedListViewModel allFixedListViewModel;

        string searchText = null;

        private AllResultDefinitionsTreeViewModel allResultDefinitionsTreeViewModel;
        private AllNowsTreeViewModel allNowsTreeViewModel;
        private AllEDTsTreeViewModel allEDTsTreeViewModel;
        private AllNowSubscriptionsTreeViewModel allNowSubscriptionsTreeViewModel;
        private AllEDTSubscriptionsTreeViewModel allEDTSubscriptionsTreeViewModel;
        private AllInformantRegisterSubscriptionsTreeViewModel allInformantRegisterSubscriptionsTreeViewModel;
        private AllPrisonCourtRegisterSubscriptionsTreeViewModel allPrisonCourtRegisterSubscriptionsTreeViewModel;
        private AllCourtRegisterSubscriptionsTreeViewModel allCourtRegisterSubscriptionsTreeViewModel;

        private IAppSettings appSettings;

        private static readonly string cryptoKey = "YP1afDJA";
        private static readonly string initVector = "8O14lnfc";

        public TreeModel(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
            treeModel = this;

            //Initialise data
            LoadDataFromFile();

            //initiate the commands
            AddFixedListCommand = new AddFixedListCommand(this);
            AddNewChildNowSubscriptionCommand = new AddNewChildNowSubscriptionCommand(this);
            AddNewChildResultDefinitionCommand = new AddNewChildResultDefinitionCommand(this);
            AddNewEDTCommand = new AddNewEDTCommand(this);
            AddNewNowCommand = new AddNewNowCommand(this);
            AddNewNowSubscriptionCommand = new AddNewNowSubscriptionCommand(this);
            AddNewResultDefinitionCommand = new AddNewResultDefinitionCommand(this);
            AddNewResultDefinitionWordGroupCommand = new AddNewResultDefinitionWordGroupCommand(this);
            AddNewResultPromptCommand = new AddNewResultPromptCommand(this);
            AddNewResultPromptWordGroupCommand = new AddNewResultPromptWordGroupCommand(this);
            AddNowRequirementTextValueCommand = new AddNowRequirementTextValueCommand(this);
            AddNowTextValueCommand = new AddNowTextValueCommand(this);
            CopyEDTCommand = new CopyEDTCommand(this);
            CopyFixedListCommand = new CopyFixedListCommand(this);
            CopyNowCommand = new CopyNowCommand(this);
            CopyResultDefinitionCommand = new CopyResultDefinitionCommand(this);
            CopyResultDefinitionWordGroupCommand = new CopyResultDefinitionWordGroupCommand(this);
            CopyResultPromptCommand = new CopyResultPromptCommand(this);
            CopyResultPromptWordGroupCommand = new CopyResultPromptWordGroupCommand(this);
            DataPatchByTagCommand = new DataPatchByTagCommand(this);
            DeleteEDTCommand = new DeleteEDTCommand(this);
            SetDeleteEDTCommand = new SetDeleteEDTCommand(this);
            DeleteFixedListCommand = new DeleteFixedListCommand(this);
            DeleteNowCommand = new DeleteNowCommand(this);
            SetDeleteNowCommand = new SetDeleteNowCommand(this);
            DeleteNowRequirementCommand = new DeleteNowRequirementCommand(this);
            DeleteNowRequirementTextValueCommand = new DeleteNowRequirementTextValueCommand(this);
            DeleteNowSubscriptionCommand = new DeleteNowSubscriptionCommand(this);
            SetDeleteNowSubscriptionCommand = new SetDeleteNowSubscriptionCommand(this);
            DeleteNowTextValueCommand = new DeleteNowTextValueCommand(this);
            DeleteResultDefinitionCommand = new DeleteResultDefinitionCommand(this);
            SetDeleteResultDefinitionCommand = new SetDeleteResultDefinitionCommand(this);
            DeleteResultDefinitionWordGroupCommand = new DeleteResultDefinitionWordGroupCommand(this);
            DeleteResultPromptCommand = new DeleteResultPromptCommand(this);
            DeleteResultPromptWordGroupCommand = new DeleteResultPromptWordGroupCommand(this);
            DraftFixedListCommand = new DraftFixedListCommand(this);
            DraftNowCommand = new DraftNowCommand(this);
            DraftNowSubscriptionCommand = new DraftNowSubscriptionCommand(this);
            DraftResultDefinitionCommand = new DraftResultDefinitionCommand(this);
            DraftResultDefinitionWordGroupCommand = new DraftResultDefinitionWordGroupCommand(this);
            DraftResultPromptCommand = new DraftResultPromptCommand(this);
            DraftResultPromptWordGroupCommand = new DraftResultPromptWordGroupCommand(this);
            SaveToFileCommand = new SaveToFileCommand(this);
            ValidateCommand = new ValidateCommand(this);
            GenerateReportCommand = new GenerateReportCommand(this, appSettings);
            PasteFixedListCommand = new PasteFixedListCommand(this);
            PasteIncludedExcludedSubscriptionCommand = new PasteIncludedExcludedSubscriptionCommand(this);
            PasteResultDefinitionChildCommand = new PasteResultDefinitionChildCommand(this);
            PasteResultDefinitionCommand = new PasteResultDefinitionCommand(this);
            PasteResultDefinitionWordGroupCommand = new PasteResultDefinitionWordGroupCommand(this);
            PasteResultPromptCommand = new PasteResultPromptCommand(this);
            PasteResultPromptWordGroupCommand = new PasteResultPromptWordGroupCommand(this);
            PublishDraftsCommand = new PublishDraftsCommand(this);
            ResequenceCommand = new ResequenceCommand(this);
            SampleNowCommand = new SampleNowCommand(appSettings, this);
            SearchItemCommand = new SearchItemCommand(this);
            SetSubscriptionFromResultCommand = new SetSubscriptionFromResultCommand(this);
            UndeleteFixedListCommand = new UndeleteFixedListCommand(this);
            UndeleteResultDefinitionCommand = new UndeleteResultDefinitionCommand(this);
            UndeleteResultPromptCommand = new UndeleteResultPromptCommand(this);
            UndeleteResultPromptWordGroupCommand = new UndeleteResultPromptWordGroupCommand(this);
            CascadeNowTextCommand = new CascadeNowTextCommand(this);
            DeleteRevisionsCommand = new DeleteRevisionsCommand(this);

            //custom fixes
            ApplyDataFixes();
        }

        private void LoadDataFromFile()
        {
            // Look For Saved Data
            var filePath = appSettings.DataFilePath;
            if (!File.Exists(filePath))
            {
                Log.Information("No data is available");
                return;
            }

            try
            {
                // Deserialize the data from the file 
                AllData = DecryptAndDeserialize(filePath);
                InitialiseReferenceData();
                InitialiseLists();
            }
            catch (IOException e)
            {
                Log.Error(e, "Failed to read data file. Reason: " + e.Message);
                throw;
            }
            catch (SerializationException e)
            {
                Log.Error(e, "Failed to deserialize Data file. Reason: " + e.Message);
                throw;
            }
        }

        public static AllData DecryptAndDeserialize(string filename)
        {
            var formatter = new BinaryFormatter();
            var algorithm = GetSymmetricAlgorithm();

            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                using (CryptoStream cs = new CryptoStream(fs, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    return (AllData)formatter.Deserialize(cs);
                }
            }
        }

        private static SymmetricAlgorithm GetSymmetricAlgorithm()
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = ASCIIEncoding.ASCII.GetBytes(cryptoKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(initVector);

            return des;
        }

        public static void EncryptAndSerialize(string filename, AllData data)
        {
            var formatter = new BinaryFormatter();
            var algorithm = GetSymmetricAlgorithm();

            using (FileStream fs = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cs = new CryptoStream(fs, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    formatter.Serialize(cs, data);
                }
            }
        }

        private void ApplyDataFixes()
        {
            ////fix now requirements that are publishPending so that they have rd references to either published pending or published
            //foreach (var item in AllData.NowRequirements.Where(x => x.CalculatedPublishedStatus == PublishedStatus.PublishedPending))
            //{
            //    if (item.ResultDefinition.CalculatedPublishedStatus == PublishedStatus.RevisionPending)
            //    {
            //        var replacementRd = AllData.ResultDefinitions.FirstOrDefault(x => x.CalculatedPublishedStatus == PublishedStatus.PublishedPending &&
            //                                                             (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
            //        if (replacementRd == null)
            //        {
            //            replacementRd = AllData.ResultDefinitions.First(x => x.CalculatedPublishedStatus == PublishedStatus.Published &&
            //                                                             (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
            //        }
            //        item.ResultDefinition = replacementRd;

            //        //reset each now requirement prompt rule
            //        if (item.NowRequirementPromptRules != null)
            //        {
            //            foreach (var rule in item.NowRequirementPromptRules)
            //            {
            //                var matchedRule = replacementRd.ResultPrompts.First(x => (x.ResultPrompt.MasterUUID ?? x.ResultPrompt.UUID) == (rule.ResultPromptRule.ResultPrompt.MasterUUID ?? rule.ResultPromptRule.ResultPrompt.UUID));
            //                rule.ResultPromptRule = matchedRule;
            //            }
            //        }
            //    }
            //}

            ////fix now requirements that are revisionPending so that they have rd references to either revision pending or published
            //foreach (var item in AllData.NowRequirements.Where(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending))
            //{
            //    if (item.ResultDefinition.CalculatedPublishedStatus != PublishedStatus.RevisionPending)
            //    {
            //        var replacementRd = AllData.ResultDefinitions.FirstOrDefault(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending &&
            //                                                             (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
            //        if (replacementRd == null)
            //        {
            //            replacementRd = AllData.ResultDefinitions.First(x => x.CalculatedPublishedStatus == PublishedStatus.Published &&
            //                                                             (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
            //        }
            //        if (item.ResultDefinition.UUID != replacementRd.UUID)
            //        {
            //            item.ResultDefinition = replacementRd;

            //            //reset each now requirement prompt rule
            //            if (item.NowRequirementPromptRules != null)
            //            {
            //                foreach (var rule in item.NowRequirementPromptRules)
            //                {
            //                    var matchedRule = replacementRd.ResultPrompts.First(x => (x.ResultPrompt.MasterUUID ?? x.ResultPrompt.UUID) == (rule.ResultPromptRule.ResultPrompt.MasterUUID ?? rule.ResultPromptRule.ResultPrompt.UUID));
            //                    rule.ResultPromptRule = matchedRule;
            //                }
            //            }
            //        }
            //    }
            //}

            ////ensure that Published/PublishPending Now Subscriptions do not have references to revision pending prompt rules
            //foreach (var item in AllData.NowSubscriptions.Where(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending ||
            //                                                                      x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                                      x.SubscriptionVocabulary != null &&
            //                                                                    ((x.SubscriptionVocabulary.IncludedPromptRules != null && x.SubscriptionVocabulary.IncludedPromptRules.Any()) ||
            //                                                                     (x.SubscriptionVocabulary.ExcludedPromptRules != null && x.SubscriptionVocabulary.ExcludedPromptRules.Any())
            //                                                                    )
            //                                                                   )
            //                                                              )
            //{
            //    if (item.SubscriptionVocabulary.IncludedPromptRules != null && item.SubscriptionVocabulary.IncludedPromptRules.Any())
            //    {
            //        var newIncludedPromptRules = new List<ResultPromptRule>();
            //        foreach (var rule in item.SubscriptionVocabulary.IncludedPromptRules)
            //        {
            //            if (rule.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rule.CalculatedPublishedStatus == PublishedStatus.Published)
            //            {
            //                newIncludedPromptRules.Add(rule);
            //            }
            //            else
            //            {
            //                var replacementRule = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                             (x.MasterUUID ?? x.UUID) == (rule.MasterUUID ?? rule.UUID));
            //                if (replacementRule != null)
            //                {
            //                    newIncludedPromptRules.Add(replacementRule);
            //                }
            //            }
            //        }
            //        item.SubscriptionVocabulary.IncludedPromptRules = newIncludedPromptRules;
            //    }

            //    if (item.SubscriptionVocabulary.ExcludedPromptRules != null && item.SubscriptionVocabulary.ExcludedPromptRules.Any())
            //    {
            //        var newExcludedPromptRules = new List<ResultPromptRule>();
            //        foreach (var rule in item.SubscriptionVocabulary.ExcludedPromptRules)
            //        {
            //            if (rule.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rule.CalculatedPublishedStatus == PublishedStatus.Published)
            //            {
            //                newExcludedPromptRules.Add(rule);
            //            }
            //            else
            //            {
            //                var replacementRule = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                             (x.MasterUUID ?? x.UUID) == (rule.MasterUUID ?? rule.UUID));
            //                if (replacementRule != null)
            //                {
            //                    newExcludedPromptRules.Add(replacementRule);
            //                }
            //            }
            //        }
            //        item.SubscriptionVocabulary.ExcludedPromptRules = newExcludedPromptRules;
            //    }
            //}

            ////ensure that Published/PublishPending Now Subscriptions do not have references to revision pending results
            //foreach (var item in treeModel.AllData.NowSubscriptions.Where(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending ||
            //                                                                      x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                                      x.SubscriptionVocabulary != null &&
            //                                                                    ((x.SubscriptionVocabulary.IncludedResults != null && x.SubscriptionVocabulary.IncludedResults.Any()) ||
            //                                                                     (x.SubscriptionVocabulary.ExcludedResults != null && x.SubscriptionVocabulary.ExcludedResults.Any())
            //                                                                    )
            //                                                                   )
            //                                                              )
            //{
            //    if (item.SubscriptionVocabulary.IncludedResults != null && item.SubscriptionVocabulary.IncludedResults.Any())
            //    {
            //        var newIncludedResults = new List<ResultDefinition>();
            //        foreach (var rd in item.SubscriptionVocabulary.IncludedResults)
            //        {
            //            if (rd.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rd.CalculatedPublishedStatus == PublishedStatus.Published)
            //            {
            //                newIncludedResults.Add(rd);
            //            }
            //            else
            //            {
            //                var replacementRd = AllData.ResultDefinitions.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                             (x.MasterUUID ?? x.UUID) == (rd.MasterUUID ?? rd.UUID));
            //                if (replacementRd != null)
            //                {
            //                    newIncludedResults.Add(replacementRd);
            //                }
            //            }
            //        }
            //        item.SubscriptionVocabulary.IncludedResults = newIncludedResults;
            //    }

            //    if (item.SubscriptionVocabulary.ExcludedResults != null && item.SubscriptionVocabulary.ExcludedResults.Any())
            //    {
            //        var newExcludedResults = new List<ResultDefinition>();
            //        foreach (var rd in item.SubscriptionVocabulary.ExcludedResults)
            //        {
            //            if (rd.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rd.CalculatedPublishedStatus == PublishedStatus.Published)
            //            {
            //                newExcludedResults.Add(rd);
            //            }
            //            else
            //            {
            //                var replacementRd = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                             (x.MasterUUID ?? x.UUID) == (rd.MasterUUID ?? rd.UUID));
            //                if (replacementRd != null)
            //                {
            //                    newExcludedResults.Add(replacementRd);
            //                }
            //            }
            //        }
            //        item.SubscriptionVocabulary.ExcludedResults = newExcludedResults;
            //    }
            //}

            ////ensure that Published/PublishPending Now Subscriptions do not have references to revision pending NOWs
            //foreach (var item in treeModel.AllData.NowSubscriptions.Where(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending ||
            //                                                                      x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                                    ((x.IncludedNOWS != null && x.IncludedNOWS.Any()) ||
            //                                                                     (x.ExcludedNOWS != null && x.ExcludedNOWS.Any())
            //                                                                    )
            //                                                                   )
            //                                                              )
            //{
            //    if (item.IncludedNOWS != null && item.IncludedNOWS.Any())
            //    {
            //        var newIncludedNows = new List<Now>();
            //        foreach (var now in item.IncludedNOWS)
            //        {
            //            if (now.CalculatedPublishedStatus == PublishedStatus.PublishedPending || now.CalculatedPublishedStatus == PublishedStatus.Published)
            //            {
            //                newIncludedNows.Add(now);
            //            }
            //            else
            //            {
            //                var replacementNow = treeModel.AllData.Nows.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                             (x.MasterUUID ?? x.UUID) == (now.MasterUUID ?? now.UUID));
            //                if (replacementNow != null)
            //                {
            //                    newIncludedNows.Add(replacementNow);
            //                }
            //            }
            //        }
            //        item.IncludedNOWS = newIncludedNows;
            //    }

            //    if (item.ExcludedNOWS != null && item.ExcludedNOWS.Any())
            //    {
            //        var newExcludedNows = new List<Now>();
            //        foreach (var now in item.ExcludedNOWS)
            //        {
            //            if (now.CalculatedPublishedStatus == PublishedStatus.PublishedPending || now.CalculatedPublishedStatus == PublishedStatus.Published)
            //            {
            //                newExcludedNows.Add(now);
            //            }
            //            else
            //            {
            //                var replacementNow = treeModel.AllData.Nows.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
            //                                                             (x.MasterUUID ?? x.UUID) == (now.MasterUUID ?? now.UUID));
            //                if (replacementNow != null)
            //                {
            //                    newExcludedNows.Add(replacementNow);
            //                }
            //            }
            //        }
            //        item.ExcludedNOWS = newExcludedNows;
            //    }
            //}

            //var rdRevisions = AllData.ResultDefinitions.Where(x => x.CalculatedPublishedStatus == PublishedStatus.Revision || x.CalculatedPublishedStatus == PublishedStatus.RevisionPending).ToList();
            //var nowRevisions = AllData.Nows.Where(x => x.CalculatedPublishedStatus == PublishedStatus.Revision || x.CalculatedPublishedStatus == PublishedStatus.RevisionPending).ToList();
            //var rdPublishedPending = AllData.ResultDefinitions.Where(x => x.CalculatedPublishedStatus == PublishedStatus.PublishedPending).ToList();
            //var rdDraft = AllData.ResultDefinitions.Where(x => x.CalculatedPublishedStatus == PublishedStatus.Draft).ToList();
            //var nowDraft = AllData.Nows.Where(x => x.CalculatedPublishedStatus == PublishedStatus.Draft).ToList();
            //var nowPublishedPending = AllData.Nows.Where(x => x.CalculatedPublishedStatus == PublishedStatus.PublishedPending).ToList();
            //var rep = string.Format("Number of Result Definition revisions = {0}{1}Number of new Result Definitions = {2}{1}" +
            //    "Number of NOW/EDT revisions = {3}{1}Number of new NOWs/EDTs = {4}{1}Number of Result Definition drafts = {5}{1}Number of NOWs/EDTs drafts = {6}",
            //    rdRevisions.Count(), Environment.NewLine, rdPublishedPending.Count() - rdRevisions.Count(), nowRevisions.Count(), nowPublishedPending.Count() - nowRevisions.Count(),
            //    rdDraft.Count(), nowDraft.Count());
            //var one = 1;

            ////fix result definition rules and result prompt rules
            //var publishedRds = AllData.ResultDefinitions.Where(x => (x.CalculatedPublishedStatus == PublishedStatus.Published || x.CalculatedPublishedStatus == PublishedStatus.PublishedPending)).ToList();
            //foreach(var rd in publishedRds)
            //{
            //    if (rd.ResultDefinitionRules != null)
            //    {
            //        foreach (var rdr in rd.ResultDefinitionRules)
            //        {
            //            if (rdr.CalculatedPublishedStatus != rd.CalculatedPublishedStatus)
            //            {
            //                rdr.PublishedStatus = rd.PublishedStatus;
            //                rdr.PublishedStatusDate = rd.PublishedStatusDate;
            //                if (rdr.PublishedStatus == null)
            //                {
            //                    rdr.PublicationTags = null;
            //                    rdr.MasterUUID = null;
            //                }
            //            }
            //        }
            //    }

            //    if (rd.ResultPrompts != null)
            //    {
            //        foreach (var rpr in rd.ResultPrompts)
            //        {
            //            if (rpr.CalculatedPublishedStatus != rd.CalculatedPublishedStatus)
            //            {
            //                rpr.PublishedStatus = rd.PublishedStatus;
            //                rpr.PublishedStatusDate = rd.PublishedStatusDate;
            //                if (rpr.PublishedStatus == null)
            //                {
            //                    rpr.PublicationTags = null;
            //                    rpr.MasterUUID = null;
            //                }
            //            }
            //        }
            //    }
            //}

            ////compare revision pending to publish pending Nows to ensure that revisions are required
            //var revisionNows = AllData.Nows.Where(x => x.PublishedStatus != null && x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
            //var toPurge = new List<Tuple<Now,Now>>();
            //var nowCount = 0;
            //var staticId = Guid.NewGuid();
            //foreach (var rNow in revisionNows)
            //{
            //    if (rNow.Name == "Notice that Bail has been Refused")
            //    {
            //        var stop = 1;
            //    }
            //    nowCount++;
            //    var pNow = AllData.Nows.First(x => (x.MasterUUID ?? x.UUID) == (rNow.MasterUUID ?? rNow.UUID) && x.PublishedStatus != null && x.PublishedStatus == PublishedStatus.PublishedPending);

            //    if (pNow != rNow)
            //    {
            //        continue;
            //    }
            //    ////look for equality in the now requirements
            //    //if(rNow.AllNowRequirements.Count() != pNow.AllNowRequirements.Count())
            //    //{
            //    //    continue;
            //    //}
            //    var nrsEqual = true;                
            //    foreach(var rNr in rNow.AllNowRequirements)
            //    {
            //        var pNr = pNow.AllNowRequirements.FirstOrDefault(x => 
            //                                                    (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) == (rNr.ResultDefinition.MasterUUID ?? rNr.ResultDefinition.UUID) //results equal
            //                                                    &&
            //                                                    (x.ParentNowRequirement == null ? staticId : (x.ParentNowRequirement.ResultDefinition.MasterUUID ?? x.ParentNowRequirement.ResultDefinition.UUID)) == (rNr.ParentNowRequirement == null ? staticId : (rNr.ParentNowRequirement.ResultDefinition.MasterUUID ?? rNr.ParentNowRequirement.ResultDefinition.UUID)) // same parent NR
            //                                                    &&
            //                                                    (x.RootParentNowRequirement == null ? staticId : (x.RootParentNowRequirement.ResultDefinition.MasterUUID ?? x.ParentNowRequirement.ResultDefinition.UUID)) == (rNr.RootParentNowRequirement == null ? staticId : (rNr.RootParentNowRequirement.ResultDefinition.MasterUUID ?? rNr.RootParentNowRequirement.ResultDefinition.UUID)) // same root parent NR
            //                                                    );
            //        if (pNr == null)
            //        {
            //            nrsEqual = false;
            //            break;
            //        }
            //        if (pNr != rNr)
            //        {
            //            nrsEqual = false;
            //            break;
            //        }

            //        var rRprs = rNr.ResultDefinition.ResultPrompts ?? new List<ResultPromptRule>();
            //        var pRprs = pNr.ResultDefinition.ResultPrompts ?? new List<ResultPromptRule>();
            //        foreach (var rRpr in rRprs)
            //        {
            //            var pRpr = pRprs.FirstOrDefault(x => (x.MasterUUID ?? x.UUID) == (rRpr.MasterUUID ?? rRpr.UUID));
            //            if (pRpr == null)
            //            {
            //                nrsEqual = false;
            //                break;
            //            }
            //        }

            //        //test that the now requirement prompt rules are structurally the same
            //        var rNrprs = rNr.NowRequirementPromptRules ?? new List<NowRequirementPromptRule>();
            //        var pNrprs = pNr.NowRequirementPromptRules ?? new List<NowRequirementPromptRule>();
            //        foreach (var rNrpr in rNrprs)
            //        {
            //            var pNrpr = pNrprs.FirstOrDefault(x => (x.ResultPromptRule.MasterUUID ?? x.ResultPromptRule.UUID) == (rNrpr.ResultPromptRule.MasterUUID ?? rNrpr.ResultPromptRule.UUID));
            //            if (pNrpr == null)
            //            {
            //                nrsEqual = false;
            //                break;
            //            }
            //            if (pNrpr != rNrpr)
            //            {
            //                nrsEqual = false;
            //                break;
            //            }
            //        }
            //    }
            //    if (!nrsEqual)
            //    {
            //        continue;
            //    }
            //    toPurge.Add(new Tuple<Now, Now>(rNow, pNow));
            //}








            //var rd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == new Guid("8d7c6359-45e7-4976-bcee-b2d962be6312"));

            //var missingRds = new List<ResultDefinition>();
            //var missingRprs = new List<ResultPromptRule>();

            //foreach (var now in AllData.Nows)
            //{
            //    foreach(var nr in now.AllNowRequirements)
            //    {
            //        var matchedRd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == nr.ResultDefinition.UUID);
            //        if (matchedRd == null)
            //        {
            //            var alreadyMissing = missingRds.FirstOrDefault(x => x.UUID == nr.ResultDefinition.UUID);
            //            if (alreadyMissing == null)
            //            {
            //                missingRds.Add(nr.ResultDefinition);

            //                //check the prompts
            //                if (nr.ResultDefinition.ResultPrompts != null)
            //                {
            //                    foreach (var rpr in nr.ResultDefinition.ResultPrompts)
            //                    {
            //                        var matchedPrompt = AllData.ResultPromptRules.FirstOrDefault(x => x.UUID == rpr.UUID);

            //                        if(matchedPrompt == null)
            //                        {
            //                            var alreadyMissingPrompt = missingRprs.FirstOrDefault(x => x.UUID == rpr.UUID);
            //                            if(alreadyMissingPrompt == null)
            //                            {
            //                                missingRprs.Add(rpr);
            //                            }
            //                        }
            //                    }                                    
            //                }
            //            }
            //        }
            //    }
            //}

            //AllData.ResultDefinitions.AddRange(missingRds);
            //AllData.ResultPromptRules.AddRange(missingRprs);
            //var one = 1;
            ////Ensure that RevisionPending Nows do not have PublishedPending Results or PublishedPending Prompts
            //var revisionNows = AllData.Nows.Where(x => x.PublishedStatus != null && x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
            //foreach(var now in revisionNows)
            //{
            //    foreach(var nr in now.AllNowRequirements)
            //    {
            //        if (nr.ResultDefinition.PublishedStatus != null &&
            //            nr.ResultDefinition.PublishedStatus == PublishedStatus.PublishedPending
            //          )
            //        {
            //            //find revisionPending
            //            var revPendingRd = AllData.ResultDefinitions.First(x => x.PublishedStatus != null && x.PublishedStatus == PublishedStatus.RevisionPending &&
            //                                                                    (
            //                                                                           (x.MasterUUID != null &&
            //                                                                           x.MasterUUID == nr.ResultDefinition.MasterUUID) ||

            //                                                                           (x.MasterUUID == null &&
            //                                                                           x.UUID == nr.ResultDefinition.MasterUUID)
            //                                                                           )
            //                                                                     );
            //            nr.SetResultDefinitionAndPromptRules(revPendingRd);
            //        }
            //    }
            //}

            ////Ensure that PublishedPending Nows do not have RevisionPending Results or RevisionPending Prompts
            //var publishedPendingNows = AllData.Nows.Where(x => x.PublishedStatus != null && x.PublishedStatus == PublishedStatus.PublishedPending).ToList();
            //foreach (var now in publishedPendingNows)
            //{
            //    foreach (var nr in now.AllNowRequirements)
            //    {
            //        if (nr.ResultDefinition.PublishedStatus != null &&
            //            nr.ResultDefinition.PublishedStatus == PublishedStatus.RevisionPending
            //          )
            //        {
            //            //try to find publishPending first
            //            var publishPendingRd = AllData.ResultDefinitions.First(x =>
            //                                                                    (
            //                                                                        x.PublishedStatus != null && x.PublishedStatus == PublishedStatus.PublishedPending &&
            //                                                                        x.MasterUUID != null && nr.ResultDefinition.MasterUUID != null && x.MasterUUID == nr.ResultDefinition.MasterUUID
            //                                                                    ) ||
            //                                                                    (
            //                                                                        x.PublishedStatus != null && x.PublishedStatus == PublishedStatus.PublishedPending &&
            //                                                                        x.MasterUUID != null && nr.ResultDefinition.MasterUUID == null && x.MasterUUID == nr.ResultDefinition.UUID
            //                                                                    ));
            //            nr.SetResultDefinitionAndPromptRules(publishPendingRd);
            //        }
            //    }
            //}
            //MakePublishedPendingNows();
            //MakePublishedNowSubscriptions();
        }

        private void MakePublishedNowSubscriptions()
        {
            var subsWithRevisionPendingNows = AllData.NowSubscriptions.Where(x => x.IncludedNOWS != null && x.IncludedNOWS.Any() && (x.IncludedNOWS.FirstOrDefault(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending) != null)).ToList();

            foreach(var sub in subsWithRevisionPendingNows)
            {
                var revisionPendingNows = sub.IncludedNOWS.Where(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending).ToList();
                //create a revision pending version of the subscription
                var draftSub = sub.Draft();
                AllData.NowSubscriptions.Add(draftSub);
                draftSub.PublishedStatus = PublishedStatus.RevisionPending;
                draftSub.PublishedStatusDate = revisionPendingNows.First().PublishedStatusDate;

                //reset this sub as publishedPending
                sub.PublishedStatus = PublishedStatus.PublishedPending;
                sub.PublishedStatusDate = revisionPendingNows.First().PublishedStatusDate;

                //replace the revisionPending included nows with the publishedPending versions of those nows
                foreach(var now in revisionPendingNows)
                {
                    var publishedPendingNow = AllData.Nows.First(x=>x.CalculatedPublishedStatus == PublishedStatus.PublishedPending 
                                                                && ((x.MasterUUID == null ? x.UUID : x.MasterUUID) == (now.MasterUUID == null ? now.UUID : now.MasterUUID)));
                    sub.IncludedNOWS.Add(publishedPendingNow);
                    sub.IncludedNOWS.Remove(now);
                }
            }

            subsWithRevisionPendingNows = AllData.NowSubscriptions.Where(x => x.ExcludedNOWS != null && x.ExcludedNOWS.Any() && (x.ExcludedNOWS.FirstOrDefault(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending) != null)).ToList();

            foreach (var sub in subsWithRevisionPendingNows)
            {
                var revisionPendingNows = sub.ExcludedNOWS.Where(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending).ToList();
                //create a revision pending version of the subscription
                var draftSub = sub.Draft();
                AllData.NowSubscriptions.Add(draftSub);
                draftSub.PublishedStatus = PublishedStatus.RevisionPending;
                draftSub.PublishedStatusDate = revisionPendingNows.First().PublishedStatusDate;

                //reset this sub as publishedPending
                sub.PublishedStatus = PublishedStatus.PublishedPending;
                sub.PublishedStatusDate = revisionPendingNows.First().PublishedStatusDate;

                //replace the revisionPending included nows with the publishedPending versions of those nows
                foreach (var now in revisionPendingNows)
                {
                    var publishedPendingNow = AllData.Nows.First(x => x.CalculatedPublishedStatus == PublishedStatus.PublishedPending 
                                                                && ((x.MasterUUID == null ? x.UUID : x.MasterUUID) == (now.MasterUUID == null ? now.UUID : now.MasterUUID)));
                    sub.ExcludedNOWS.Add(publishedPendingNow);
                    sub.ExcludedNOWS.Remove(now);
                }
            }
        }

        private void MakePublishedPendingNows()
        {
            var nrsWithPublishedPendingResults = AllData.NowRequirements.Where(x => x.ResultDefinition.CalculatedPublishedStatus == PublishedStatus.PublishedPending).ToList();

            //find and make draft nows
            foreach (var nowGroup in nrsWithPublishedPendingResults.GroupBy(x => x.NOWUUID))
            {
                //deal with the now first
                var now = AllData.Nows.FirstOrDefault(x => x.UUID == nowGroup.Key);
                if (now.CalculatedPublishedStatus == PublishedStatus.Draft)
                {
                    continue;
                }

                //make a draft now - will create new draft versions of now requirements that have the publishedpending results
                var draftNow = now.Draft();

                //make the draft now publishedPending
                draftNow.PublishedStatus = PublishedStatus.PublishedPending;
                draftNow.PublishedStatusDate = nowGroup.First().ResultDefinition.PublishedStatusDate;
                draftNow.LastModifiedDate = nowGroup.First().ResultDefinition.LastModifiedDate;
                draftNow.LastModifiedUser = nowGroup.First().ResultDefinition.LastModifiedUser;
                draftNow.PublicationTags = new List<string>();
                draftNow.PublicationTags.AddRange(nowGroup.First().ResultDefinition.PublicationTags);
                foreach (var nr in draftNow.AllNowRequirements)
                {
                    nr.PublishedStatus = PublishedStatus.PublishedPending;
                    nr.PublishedStatusDate = nowGroup.First().ResultDefinition.PublishedStatusDate;
                    nr.LastModifiedDate = nowGroup.First().ResultDefinition.LastModifiedDate;
                    nr.LastModifiedUser = nowGroup.First().ResultDefinition.LastModifiedUser;
                }

                //set the orginal now as revisionPending, resetting each now requirement back to the revision pending version of the result
                now.PublishedStatus = PublishedStatus.RevisionPending;
                now.PublishedStatusDate = nowGroup.First().ResultDefinition.PublishedStatusDate;
                foreach (var nr in now.AllNowRequirements)
                {
                    nr.PublishedStatus = PublishedStatus.RevisionPending;
                    nr.PublishedStatusDate = nowGroup.First().ResultDefinition.PublishedStatusDate;
                }
                foreach (var nr in nowGroup)
                {
                    var revisionPendingResult = AllData.ResultDefinitions.First(x =>
                                                                                    ((x.MasterUUID == null ? x.UUID : x.MasterUUID)
                                                                                    ==
                                                                                    (nr.ResultDefinition.MasterUUID == null ? nr.ResultDefinition.UUID : nr.ResultDefinition.MasterUUID)
                                                                                    )
                                                                                    && x.PublishedStatus == PublishedStatus.RevisionPending
                                                                                    );
                    nr.ResultDefinition = revisionPendingResult;
                }

                //update the data
                AllData.Nows.Add(draftNow);
                AllData.NowRequirements.AddRange(draftNow.AllNowRequirements);
            }
        }

        private void InitialiseReferenceData()
        {
            //reference data from txt files
            var dataFileDirectory = appSettings.DataFileDirectory;
            string[] filePaths = Directory.GetFiles(dataFileDirectory, "*.txt", SearchOption.TopDirectoryOnly);
            foreach (var filePath in filePaths)
            {
                List<string> data = new List<string>();
                foreach (var line in File.ReadLines(filePath))
                {
                    var parts = line.Split(',');
                    if (parts.Length > 0)
                    {
                        data.AddRange(parts);
                    }
                }

                if (data.Count == 0) { continue; }

                var comboData = new List<ComboBoxItemString>();
                foreach (var item in data)
                {
                    comboData.Add(new ComboBoxItemString() { ValueString = item });
                }

                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name.ToLowerInvariant();
                switch (fileName.ToLowerInvariant())
                {
                    case "jurisdictions.txt":
                        Jurisdictions = comboData;
                        break;
                    case "posthearingcustodystatus.txt":
                        PostHearingCustodyStatus = InitialisePostHearingCustodyStatusReferenceData(comboData);
                        break;
                    case "usergroups.txt":
                        UserGroups = data;
                        break;
                    case "resultdefinitioncategories.txt":
                        ResultDefinitionCategories = comboData;
                        break;
                    case "resultdefinitionlevels.txt":
                        ResultDefinitionLevels = comboData;
                        break;
                    case "resultprompttypes.txt":
                        ResultPromptTypes = comboData;
                        break;
                    case "persontitles.txt":
                        PersonTitles = comboData;
                        break;
                    default:
                        break;
                }
            }

            //pseudo ref data from enumerated values
            CachableOptions = new List<ComboBoxItemString>();
            foreach (CacheableEnum cachableEnum in Enum.GetValues(typeof(CacheableEnum)))
            {
                //get the description
                CachableOptions.Add(new ComboBoxItemString() { ValueString = cachableEnum.GetDescription() });
            }
            StipulatedDrivingTestOptions = new List<ComboBoxItemString>();
            foreach (DrivingTestTypeEnum drivingTestEnum in Enum.GetValues(typeof(DrivingTestTypeEnum)))
            {
                //get the description
                StipulatedDrivingTestOptions.Add(new ComboBoxItemString() { ValueString = drivingTestEnum.GetDescription() });
            }

            //ref data from csv files
            filePaths = Directory.GetFiles(dataFileDirectory, "*.csv", SearchOption.TopDirectoryOnly);

            foreach (var filePath in filePaths)
            {
                List<string> lines = File.ReadLines(filePath).Skip(1).ToList();
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name.ToLowerInvariant();

                switch (fileName.ToLowerInvariant())
                {
                    case "courthouses.csv":
                        foreach (var line in lines)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 2)
                            {
                                if (CourtHouses == null) { CourtHouses = new List<ComboBoxItemString>(); }
                                CourtHouses.Add(new ComboBoxItemString() { CodeString = parts[0].Trim(), ValueString = parts[1].Trim() });
                            }
                        }
                        break;
                    case "featuretoggles.csv":
                        foreach (var line in lines)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 2)
                            {
                                if (AvailableFeatureToggles == null) { AvailableFeatureToggles = new List<ComboBoxItemString>(); }
                                AvailableFeatureToggles.Add(new ComboBoxItemString() { CodeString = parts[0].Trim(), ValueString = parts[1].Trim() });
                            }
                        }
                        break;
                    case "informants.csv":
                        foreach (var line in lines)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 3)
                            {
                                if (Informants == null) { Informants = new List<ComboBoxItemString>(); }
                                Informants.Add(new ComboBoxItemString() { CodeString = parts[0].Trim(), ValueString = parts[1].Trim(), EmailAddressString = parts[2].Trim() });
                            }
                        }
                        break;
                    case "yots.csv":
                        foreach (var line in lines)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 3)
                            {
                                if (Yots == null) { Yots = new List<ComboBoxItemString>(); }
                                Yots.Add(new ComboBoxItemString() { CodeString = parts[0].Trim(), ValueString = parts[1].Trim(), EmailAddressString = parts[2].Trim() });
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            NameAddressTypes = new List<ComboBoxItemString>() { new ComboBoxItemString() { ValueString = "Organisation" }, new ComboBoxItemString() { ValueString = "Person" }, new ComboBoxItemString() { ValueString = "Both" } };
        }

        private List<ComboBoxItemString> InitialisePostHearingCustodyStatusReferenceData(List<ComboBoxItemString> data)
        {
            var res = new List<ComboBoxItemString>();

            foreach (var item in data)
            {
                var parts = item.ValueString.Split('~');
                if (parts.Length > 1)
                {
                    res.Add(new ComboBoxItemString() { ValueString = parts[1], CodeString = parts[0] });
                }
            }

            return res;
        }

        private void InitialiseLists()
        {
            if (AllData.FixedLists == null) { AllData.FixedLists = new List<FixedList>(); }
            if (AllData.NowRequirements == null) { AllData.NowRequirements = new List<NowRequirement>(); }
            if (AllData.Nows == null) { AllData.Nows = new List<Now>(); }
            if (AllData.NowSubscriptions == null) { AllData.NowSubscriptions = new List<NowSubscription>(); }
            if (AllData.ResultDefinitionRules == null) { AllData.ResultDefinitionRules = new List<ResultDefinitionRule>(); }
            if (AllData.ResultDefinitions == null) { AllData.ResultDefinitions = new List<ResultDefinition>(); }
            if (AllData.ResultDefinitionWordGroups == null) { AllData.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroup>(); }
            if (AllData.ResultPromptRules == null) { AllData.ResultPromptRules = new List<ResultPromptRule>(); }
            if (AllData.ResultPromptWordGroups == null) { AllData.ResultPromptWordGroups = new List<ResultPromptWordGroup>(); }
        }

        public AllData AllData
        {
            get; set;
        }

        public IdMap Map
        {
            get; set;
        }

        public string SaveErrors { get; set; }

        #region SaveCommand

        /// <summary>
        /// Returns the command used to execute a save data to local file
        /// </summary>
        public ICommand SaveToFileCommand
        {
            get; private set;
        }

        #endregion // SaveCommand

        #region ValidateCommand

        /// <summary>
        /// Returns the command used to validate the data in the tree
        /// </summary>
        public IValidateCommand ValidateCommand
        {
            get; private set;
        }

        #endregion // ValidateCommand

        #region SampleNowCommand

        /// <summary>
        /// Returns the command used to generate a sample NOW
        /// </summary>
        public ICommand SampleNowCommand
        {
            get; private set;
        }

        #endregion // SampleNowCommand

        #region ResequenceCommand

        /// <summary>
        /// Returns the command used to execute resequence of the data
        /// </summary>
        public ICommand ResequenceCommand
        {
            get; private set;
        }

        #endregion // ResequenceCommand

        #region SearchCommand

        /// <summary>
        /// Returns the command used to execute a search in the family tree.
        /// </summary>
        public ICommand SearchItemCommand
        {
            get; private set;
        }
        #endregion // SearchCommand

        #region DataPatchCommand

        public ICommand DataPatchByTagCommand
        {
            get; private set;
        }
        

        #endregion // DataPatchCommand

        #region GenerateReportCommand

        /// <summary>
        /// Returns the command used to generate a report
        /// </summary>
        public ICommand GenerateReportCommand
        {
            get; private set;
        }
        #endregion  GenerateReportCommand

        #region ImportWelshCommand

        /// <summary>
        /// Returns the command used to import welsh translations
        /// </summary>
        public ICommand ImportWelshCommand
        {
            get; private set;
        }
        #endregion  ImportWelshCommand

        #region Add Commands

        /// <summary>
        /// Returns the command used to create a new result definition at the point of the selected tree view item
        /// </summary>
        public ICommand AddNewResultDefinitionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new child result definition at the point of the selected tree view item
        /// </summary>
        public ICommand AddNewChildResultDefinitionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new result prompt for this result definition
        /// </summary>
        public ICommand AddNewResultPromptCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new word group for this result definition
        /// </summary>
        public ICommand AddNewResultDefinitionWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new user group for this result definition
        /// </summary>
        public ICommand AddUserGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new fixed list
        /// </summary>
        public ICommand AddFixedListCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to add a new now
        /// </summary>
        public ICommand AddNewNowCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to add a new edt
        /// </summary>
        public ICommand AddNewEDTCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new NOW
        /// </summary>
        public ICommand AddNowCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new NOW Requirement
        /// </summary>
        public ICommand AddNowRequirementCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new prompt word group for a result prompt
        /// </summary>
        public ICommand AddNewResultPromptWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new Now Subscription
        /// </summary>
        public ICommand AddNewNowSubscriptionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new Now Subscription as a child of a parent subscription
        /// </summary>
        public ICommand AddNewChildNowSubscriptionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new Now Requirement Text Item
        /// </summary>
        public ICommand AddNowRequirementTextValueCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create a new Now Text Item
        /// </summary>
        public ICommand AddNowTextValueCommand
        {
            get; private set;
        }

        #endregion Add Commands

        #region Copy Commands

        /// <summary>
        /// Returns the command used to copy/create a shallow clone of a result definition.  Note that the clone is not inserted into the tree until the paste command is executed
        /// </summary>
        public ICommand CopyResultDefinitionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to copy a Now under into a subscription.  Note that the implementation does not create any clones or manipulate the trees
        /// </summary>
        public ICommand CopyNowCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to copy a EDT under into a subscription.  Note that the implementation does not create any clones or manipulate the trees
        /// </summary>
        public ICommand CopyEDTCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to copy/create a shallow clone of a result prompt.  Note that the clone is not inserted into the tree until the paste command is executed
        /// </summary>
        public ICommand CopyResultPromptCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to copy/create a shallow clone of a result definition word group.  Note that the clone is not inserted into the tree until the paste command is executed
        /// </summary>
        public ICommand CopyResultDefinitionWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to copy/create a shallow clone of a user group.  Note that the clone is not inserted into the tree until the paste command is executed
        /// </summary>
        public ICommand CopyUserGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to copy/create a shallow clone of a fixed list.  Note that the clone is not inserted into the tree until the paste command is executed
        /// </summary>
        public ICommand CopyFixedListCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to copy/create a shallow clone of a result prompt word group.  Note that the clone is not inserted into the tree until the paste command is executed
        /// </summary>
        public ICommand CopyResultPromptWordGroupCommand
        {
            get; private set;
        }

        #endregion Copy Commands

        #region Paste Commands

        /// <summary>
        /// Returns the command used to paste a result definition copied by the copy command
        /// </summary>
        public ICommand PasteResultDefinitionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to paste a result definition copied by the copy command as a child of another result prompt
        /// </summary>
        public ICommand PasteResultDefinitionChildCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to paste a result prompt copied by the copy command
        /// </summary>
        public ICommand PasteResultPromptCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to paste a result definition word group copied by the copy command
        /// </summary>
        public ICommand PasteResultDefinitionWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to paste a user group copied by the copy command
        /// </summary>
        public ICommand PasteUserGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to paste a fixed list copied by the copy command
        /// </summary>
        public ICommand PasteFixedListCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to paste a result prompt word group copied by the copy command
        /// </summary>
        public ICommand PasteResultPromptWordGroupCommand
        {
            get; private set;
        }

        #endregion Paste Commands

        #region Draft Commands

        /// <summary>
        /// Returns the command used to create draft versions of result definitions
        /// </summary>
        public ICommand DraftResultDefinitionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create draft versions of result prompts
        /// </summary>
        public ICommand DraftResultPromptCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create draft versions of result definition word groups
        /// </summary>
        public ICommand DraftResultDefinitionWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create draft versions of fixed lists
        /// </summary>
        public ICommand DraftFixedListCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create draft versions of result prompt word groups
        /// </summary>
        public ICommand DraftResultPromptWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create draft versions of NOWs/EDTs
        /// </summary>
        public ICommand DraftNowCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to create draft versions of NowSubscriptions
        /// </summary>
        public ICommand DraftNowSubscriptionCommand
        {
            get; private set;
        }

        #endregion

        #region Publish Commands

        /// <summary>
        /// Returns the command used to publish draft changes
        /// </summary>
        public ICommand PublishDraftsCommand
        {
            get; private set;
        }

        #endregion

        #region Delete Commands

        /// <summary>
        /// Returns the command used to delete a set of revision data items
        /// </summary>
        public ICommand DeleteRevisionsCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to set a result definition from the tree as being deleted
        /// </summary>
        public ICommand SetDeleteResultDefinitionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a result definition from the tree
        /// </summary>
        public ICommand DeleteResultDefinitionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a result prompt from the tree
        /// </summary>
        public ICommand DeleteResultPromptCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a result definition word group from the tree
        /// </summary>
        public ICommand DeleteResultDefinitionWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a user group from the tree
        /// </summary>
        public ICommand DeleteUserGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a fixed list from the tree
        /// </summary>
        public ICommand DeleteFixedListCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to set a NOW from the tree as being deleted
        /// </summary>
        public ICommand SetDeleteNowCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a NOW
        /// </summary>
        public ICommand DeleteNowCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a NOW Requirement
        /// </summary>
        public ICommand DeleteNowRequirementCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to set a EDT from the tree as being deleted
        /// </summary>
        public ICommand SetDeleteEDTCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a EDT
        /// </summary>
        public ICommand DeleteEDTCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a result prompt word group from the tree
        /// </summary>
        public ICommand DeleteResultPromptWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a now requirement text item
        /// </summary>
        public ICommand DeleteNowRequirementTextValueCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a now text item
        /// </summary>
        public ICommand DeleteNowTextValueCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to delete a now subscription
        /// </summary>
        public ICommand DeleteNowSubscriptionCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to set a now subscription as deleted
        /// </summary>
        public ICommand SetDeleteNowSubscriptionCommand
        {
            get; private set;
        }        

        #endregion Delete Commands

        #region Edit Commands

        /// <summary>
        /// Returns the command used to edit a user group
        /// </summary>
        public ICommand EditUserGroupCommand
        {
            get; private set;
        }

        #endregion Edit Commands

        #region Undelete Commands

        /// <summary>
        /// Returns the command used to undelete a fixed list
        /// </summary>
        public ICommand UndeleteFixedListCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to undelete a result prompt word group
        /// </summary>
        public ICommand UndeleteResultPromptWordGroupCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to undelete a result prompt
        /// </summary>
        public ICommand UndeleteResultPromptCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to undelete a result definition
        /// </summary>
        public ICommand UndeleteResultDefinitionCommand
        {
            get; private set;
        }

        #endregion

        #region Subscription Commands

        /// <summary>
        /// Returns the command used to set the result prompts for subscriptions
        /// </summary>
        public ICommand SetSubscriptionFromResultCommand
        {
            get; private set;
        }

        /// <summary>
        /// Returns the command used to paste now or edt into the included/excluded array
        /// </summary>
        public ICommand PasteIncludedExcludedSubscriptionCommand
        {
            get; private set;
        }

        #endregion Subscription Commands

        #region Tree Model Properties For Binding

        bool isReadOnly = false;
        public override bool IsReadOnly
        {
            get => isReadOnly;
            set
            {
                isReadOnly = value;
                base.IsReadOnly = value;
                OnPropertyChanged("SaveVisibility");
                OnPropertyChanged("CheckInVisibility");
                OnPropertyChanged("CheckedOutWarning");
            }
        }

        bool isLocalChangeMode = false;
        public bool IsLocalChangeMode
        {
            get => isLocalChangeMode;
            set
            {
                SetProperty(ref isLocalChangeMode, value);
                OnPropertyChanged("SaveVisibility");
                OnPropertyChanged("CheckInVisibility");
            }
        }
        public bool IsTest
        {
            get
            {
                return appSettings.IsTest;
            }
        }

        public bool IsNotTest
        {
            get
            {
                return !IsTest;
            }
        }

        public string VersionText
        {
            get => appSettings.AppVersion;
        }

        User checkOutUser = null;
        public User CheckOutUser
        {
            get => checkOutUser;
            set
            {
                SetProperty(ref checkOutUser, value);
                OnPropertyChanged("SaveVisibility");
                OnPropertyChanged("CheckInVisibility");
                OnPropertyChanged("CheckedOutWarning");
            }
        }

        public bool SaveVisibility
        {
            get
            {
                if (IdentityHelper.IsSignedInUserALocalEdditor() && isLocalChangeMode) { return true; }

                if (CheckOutUser == null || IsReadOnly) { return false; }
                if (CheckOutUser.Email != IdentityHelper.SignedInUser.Email) { return false; }
                return true;
            }
        }

        public bool CheckInVisibility
        {
            get
            {
                if (IdentityHelper.IsSignedInUserALocalEdditor() && isLocalChangeMode) { return false; }
                return SaveVisibility;
            }
        }

        public string CheckedOutWarning
        {
            get
            {
                if (IsReadOnly) { return "Not authenticated"; }
                if (CheckOutUser == null || string.IsNullOrEmpty(CheckOutUser.Email)) { return null;}
                if (CheckOutUser.Email != IdentityHelper.SignedInUser.Email) { return string.Format("Checked out to:{0}'{1}'", Environment.NewLine, CheckOutUser.Email); }
                return null;
            }
        }

        public string LoggedOnUserText
        {
            get
            {
                if (IsReadOnly) { return "Not Signed In"; }
                return string.Format("Signed in as {0}", IdentityHelper.SignedInUser.Email);
            }
        }

        public SilentObservableCollection<ResultDefinitionTreeViewModel> ResultDefinitionRootsDraft
        {
            get { return AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft; }
        }

        public SilentObservableCollection<ResultDefinitionTreeViewModel> ResultDefinitionRootsPublished
        {
            get { return AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished; }
        }

        public SilentObservableCollection<ResultDefinitionRevisionTreeViewModel> ResultDefinitionRevision
        {
            get { return AllResultDefinitionsTreeViewModel.ResultDefinitionRevision; }
        }

        public SilentObservableCollection<NowTreeViewModel> Nows
        {
            get { return AllNowsTreeViewModel.Nows; }
        }

        public SilentObservableCollection<NowTreeViewModel> NowsDraft
        {
            get { return AllNowsTreeViewModel.NowsDraft; }
        }

        public SilentObservableCollection<NowTreeViewModel> NowsPublished
        {
            get { return AllNowsTreeViewModel.NowsPublished; }
        }

        public SilentObservableCollection<NowRevisionTreeViewModel> NowsRevision
        {
            get { return AllNowsTreeViewModel.NowsRevision; }
        }

        public SilentObservableCollection<NowTreeViewModel> EDTs
        {
            get { return AllEDTsTreeViewModel.EDTs; }
        }

        public SilentObservableCollection<NowTreeViewModel> EdtsDraft
        {
            get { return AllEDTsTreeViewModel.EdtsDraft; }
        }

        public SilentObservableCollection<NowTreeViewModel> EdtsPublished
        {
            get { return AllEDTsTreeViewModel.EdtsPublished; }
        }

        public SilentObservableCollection<NowRevisionTreeViewModel> EdtsRevision
        {
            get { return AllEDTsTreeViewModel.EdtsRevision; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> NowSubscriptions
        {
            get { return AllNowSubscriptionsTreeViewModel.NowSubscriptions; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> NowSubscriptionsDraft
        {
            get { return AllNowSubscriptionsTreeViewModel.NowSubscriptionsDraft; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> NowSubscriptionsPublished
        {
            get { return AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished; }
        }

        public SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> NowSubscriptionsRevision
        {
            get { return AllNowSubscriptionsTreeViewModel.NowSubscriptionsRevision; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptions
        {
            get { return AllEDTSubscriptionsTreeViewModel.EDTSubscriptions; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptionsDraft
        {
            get { return AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsDraft; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptionsPublished
        {
            get { return AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished; }
        }

        public SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> EDTSubscriptionsRevision
        {
            get { return AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsRevision; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> InformantRegisterSubscriptions
        {
            get { return AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions; }
        }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> InformantRegisterSubscriptionsDraft
        {
            get { return AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsDraft; }
        }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> InformantRegisterSubscriptionsPublished
        {
            get { return AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished; }
        }
        public SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> InformantRegisterSubscriptionsRevision
        {
            get { return AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsRevision; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> CourtRegisterSubscriptions
        {
            get { return AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions; }
        }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> CourtRegisterSubscriptionsDraft
        {
            get { return AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsDraft; }
        }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> CourtRegisterSubscriptionsPublished
        {
            get { return AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished; }
        }
        public SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> CourtRegisterSubscriptionsRevision
        {
            get { return AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsRevision; }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> PrisonCourtRegisterSubscriptions
        {
            get { return AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions; }
        }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> PrisonCourtRegisterSubscriptionsDraft
        {
            get { return AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsDraft; }
        }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> PrisonCourtRegisterSubscriptionsPublished
        {
            get { return AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished; }
        }
        public SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> PrisonCourtRegisterSubscriptionsRevision
        {
            get { return AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsRevision; }
        }

        #region SearchText

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (value == searchText)
                    return;

                searchText = value;
            }
        }
        #endregion // SearchText

        int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                SetProperty(ref selectedIndex, value);
            }
        }

        #region Result Prompt Rule User Group CRUD

        string selectedPromptRuleUserGroup = null;
        public string SelectedPromptRuleUserGroup
        {
            get => selectedPromptRuleUserGroup;
            set
            {
                SetProperty(ref selectedPromptRuleUserGroup, value);
                OnPropertyChanged("RemovePromptRuleUserGroupEnabled");
            }
        }

        string selectedAvailablePromptRuleUserGroup = null;
        public string SelectedAvailablePromptRuleUserGroup
        {
            get => selectedAvailablePromptRuleUserGroup;
            set
            {
                SetProperty(ref selectedAvailablePromptRuleUserGroup, value);
                OnPropertyChanged("AddPromptRuleUserGroupEnabled");
            }
        }

        public bool AddPromptRuleUserGroupEnabled
        {
            get => selectedAvailablePromptRuleUserGroup != null ? true : false;
        }

        public bool RemovePromptRuleUserGroupEnabled
        {
            get => selectedPromptRuleUserGroup != null ? true : false;
        }

        #endregion Result Prompt Rule User Group CRUD

        #region Result Definition User Group CRUD

        string selectedResultDefinitionUserGroup = null;
        public string SelectedResultDefinitionUserGroup
        {
            get => selectedResultDefinitionUserGroup;
            set
            {
                SetProperty(ref selectedResultDefinitionUserGroup, value);
                OnPropertyChanged("RemoveResultDefinitionUserGroupEnabled");
            }
        }

        string selectedAvailableResultDefinitionUserGroup = null;
        public string SelectedAvailableResultDefinitionUserGroup
        {
            get => selectedAvailableResultDefinitionUserGroup;
            set
            {
                SetProperty(ref selectedAvailableResultDefinitionUserGroup, value);
                OnPropertyChanged("AddResultDefinitionUserGroupEnabled");
            }
        }

        public bool AddResultDefinitionUserGroupEnabled
        {
            get => selectedAvailableResultDefinitionUserGroup != null ? true : false;
        }

        public bool RemoveResultDefinitionUserGroupEnabled
        {
            get => selectedResultDefinitionUserGroup != null ? true : false;
        }

        #endregion Result Definition User Group CRUD

        TreeViewItemViewModel selectedTreeViewModel = null;

        public TreeViewItemViewModel SelectedItem
        {
            get => selectedTreeViewModel;
            set
            {
                //cancel out the previous selection
                if (selectedTreeViewModel != null && value != null && selectedTreeViewModel.Id != value.Id)
                {
                    selectedTreeViewModel.IsSelected = false;
                }                

                //set the new selection
                SetProperty(ref selectedTreeViewModel, value);
                OnPropertyChanged("SelectedNowVisibility");
                OnPropertyChanged("SelectedNowSubscriptionVisibility");
                OnPropertyChanged("SelectedNowRequirementVisibility");
                OnPropertyChanged("SelectedNowRequirementResultPromptVisibility");
                OnPropertyChanged("SelectedResultDefinitionVisibility");
                OnPropertyChanged("SelectedResultPromptVisibility");
                OnPropertyChanged("SelectedFixedListVisibility");
                OnPropertyChanged("SelectedResultDefinitionWordGroupVisibility");
                OnPropertyChanged("SelectedResultPromptWordGroupVisibility");
                OnPropertyChanged("SelectedResultDefinitionRuleVisibility");
                OnPropertyChanged("ResultPromptUndeleteVisibility");
                OnPropertyChanged("FixedListUndeleteVisibility");
                OnPropertyChanged("ResultPromptDeleteVisibility");
                OnPropertyChanged("ResultPromptDraftChildActionsVisibility");
                OnPropertyChanged("FixedListDeleteVisibility");
                OnPropertyChanged("ResultPromptWordGroupUndeleteVisibility");
                OnPropertyChanged("ResultPromptWordGroupDeleteVisibility");
                OnPropertyChanged("ResultPromptDraftVisibility");
                OnPropertyChanged("ResultDefinitionDraftVisibility");
                OnPropertyChanged("ResultDefinitionWordGroupDraftVisibility");
                OnPropertyChanged("ResultPromptWordGroupDraftVisibility");
                OnPropertyChanged("FixedListDraftVisibility");
                OnPropertyChanged("ResultDefinitionPublishVisibility");
                OnPropertyChanged("NOWPublishVisibility");
                OnPropertyChanged("NOWSubscriptionPublishVisibility");
                OnPropertyChanged("EDTPublishVisibility");
                OnPropertyChanged("ResultDefinitionUndeleteVisibility");
                OnPropertyChanged("ResultDefinitionDeleteVisibility");
                OnPropertyChanged("SetResultDefinitionDeleteVisibility");
                OnPropertyChanged("AddPasteRDWGVisibility");
                OnPropertyChanged("RevisionsDeleteVisibility");

                //paste visibility will change based on the selected item
                if (CopiedTreeViewModel != null)
                {
                    OnPropertyChanged("NowPasteVisibility");
                    OnPropertyChanged("EDTPasteVisibility");
                    OnPropertyChanged("NowRequirementPasteVisibility");
                    OnPropertyChanged("ResultDefinitionPasteVisibility");
                    OnPropertyChanged("ResultPromptPasteVisibility");
                    OnPropertyChanged("UserGroupPasteVisibility");
                    OnPropertyChanged("ResultDefinitionWordGroupPasteVisibility");
                    OnPropertyChanged("ResultPromptWordGroupPasteVisibility");
                    OnPropertyChanged("FixedListPasteVisibility");
                    OnPropertyChanged("ResultDefinitionChildPasteVisibility");
                }

                //when a subscription is selected, detrmine if a context menu can be displayed
                var subscriptionTreeItem = value as NowSubscriptionTreeViewModel;
                if (subscriptionTreeItem != null)
                {
                    //inform the view model that it is now in focus
                    subscriptionTreeItem.NowSubscriptionViewModel.SelectedFocusReceived();
                }
            }
        }

        TreeViewItemViewModel selectedEditTreeViewModel = null;
        public TreeViewItemViewModel SelectedEditTreeViewModel
        {
            get => selectedEditTreeViewModel;
            set
            {
                //reset the old edit mode
                if (selectedEditTreeViewModel != null)
                {
                    selectedEditTreeViewModel.IsInEditMode = false;
                }

                //set the new edit mode
                if (value != null) { value.IsInEditMode = true; }
                SetProperty(ref selectedEditTreeViewModel, value);
            }
        }

        public bool SelectedResultDefinitionRuleVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel && ((ResultDefinitionTreeViewModel)selectedTreeViewModel).ResultRuleViewModel.ParentUUID != null ? true : false;
        }

        public bool SelectedNowSubscriptionVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is NowSubscriptionTreeViewModel ? true : false;
        }

        public bool SelectedNowVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is NowTreeViewModel ? true : false;
        }

        public bool SelectedNowRequirementVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is NowRequirementTreeViewModel ? true : false;
        }

        public bool SelectedNowRequirementResultPromptVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is NowRequirementPromptRuleTreeViewModel ? true : false;
        }

        public bool SelectedResultDefinitionVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel ? true : false;
        }

        public bool SelectedResultPromptVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptTreeViewModel ? true : false;
        }

        public bool SelectedFixedListVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is FixedListTreeViewModel ? true : false;
        }

        public bool SelectedResultDefinitionWordGroupVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionWordGroupTreeViewModel ? true : false;
        }

        public bool SelectedResultPromptWordGroupVisibility
        {
            get => selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptWordGroupTreeViewModel ? true : false;
        }

        #region Reference Data
        public List<ComboBoxItemString> ResultDefinitionCategories { get; private set; }

        public List<ComboBoxItemString> PersonTitles { get; private set; }

        public List<ComboBoxItemString> ResultDefinitionLevels { get; private set; }

        public List<ComboBoxItemString> PostHearingCustodyStatus { get; private set; }

        public List<string> UserGroups { get; private set; }

        public List<ComboBoxItemString> UserGroupsAsCombo
        {
            get
            {
                if (UserGroups == null) { return null; }
                var res = new List<ComboBoxItemString>();
                UserGroups.ForEach(x => res.Add(new ComboBoxItemString() { CodeString = x, ValueString = x, CheckStatus = false }));
                return res;
            }
        }

        public SilentObservableCollection<string> AllPublicationTags
        {
            get
            {               
                var allTags = CalculateDraftPublishedPendingTags();
                return new SilentObservableCollection<string>(allTags);
            }
        }

        private List<string> CalculateDraftPublishedPendingTags()
        {
            var allTags = new List<string>();
            foreach (var item in AllData.ResultDefinitionRules.Where(x => x.PublicationTags != null && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            foreach (var item in AllData.ResultDefinitions.Where(x => x.PublicationTags != null
                                                                    && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            foreach (var item in AllData.ResultPromptRules.Where(x => x.PublicationTags != null
                                                                    && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            foreach (var item in AllData.ResultPromptRules.Where(x => x.ResultPrompt.PublicationTags != null
                                                                    && (x.ResultPrompt.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.ResultPrompt.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.ResultPrompt.PublicationTags);
            }

            foreach (var item in AllData.FixedLists.Where(x => x.PublicationTags != null && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            foreach (var item in AllData.ResultDefinitionWordGroups.Where(x => x.PublicationTags != null && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            foreach (var item in AllData.ResultPromptWordGroups.Where(x => x.PublicationTags != null && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            foreach (var item in AllData.Nows.Where(x => x.PublicationTags != null && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            foreach (var item in AllData.NowSubscriptions.Where(x => x.PublicationTags != null && (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Draft)))
            {
                allTags.AddRange(item.PublicationTags);
            }

            return allTags.Select(x => x).Distinct().OrderBy(x=>x).ToList();
        }

        public List<ComboBoxItemString> Jurisdictions { get; private set; }

        public List<ComboBoxItemString> ResultPromptTypes { get; private set; }

        public List<ComboBoxItemString> NameAddressTypes { get; private set; }

        public List<ComboBoxItemString> CourtHouses { get; private set; }

        public List<ComboBoxItemString> Informants { get; private set; }

        public List<ComboBoxItemString> Yots { get; private set; }

        public List<ComboBoxItemString> CachableOptions { get; private set; }

        public List<ComboBoxItemString> StipulatedDrivingTestOptions { get; private set; }

        public List<ComboBoxItemString> AvailableFeatureToggles { get; set; }

        #endregion Reference Data

        #endregion // TreeModel Properties For Binding

        #region Draft Binding

        public void ResetDraftVisibility()
        {
            OnPropertyChanged("ResultDefinitionDraftVisibility");
            OnPropertyChanged("ResultDefinitionWordGroupDraftVisibility");
            OnPropertyChanged("ResultPromptWordGroupDraftVisibility");
            OnPropertyChanged("ResultPromptDraftVisibility");
            OnPropertyChanged("FixedListDraftVisibility");
        }

        public bool ResultDefinitionDraftVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var treeItem = (ResultDefinitionTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultRuleViewModel.PublishedStatus == PublishedStatus.Draft &&
                            (treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Published ||
                                treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.PublishedPending
                            )
                        )
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ResultDefinitionWordGroupDraftVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionWordGroupTreeViewModel)
                {
                    var treeItem = (ResultDefinitionWordGroupTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultDefinitionWordGroupViewModel.PublishedStatus == PublishedStatus.Published ||
                        treeItem.ResultDefinitionWordGroupViewModel.PublishedStatus == PublishedStatus.PublishedPending)
                    {
                        //ensure the the parent result definition is a draft
                        var parentResultWordGroupTreeItem = treeItem.Parent as ResultWordGroupTreeViewModel;
                        if (parentResultWordGroupTreeItem != null)
                        {
                            var parentRd = parentResultWordGroupTreeItem.ResultWordGroupViewModel.ParentResultDefinitionVM;
                            if (parentRd.PublishedStatus == PublishedStatus.Draft)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        public bool FixedListDraftVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is FixedListTreeViewModel)
                {
                    var treeItem = (FixedListTreeViewModel)selectedTreeViewModel;
                    if (treeItem.FixedListViewModel.PublishedStatus == PublishedStatus.Published ||
                        treeItem.FixedListViewModel.PublishedStatus == PublishedStatus.PublishedPending)
                    {
                        //ensure the the parent result prompt is a draft
                        var parentResultPromptTreeItem = treeItem.Parent as ResultPromptTreeViewModel;
                        if (parentResultPromptTreeItem != null &&
                            parentResultPromptTreeItem.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool ResultPromptWordGroupDraftVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptWordGroupTreeViewModel)
                {
                    var treeItem = (ResultPromptWordGroupTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultPromptWordGroupViewModel.PublishedStatus == PublishedStatus.Published ||
                        treeItem.ResultPromptWordGroupViewModel.PublishedStatus == PublishedStatus.PublishedPending)
                    {
                        //ensure the the parent result prompt is a draft
                        var parentResultPromptTreeItem = treeItem.Parent as ResultPromptTreeViewModel;
                        if (parentResultPromptTreeItem != null &&
                            parentResultPromptTreeItem.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool ResultDefinitionPublishVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var treeItem = (ResultDefinitionTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultRuleViewModel.PublishedStatus == PublishedStatus.Draft && treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool NOWPublishVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is NowTreeViewModel)
                {
                    var treeItem = (NowTreeViewModel)selectedTreeViewModel;
                    if (treeItem.NowViewModel.PublishedStatus == PublishedStatus.Draft  && !treeItem.NowViewModel.IsEDT)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool EDTPublishVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is NowTreeViewModel)
                {
                    var treeItem = (NowTreeViewModel)selectedTreeViewModel;
                    if (treeItem.NowViewModel.PublishedStatus == PublishedStatus.Draft && treeItem.NowViewModel.IsEDT)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool NOWSubscriptionPublishVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is NowSubscriptionTreeViewModel)
                {
                    var treeItem = (NowSubscriptionTreeViewModel)selectedTreeViewModel;
                    if (treeItem.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ResultPromptDraftVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptTreeViewModel)
                {
                    var treeItem = (ResultPromptTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultPromptRuleViewModel.PublishedStatus == PublishedStatus.Draft &&
                            (treeItem.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Published ||
                                treeItem.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending
                            )
                        )
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region Undelete Binding

        public bool ResultPromptUndeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptTreeViewModel)
                {
                    var treeItem = (ResultPromptTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultPromptRuleViewModel.IsDeleted)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ResultPromptDeleteVisibility
        {
            get
            {
                return !ResultPromptUndeleteVisibility;
            }
        }

        public bool ResultPromptDraftChildActionsVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptTreeViewModel)
                {
                    var treeItem = (ResultPromptTreeViewModel)selectedTreeViewModel;
                    if (!treeItem.ResultPromptRuleViewModel.IsDeleted && treeItem.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ResultPromptWordGroupUndeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptWordGroupTreeViewModel)
                {
                    var treeItem = (ResultPromptWordGroupTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultPromptWordGroupViewModel.IsDeleted)
                    {
                        //check that parent is not deleted
                        var parent = treeItem.Parent as ResultPromptTreeViewModel;
                        if (!parent.ResultPromptRuleViewModel.ResultPromptViewModel.IsDeleted)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool ResultPromptWordGroupDeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptWordGroupTreeViewModel)
                {
                    var treeItem = (ResultPromptWordGroupTreeViewModel)selectedTreeViewModel;
                    //ensure the the parent result prompt is a draft
                    var parentResultPromptTreeItem = treeItem.Parent as ResultPromptTreeViewModel;
                    if (parentResultPromptTreeItem != null &&
                        parentResultPromptTreeItem.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool FixedListUndeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is FixedListTreeViewModel)
                {
                    var treeItem = (FixedListTreeViewModel)selectedTreeViewModel;
                    if (treeItem.FixedListViewModel.IsDeleted)
                    {
                        //check that parent is not deleted
                        var parent = treeItem.Parent as ResultPromptTreeViewModel;
                        if (!parent.ResultPromptRuleViewModel.ResultPromptViewModel.IsDeleted)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool FixedListDeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is FixedListTreeViewModel)
                {
                    var treeItem = (FixedListTreeViewModel)selectedTreeViewModel;
                    //ensure the the parent result prompt is a draft
                    var parentResultPromptTreeItem = treeItem.Parent as ResultPromptTreeViewModel;
                    if (parentResultPromptTreeItem != null &&
                        parentResultPromptTreeItem.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void SetRootResultDeleted()
        {
            OnPropertyChanged("ResultDefinitionUndeleteVisibility");
        }

        public bool ResultDefinitionUndeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var treeItem = (ResultDefinitionTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultRuleViewModel.IsDeleted || treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool RevisionsDeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel.Label.ToLowerInvariant() != "pending" && 
                        (
                            selectedTreeViewModel is ResultDefinitionRevisionTreeViewModel ||
                            selectedTreeViewModel is NowRevisionTreeViewModel ||
                            selectedTreeViewModel is NowSubscriptionRevisionTreeViewModel
                        )
                    )
                {
                    return true;
                }
                return false;
            }
        }

        public bool ResultDefinitionDeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var treeItem = (ResultDefinitionTreeViewModel)selectedTreeViewModel;
                    if (treeItem.ResultRuleViewModel.ParentResultDefinitionViewModel == null)
                    {
                        //root item child result for the rule must be draft
                        if (treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //Child result item, rule must be draft
                    if (treeItem.ResultRuleViewModel.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        public bool SetResultDefinitionDeleteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var treeItem = (ResultDefinitionTreeViewModel)selectedTreeViewModel;
                    if (!treeItem.ResultRuleViewModel.IsDeleted && !treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted
                        && treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft
                        && treeItem.ResultRuleViewModel.ParentResultDefinitionViewModel == null
                        )
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool AddPasteRDWGVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var treeItem = (ResultDefinitionTreeViewModel)selectedTreeViewModel;
                    if (!treeItem.ResultRuleViewModel.IsDeleted && !treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted
                        && treeItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft
                        )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultWordGroupTreeViewModel)
                {
                    var treeItem = (ResultWordGroupTreeViewModel)selectedTreeViewModel;
                    var parent = treeItem.ResultWordGroupViewModel.ParentResultDefinitionVM;
                    if (!parent.IsDeleted && parent.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionWordGroupTreeViewModel)
                {
                    var treeItem = (ResultDefinitionWordGroupTreeViewModel)selectedTreeViewModel;
                    var parentWg = treeItem.Parent as ResultWordGroupTreeViewModel;
                    var parent = parentWg.ResultWordGroupViewModel.ParentResultDefinitionVM;
                    if (!parent.IsDeleted && parent.PublishedStatus == PublishedStatus.Draft)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        #endregion

        #region Paste Binding

        TreeViewItemViewModel copiedTreeViewModel;

        public TreeViewItemViewModel CopiedTreeViewModel
        {
            get => copiedTreeViewModel;
            set
            {
                SetProperty(ref copiedTreeViewModel, value);
                OnPropertyChanged("NowPasteVisibility");
                OnPropertyChanged("EDTPasteVisibility");
                OnPropertyChanged("NowRequirementPasteVisibility");
                OnPropertyChanged("ResultDefinitionPasteVisibility");

                OnPropertyChanged("ResultPromptPasteVisibility");
                OnPropertyChanged("UserGroupPasteVisibility");
                OnPropertyChanged("ResultDefinitionWordGroupPasteVisibility");
                OnPropertyChanged("ResultPromptWordGroupPasteVisibility");
                OnPropertyChanged("FixedListPasteVisibility");
                OnPropertyChanged("ResultDefinitionChildPasteVisibility");
            }
        }

        public bool NowPasteVisibility
        {
            get
            {
                if (CopiedTreeViewModel != null && CopiedTreeViewModel.GetType() == typeof(NowTreeViewModel))
                {
                    var treeItem = (NowTreeViewModel)CopiedTreeViewModel;
                    if (!treeItem.NowViewModel.IsEDT)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool EDTPasteVisibility
        {
            get
            {
                if (CopiedTreeViewModel != null && CopiedTreeViewModel.GetType() == typeof(NowTreeViewModel))
                {
                    var treeItem = (NowTreeViewModel)CopiedTreeViewModel;
                    if (treeItem.NowViewModel.IsEDT)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool NowRequirementPasteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is NowTreeViewModel)
                {
                    var item = (NowTreeViewModel)SelectedItem;
                    if (item.NowViewModel.IsDeleted)
                    {
                        return false;
                    }

                    return CopiedTreeViewModel != null
                      &&
                      CopiedTreeViewModel.GetType() == typeof(ResultDefinitionTreeViewModel)
                      ?
                      true : false;
                }
                return false;
            }
        }

        public bool ResultDefinitionPasteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel || selectedTreeViewModel is NowSubscriptionTreeViewModel)
                {
                    var selectedRd = SelectedItem as ResultDefinitionTreeViewModel;
                    if (selectedRd != null)
                    {
                        if (selectedRd.ResultRuleViewModel.IsDeleted || selectedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted
                            || selectedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus != PublishedStatus.Draft)
                        {
                            return false;
                        }
                    }

                    var selectedSubs = SelectedItem as NowSubscriptionTreeViewModel;
                    if (selectedSubs != null)
                    {
                        if (selectedSubs.NowSubscriptionViewModel.IsDeleted || selectedSubs.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft
                            || (!selectedSubs.NowSubscriptionViewModel.ApplySubscriptionRules && !selectedSubs.NowSubscriptionViewModel.RecipientFromResultsVisibility))
                        {
                            return false;
                        }
                    }

                    return CopiedTreeViewModel != null
                      &&
                      CopiedTreeViewModel.GetType() == typeof(ResultDefinitionTreeViewModel)
                      ?
                      true : false;
                }
                return false;
            }
        }
        public bool ResultDefinitionChildPasteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var item = (ResultDefinitionTreeViewModel)SelectedItem;
                    if (item.ResultRuleViewModel.IsDeleted || item.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted
                        || item.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus != PublishedStatus.Draft)
                    {
                        return false;
                    }

                    return CopiedTreeViewModel != null
                      &&
                      (CopiedTreeViewModel.GetType() == typeof(ResultPromptTreeViewModel) || CopiedTreeViewModel.GetType() == typeof(ResultDefinitionWordGroupTreeViewModel))
                      ?
                      true : false;
                }
                return false;
            }
        }

        public bool ResultPromptPasteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel || selectedTreeViewModel is NowSubscriptionTreeViewModel || selectedTreeViewModel is ResultPromptTreeViewModel)
                {
                    var selectedRd = SelectedItem as ResultDefinitionTreeViewModel;
                    if (selectedRd != null)
                    {
                        if (selectedRd.ResultRuleViewModel.IsDeleted || selectedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted
                            || selectedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus != PublishedStatus.Draft)
                        {
                            return false;
                        }
                    }

                    var selectedSubs = SelectedItem as NowSubscriptionTreeViewModel;
                    if (selectedSubs != null)
                    {
                        if (selectedSubs.NowSubscriptionViewModel.IsDeleted || selectedSubs.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft
                            || (!selectedSubs.NowSubscriptionViewModel.ApplySubscriptionRules && !selectedSubs.NowSubscriptionViewModel.RecipientFromResultsVisibility))
                        {
                            return false;
                        }
                    }

                    return CopiedTreeViewModel != null
                      &&
                      CopiedTreeViewModel.GetType() == typeof(ResultPromptTreeViewModel)
                      ?
                      true : false;
                }
                return false;
            }
        }

        public bool ResultDefinitionWordGroupPasteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionTreeViewModel)
                {
                    var item = selectedTreeViewModel as ResultDefinitionTreeViewModel;
                    if (item.ResultRuleViewModel.IsDeleted || item.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted
                        || item.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus != PublishedStatus.Draft)
                    {
                        return false;
                    }
                }
                else if (selectedTreeViewModel != null && selectedTreeViewModel is ResultWordGroupTreeViewModel)
                {
                    var item = selectedTreeViewModel as ResultWordGroupTreeViewModel;
                    var parentResult = item.ResultWordGroupViewModel.ParentResultDefinitionVM;
                    if (parentResult.IsDeleted || parentResult.PublishedStatus != PublishedStatus.Draft)
                    {
                        return false;
                    }
                }
                else if (selectedTreeViewModel != null && selectedTreeViewModel is ResultDefinitionWordGroupTreeViewModel)
                {
                    var item = selectedTreeViewModel as ResultDefinitionWordGroupTreeViewModel;
                    var parentWG = item.Parent as ResultWordGroupTreeViewModel;
                    var parentResult = parentWG.ResultWordGroupViewModel.ParentResultDefinitionVM;
                    if (parentResult.IsDeleted || parentResult.PublishedStatus != PublishedStatus.Draft)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                return CopiedTreeViewModel != null
                  &&
                  (CopiedTreeViewModel.GetType() == typeof(ResultDefinitionWordGroupTreeViewModel) || CopiedTreeViewModel.GetType() == typeof(ResultWordGroupTreeViewModel))
                  ?
                  true : false;
            }
        }

        public bool ResultPromptWordGroupPasteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptTreeViewModel)
                {
                    var item = (ResultPromptTreeViewModel)SelectedItem;
                    if (item.ResultPromptRuleViewModel.IsDeleted)
                    {
                        return false;
                    }

                    //ensure that the parent result prompt is a draft
                    if (item.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus != PublishedStatus.Draft)
                    {
                        return false;
                    }

                    return CopiedTreeViewModel != null
                      &&
                      (CopiedTreeViewModel.GetType() == typeof(ResultPromptWordGroupTreeViewModel) || CopiedTreeViewModel.GetType() == typeof(ResultPromptTreeViewModel))
                      ?
                      true : false;
                }
                return false;
            }
        }

        public bool FixedListPasteVisibility
        {
            get
            {
                if (selectedTreeViewModel != null && selectedTreeViewModel is ResultPromptTreeViewModel)
                {
                    var item = (ResultPromptTreeViewModel)SelectedItem;
                    if (item.ResultPromptRuleViewModel.IsDeleted)
                    {
                        return false;
                    }

                    //ensure that the parent result prompt is a draft
                    if (item.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus != PublishedStatus.Draft)
                    {
                        return false;
                    }

                    return CopiedTreeViewModel != null
                      &&
                      CopiedTreeViewModel.GetType() == typeof(FixedListTreeViewModel)
                      ?
                      true : false;
                }
                return false;
            }
        }

        #endregion Paste Binding

        #region Cascade Now Text Commands

        /// <summary>
        /// Returns the command used to cascade now text to all nows and edts
        /// </summary>
        public ICommand CascadeNowTextCommand
        {
            get; private set;
        }

        #endregion

        public void RefreshResults()
        {
            var first = AllData.ResultDefinitions.FirstOrDefault(x => x.DeletedDate == null);
            first.ResultDefinitionRules = null;
            AllData.ResultDefinitions = new List<ResultDefinition>() { first };
            allResultRuleViewModel = null;
            allResultDefinitionsTreeViewModel.Refresh(AllResultRuleViewModel);
        }

        public AllResultDefinitionsTreeViewModel AllResultDefinitionsTreeViewModel
        {
            get
            {
                if (allResultDefinitionsTreeViewModel == null)
                {
                    //populate the view data
                    allResultRuleViewModel = new AllResultRuleViewModel();
                    allResultDefinitionsViewModel = new AllResultDefinitionsViewModel(this, allResultRuleViewModel, AllData.ResultDefinitions, AllResultPromptViewModel, AllResultDefinitionWordGroupViewModel, UserGroups);
                    
                    //make the tree views 
                    allResultDefinitionsTreeViewModel = new AllResultDefinitionsTreeViewModel(this, AllResultRuleViewModel);
                }
                return allResultDefinitionsTreeViewModel;
            }
        }

        public AllNowsTreeViewModel AllNowsTreeViewModel
        {
            get
            {
                if (allNowsTreeViewModel == null)
                {
                    allNowsTreeViewModel = new AllNowsTreeViewModel(this, AllNowsViewModel);
                }
                return allNowsTreeViewModel;
            }
        }

        public AllEDTsTreeViewModel AllEDTsTreeViewModel
        {
            get
            {
                if (allEDTsTreeViewModel == null)
                {
                    allEDTsTreeViewModel = new AllEDTsTreeViewModel(this, AllEDTsViewModel);
                }
                return allEDTsTreeViewModel;
            }
        }

        public AllNowSubscriptionsTreeViewModel AllNowSubscriptionsTreeViewModel
        {
            get
            {
                if (allNowSubscriptionsTreeViewModel == null)
                {
                    allNowSubscriptionsTreeViewModel = new AllNowSubscriptionsTreeViewModel(this, AllNowSubscriptionViewModel, AllNowsTreeViewModel);
                }
                return allNowSubscriptionsTreeViewModel;
            }
        }

        public AllEDTSubscriptionsTreeViewModel AllEDTSubscriptionsTreeViewModel
        {
            get
            {
                if (allEDTSubscriptionsTreeViewModel == null)
                {
                    allEDTSubscriptionsTreeViewModel = new AllEDTSubscriptionsTreeViewModel(this, AllEDTSubscriptionViewModel, AllEDTsTreeViewModel);
                }
                return allEDTSubscriptionsTreeViewModel;
            }
        }

        public AllInformantRegisterSubscriptionsTreeViewModel AllInformantRegisterSubscriptionsTreeViewModel
        {
            get
            {
                if (allInformantRegisterSubscriptionsTreeViewModel == null)
                {
                    allInformantRegisterSubscriptionsTreeViewModel = new AllInformantRegisterSubscriptionsTreeViewModel(this, AllInformantRegisterSubscriptionViewModel);
                }
                return allInformantRegisterSubscriptionsTreeViewModel;
            }
        }

        public AllCourtRegisterSubscriptionsTreeViewModel AllCourtRegisterSubscriptionsTreeViewModel
        {
            get
            {
                if (allCourtRegisterSubscriptionsTreeViewModel == null)
                {
                    allCourtRegisterSubscriptionsTreeViewModel = new AllCourtRegisterSubscriptionsTreeViewModel(this, AllCourtRegisterSubscriptionViewModel);
                }
                return allCourtRegisterSubscriptionsTreeViewModel;
            }
        }

        public AllPrisonCourtRegisterSubscriptionsTreeViewModel AllPrisonCourtRegisterSubscriptionsTreeViewModel
        {
            get
            {
                if (allPrisonCourtRegisterSubscriptionsTreeViewModel == null)
                {
                    allPrisonCourtRegisterSubscriptionsTreeViewModel = new AllPrisonCourtRegisterSubscriptionsTreeViewModel(this, AllPrisonCourtRegisterSubscriptionViewModel);
                }
                return allPrisonCourtRegisterSubscriptionsTreeViewModel;
            }
        }

        public AllResultRuleViewModel AllResultRuleViewModel
        {
            get => allResultRuleViewModel;
            //{
            //    if (allResultRuleViewModel == null)
            //    {
            //        allResultRuleViewModel = new AllResultRuleViewModel();
            //    }
            //    if (!allResultRuleViewModel.Rules.Any())
            //    {
            //        allResultDefinitionsViewModel = new AllResultDefinitionsViewModel(this, allResultRuleViewModel, AllData.ResultDefinitions, AllResultPromptViewModel, AllResultDefinitionWordGroupViewModel, UserGroups);
            //    }
            //    return allResultRuleViewModel;
            //}
        }

        public AllResultDefinitionsViewModel AllResultDefinitionsViewModel
        {
            get => allResultDefinitionsViewModel;
            //{
            //    if (allResultDefinitionsViewModel == null)
            //    {
            //        allResultRuleViewModel = new AllResultRuleViewModel();

            //        allResultDefinitionsViewModel = new AllResultDefinitionsViewModel(this, allResultRuleViewModel, AllData.ResultDefinitions, AllResultPromptViewModel, AllResultDefinitionWordGroupViewModel, UserGroups);
            //    }
            //    return allResultDefinitionsViewModel;
            //}
        }

        public AllResultPromptViewModel AllResultPromptViewModel
        {
            get
            {
                if (allResultPromptViewModel == null)
                {
                    allResultPromptViewModel = new AllResultPromptViewModel(this,AllData.ResultPromptRules, AllResultPromptWordGroupViewModel, UserGroups);
                }
                return allResultPromptViewModel;
            }
        }

        public AllFixedListViewModel AllFixedListViewModel
        {
            get
            {
                if (allFixedListViewModel == null)
                {
                    allFixedListViewModel = new AllFixedListViewModel(this, AllData.FixedLists);
                }
                return allFixedListViewModel;
            }
        }

        public AllResultDefinitionWordGroupViewModel AllResultDefinitionWordGroupViewModel
        {
            get
            {
                if (allResultDefinitionWordGroupViewModel == null)
                {
                    allResultDefinitionWordGroupViewModel = new AllResultDefinitionWordGroupViewModel(this, AllData.ResultDefinitionWordGroups);
                }
                return allResultDefinitionWordGroupViewModel;
            }
        }

        public AllResultPromptWordGroupViewModel AllResultPromptWordGroupViewModel
        {
            get
            {
                if (allResultPromptWordGroupViewModel == null)
                {
                    allResultPromptWordGroupViewModel = new AllResultPromptWordGroupViewModel(this, AllData.ResultPromptWordGroups);
                }
                return allResultPromptWordGroupViewModel;
            }
        }

        public AllNowsViewModel AllNowsViewModel
        {
            get
            {
                if (allNowsViewModel == null)
                {
                    allNowsViewModel = new AllNowsViewModel(this, AllData.Nows, AllNowRequirementsViewModel, AllResultDefinitionsViewModel);
                }
                return allNowsViewModel;
            }
        }

        public AllEDTsViewModel AllEDTsViewModel
        {
            get
            {
                if (allEDTsViewModel == null)
                {
                    allEDTsViewModel = new AllEDTsViewModel(this, AllData.Nows, AllNowRequirementsViewModel, AllResultDefinitionsViewModel);
                }
                return allEDTsViewModel;
            }
        }

        public AllNowSubscriptionViewModel AllNowSubscriptionViewModel
        {
            get
            {
                if (allNowSubscriptionsViewModel == null)
                {
                    allNowSubscriptionsViewModel = new AllNowSubscriptionViewModel(this, AllData.NowSubscriptions);
                }
                return allNowSubscriptionsViewModel;
            }
        }

        public AllEDTSubscriptionViewModel AllEDTSubscriptionViewModel
        {
            get
            {
                if (allEDTSubscriptionsViewModel == null)
                {
                    allEDTSubscriptionsViewModel = new AllEDTSubscriptionViewModel(this, AllData.NowSubscriptions);
                }
                return allEDTSubscriptionsViewModel;
            }
        }

        public AllInformantRegisterSubscriptionViewModel AllInformantRegisterSubscriptionViewModel
        {
            get
            {
                if (allInformantRegisterSubscriptionsViewModel == null)
                {
                    allInformantRegisterSubscriptionsViewModel = new AllInformantRegisterSubscriptionViewModel(this, AllData.NowSubscriptions);
                }
                return allInformantRegisterSubscriptionsViewModel;
            }
        }

        public AllCourtRegisterSubscriptionViewModel AllCourtRegisterSubscriptionViewModel
        {
            get
            {
                if (allCourtRegisterSubscriptionsViewModel == null)
                {
                    allCourtRegisterSubscriptionsViewModel = new AllCourtRegisterSubscriptionViewModel(this, AllData.NowSubscriptions);
                }
                return allCourtRegisterSubscriptionsViewModel;
            }
        }

        public AllPrisonCourtRegisterSubscriptionViewModel AllPrisonCourtRegisterSubscriptionViewModel
        {
            get
            {
                if (allPrisonCourtRegisterSubscriptionsViewModel == null)
                {
                    allPrisonCourtRegisterSubscriptionsViewModel = new AllPrisonCourtRegisterSubscriptionViewModel(this, AllData.NowSubscriptions);
                }
                return allPrisonCourtRegisterSubscriptionsViewModel;
            }
        }

        public AllNowRequirementsViewModel AllNowRequirementsViewModel
        {
            get
            {
                if (allNowRequirementsViewModel == null)
                {
                    allNowRequirementsViewModel = new AllNowRequirementsViewModel(this, AllData.NowRequirements, AllResultDefinitionsViewModel);
                }
                return allNowRequirementsViewModel;
            }
        }
    }
}
