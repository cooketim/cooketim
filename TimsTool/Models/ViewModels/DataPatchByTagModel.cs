using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DataLib;
using ExportExcel;
using Identity;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper for patching.
    /// </summary>
    public class DataPatchByTagViewModel : ViewModelBase
    {
        string dataFileDirectory;
        public DataPatchByTagViewModel(ITreeModel treeModel, string dataFileDirectory)
        {
            this.treeModel = treeModel;
            this.dataFileDirectory = dataFileDirectory;

            //PublicationTagsModel = new PublicationTagsModel(this, GetPendingTags());
            PublicationTagsModel = new PublicationTagsModel(this, GetPendingTags());
            FeatureTogglesModel = new FeatureTogglesModel(treeModel);

            PublicationTagsModel.SelectedPublicationTags.CollectionChanged += publicationTags_CollectionChanged;
            FeatureTogglesModel.SelectedFeatureToggles.CollectionChanged += featureToggles_CollectionChanged;
        }

        private List<string> GetPendingTags()
        {
            var res = new List<string>();

            //determine the distinct set of tags from published pending data items
            res.AddRange(treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllResultPromptViewModel.Prompts.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllResultPromptWordGroupViewModel.WordGroups.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllNowsViewModel.Nows.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllEDTsViewModel.EDTs.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllNowRequirementsViewModel.NowRequirements.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));
            res.AddRange(treeModel.AllFixedListViewModel.FixedLists.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.PublicationTags != null)
                                                                    .Select(x => x.PublicationTags)
                                                                    .SelectMany(l => l));

            return res.Distinct().ToList();
        }

        public void PrepareExportData()
        {
            //reset the state
            deletedResults = null;
            createdOrModifiedResultViews = null;
            createdOrModifiedResultPrompts = null;
            deletedResultPrompts = null;
            removedResultPrompts = null;
            deletedNows = null;
            createdOrModifiedNows = null;
            deletedNowSubscriptions = null;
            createdOrModifiedNowSubscriptions = null;
            deletedFixedLists = null;
            createdOrModifiedFixedLists = null;
            activeResultDefinitionSynonyms = null;
            activeResultPromptSynonyms = null;

            //prepare the report data
            PrepareChangeReportdata();
        }

        private void PrepareChangeReportdata()
        {
            ChangeReportData = new ChangeReportData(FeatureToggles, PublicationTags, PatchDate, IsTestExport, IsProdExport, IdentityHelper.SignedInUser.Username);
            DeletedResults.ForEach(x => ChangeReportData.DeletedResults.Add(new ChangeReportResultDataItem(x.ResultDefinition, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            ModifiedPublishedPendingResults.ForEach(x => ChangeReportData.ModifiedResults.Add(new ChangeReportResultDataItem(x.ResultDefinition, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            ToggledResults.ForEach(x => ChangeReportData.ToggledResults.Add(new ChangeReportResultDataItem(x.ResultDefinition, new List<Comment>())));
            NewResults.ForEach(x => ChangeReportData.NewResults.Add(new ChangeReportResultDataItem(x.ResultDefinition, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            NewNows.ForEach(x => ChangeReportData.NewNows.Add(new ChangeReportNowDataItem(x.Now, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            DeletedNows.ForEach(x => ChangeReportData.DeletedNows.Add(new ChangeReportNowDataItem(x.Now, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            ModifiedNows.ForEach(x => ChangeReportData.ModifiedNows.Add(new ChangeReportNowDataItem(x.Now, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            DeletedNowSubscriptions.ForEach(x => ChangeReportData.DeletedNowSubscriptions.Add(new ChangeReportNowSubscriptionDataItem(x.NowSubscription, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            ModifiedNowSubscriptions.ForEach(x => ChangeReportData.ModifiedNowSubscriptions.Add(new ChangeReportNowSubscriptionDataItem(x.NowSubscription, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            NewNowSubscriptions.ForEach(x => ChangeReportData.NewNowSubscriptions.Add(new ChangeReportNowSubscriptionDataItem(x.NowSubscription, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            DeletedFixedLists.ForEach(x => ChangeReportData.DeletedFixedLists.Add(new ChangeReportFixedListDataItem(x.FixedList, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            ModifiedFixedLists.ForEach(x => ChangeReportData.ModifiedFixedLists.Add(new ChangeReportFixedListDataItem(x.FixedList, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            NewFixedLists.ForEach(x => ChangeReportData.NewFixedLists.Add(new ChangeReportFixedListDataItem(x.FixedList, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                             && y.PublishedStatus == PublishedStatus.Draft
                                                             && y.ParentUUID == x.UUID
                                                             && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));

            //embelish results with prompt changes
            var resultsToEmbelish = new List<ChangeReportResultDataItem>(ChangeReportData.NewResults);
            resultsToEmbelish.AddRange(ChangeReportData.ModifiedResults);
            foreach (var result in resultsToEmbelish)
            {
                DeletedResultPrompts.Where(x => x.ResultDefinitionViewModel.UUID == result.Data.UUID).ToList().ForEach(x => result.DeletedPrompts.Add(new ChangeReportResultPromptDataItem(x.ResultPromptRule, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                                 && y.PublishedStatus == PublishedStatus.Draft
                                                                 && y.ParentUUID == x.ResultPromptViewModel.UUID
                                                                 && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
                RemovedResultPrompts.Where(x => x.ResultDefinitionViewModel.UUID == result.Data.UUID).ToList().ForEach(x => result.RemovedPrompts.Add(new ChangeReportResultPromptDataItem(x.ResultPromptRule, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                                 && y.PublishedStatus == PublishedStatus.Draft
                                                                 && y.ParentUUID == x.ResultPromptViewModel.UUID
                                                                 && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
                ModifiedResultPrompts.Where(x=>x.ResultDefinitionViewModel.UUID == result.Data.UUID).ToList().ForEach(x=> result.ModifiedPrompts.Add(new ChangeReportResultPromptDataItem(x.ResultPromptRule, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                                 && y.PublishedStatus == PublishedStatus.Draft
                                                                 && y.ParentUUID == x.ResultPromptViewModel.UUID
                                                                 && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
                NewResultPrompts.Where(x => x.ResultDefinitionViewModel.UUID == result.Data.UUID).ToList().ForEach(x => result.NewPrompts.Add(new ChangeReportResultPromptDataItem(x.ResultPromptRule, treeModel.AllData.Comments.Where(y => y.PublishedStatus != null
                                                                 && y.PublishedStatus == PublishedStatus.Draft
                                                                 && y.ParentUUID == x.ResultPromptViewModel.UUID
                                                                 && !string.IsNullOrEmpty(y.SystemCommentType)).OrderBy(z => z.LastModifiedDate).ToList())));
            }
        }

        public ChangeReportData ChangeReportData { get; private set; }

        private void publicationTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("IsOKEnabled");
        }

        private void featureToggles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("IsOKEnabled");
        }

        public PublicationTagsModel PublicationTagsModel { get; set; }

        public FeatureTogglesModel FeatureTogglesModel { get; set; }

        public List<string> FeatureToggles
        {
            get
            {
                return FeatureTogglesModel.SelectedFeatureToggles.Select(x => x.CodeValueString).ToList();
            }
        }

        private List<string> FeatureToggleCodes
        {
            get
            {
                return FeatureTogglesModel.SelectedFeatureToggles.Select(x => x.CodeString).ToList();
            }
        }

        public string DataFileDirectory
        {
            get => dataFileDirectory;
        }

        private DateTime patchDate = DateTime.Now;
        public DateTime PatchDate
        {
            get => patchDate;
            set
            {
                SetProperty(() => patchDate == value, () => patchDate = value);
            }
        }

        private bool isTestExport = true;
        public bool IsTestExport
        {
            get => isTestExport;
            set
            {
                if (SetProperty(() => isTestExport == value, () => isTestExport = value))
                {
                    if (value)
                    {
                        isProdExport = false;
                    }
                    else
                    {
                        isProdExport = true;
                    }
                    OnPropertyChanged("IsProdExport");
                }
            }
        }

        private bool isProdExport = false;
        public bool IsProdExport
        {
            get => isProdExport;
            set
            {
                if (SetProperty(() => isProdExport == value, () => isProdExport = value))
                {
                    if (value)
                    {
                        isTestExport = false;
                    }
                    else
                    {
                        isTestExport = true;
                    }
                    OnPropertyChanged("IsTestExport");
                }
            }
        }

        private bool isFullExport = false;
        public bool IsFullExport
        {
            get => isFullExport;
            set
            {
                if (SetProperty(() => isFullExport == value, () => isFullExport = value))
                {
                    if (value)
                    {
                        publicationTagsVisible = false;
                    }
                    else
                    {
                        publicationTagsVisible = true;
                    }
                    OnPropertyChanged("IsPublicationTagsVisible");
                    OnPropertyChanged("IsOKEnabled");
                }
            }
        }

        private bool publicationTagsVisible = true;
        public bool IsPublicationTagsVisible
        {
            get => publicationTagsVisible;
        }

        public bool IsOKEnabled
        {
            get
            {
                if (isFullExport)
                {
                    return true;
                }
                if (publicationTagsVisible && PublicationTags != null && PublicationTags.Count > 0)
                {
                    return true;
                }
                if(FeatureTogglesModel.SelectedFeatureToggles != null && FeatureTogglesModel.SelectedFeatureToggles.Count>0)
                {
                    return true;
                }
                return false;
            }
        }

        #region result data for patching
        private List<ResultDefinitionViewModel> deletedResults = null;
        private List<ResultDefinitionViewModel> createdOrModifiedResultViews = null;
        private List<ResultPromptRuleViewModel> createdOrModifiedResultPrompts = null;
        private List<ResultPromptRuleViewModel> deletedResultPrompts = null;
        private List<ResultPromptRuleViewModel> removedResultPrompts = null;

        public List<ResultDefinitionViewModel> DeletedResults
        {
            get
            {
                if (deletedResults == null)
                {                    
                    var publishedPendingDeletedData = treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Everything that is already published will need to be deleted.
                        deletedResults = treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.Published).ToList();

                        //Additionally, everything that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData)
                        {
                            var matchedRevisionPending = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedResults.Add(matchedRevisionPending);
                        }

                        //Finally, everything that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingResults = treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingResults)
                        {
                            var matchedPublishedPending = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedResults.Add(item);
                        }
                    }
                    else
                    {
                        deletedResults = new List<ResultDefinitionViewModel>();
                        if (publishedPendingDeletedData != null && PublicationTags != null)
                        {                            
                            foreach (var item in publishedPendingDeletedData)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedResults.Add(item);
                                }
                            }
                        }
                    }
                    
                }
                return deletedResults;
            }
        }
        public List<ResultDefinitionViewModel> CreatedOrModifiedResults
        {
            get
            {
                if (createdOrModifiedResultViews == null)
                {                    
                    var publishedPendingCreatedOrModifiedData = treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport) 
                    {
                        //Every Result that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedResultViews = publishedPendingCreatedOrModifiedData;

                        //Additionally, every Result that is already published will need to be published again,
                        //note any result that has publishedPending by definition will have a corresponding revisionPending and there will not be a result with status pending
                        createdOrModifiedResultViews.AddRange(treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));
                    }
                    else
                    {
                        createdOrModifiedResultViews = new List<ResultDefinitionViewModel>();
                        if (publishedPendingCreatedOrModifiedData != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingCreatedOrModifiedData)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedResultViews.Add(item);
                                }
                            }
                        }
                        //Append any toggled results i.e. include any published results that are subject to a feature toggle 
                        AppendGetToggledResults(createdOrModifiedResultViews);
                    }
                    
                }
                return createdOrModifiedResultViews;
            }
        }

        private void AppendGetToggledResults(List<ResultDefinitionViewModel> createdOrModifiedData)
        {
            if (FeatureToggles == null || !FeatureToggles.Any())
            {
                return;
            }
            var toggledResults = new List<ResultDefinitionViewModel>();
            var publishedPublishedPendingResults = treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) && x.DeletedDate == null).ToList();
            
            //get published results for the enabled feature toggle, note we never append draft results or revisions
            foreach (var toggle in FeatureToggleCodes)
            {
                switch (toggle.ToLowerInvariant().Trim())
                {
                    case "cct-1635":
                        var res1635 = (from x in
                                publishedPublishedPendingResults
                                    .Where(x =>  (x.Prompts != null && x.Prompts.Any() && (x.Prompts.FirstOrDefault(y=>y.RuleType == null) != null))
                                                 ||
                                                 (x.Rules != null && x.Rules.Any() && (x.Rules.FirstOrDefault(y => y.RuleType == null) != null))
                                                 // is a relevant result for this feature i.e. has the common rule type overridden
                                        && !(
                                              from cm in createdOrModifiedData
                                              select cm.MasterUUID
                                            ).Contains(x.MasterUUID) // is not already included
                                        &&
                                           !(
                                              from t in toggledResults
                                              select t.MasterUUID
                                            ).Contains(x.MasterUUID)) // is not already toggled in
                                       select x).ToList();
                        if (res1635.Any())
                        {
                            toggledResults.AddRange(res1635);
                        }
                        break;
                    case "cct-1076":                        
                        var res1076 = (from x in
                                publishedPublishedPendingResults
                                    .Where(x => x.JudicialValidation // is a relevant result for this feature
                                        && !(
                                              from cm in createdOrModifiedData
                                              select cm.MasterUUID
                                            ).Contains(x.MasterUUID) // is not already included
                                        &&
                                           !(
                                              from t in toggledResults
                                              select t.MasterUUID
                                            ).Contains(x.MasterUUID)) // is not already toggled in
                                       select x).ToList();
                        if (res1076.Any())
                        {
                            toggledResults.AddRange(res1076);
                        }
                        break;
                    case "dd-16049":
                        var res16049 = (from x in
                                publishedPublishedPendingResults
                                    .Where(x => !string.IsNullOrEmpty(x.TriggeredApplicationCode) // is a relevant result for this feature
                                        && !(
                                              from cm in createdOrModifiedData
                                              select cm.MasterUUID
                                            ).Contains(x.MasterUUID) // is not already included
                                        &&
                                           !(
                                              from t in toggledResults
                                              select t.MasterUUID
                                            ).Contains(x.MasterUUID)) // is not already toggled in
                                        select x).ToList();
                        if (res16049.Any())
                        {
                            toggledResults.AddRange(res16049);
                        }
                        break;
                    case "cct-1326":
                        var res1326 = (from x in
                                publishedPublishedPendingResults
                                    .Where(x => (!string.IsNullOrEmpty(x.ResultTextTemplate) || !string.IsNullOrEmpty(x.DependantResultDefinitionGroup)) // is a relevant result for this feature
                                        && !(
                                              from cm in createdOrModifiedData
                                              select cm.MasterUUID
                                            ).Contains(x.MasterUUID) // is not already included
                                        &&
                                           !(
                                              from t in toggledResults
                                              select t.MasterUUID
                                            ).Contains(x.MasterUUID)) // is not already toggled in
                                       select x).ToList();
                        if (res1326.Any())
                        {
                            toggledResults.AddRange(res1326);
                        }
                        break;
                    case "cct-1471":
                        var res1471 = (from x in
                                publishedPublishedPendingResults
                                    .Where(x => x.IsBusinessResult // is a relevant result for this feature
                                        && !(
                                              from cm in createdOrModifiedData
                                              select cm.MasterUUID
                                            ).Contains(x.MasterUUID) // is not already included
                                        &&
                                           !(
                                              from t in toggledResults
                                              select t.MasterUUID
                                            ).Contains(x.MasterUUID)) // is not already toggled in
                                       select x).ToList();
                        if (res1471.Any())
                        {
                            toggledResults.AddRange(res1471);
                        }
                        break;
                    case "dd-13386":
                        var res13386 = (from x in
                                publishedPublishedPendingResults
                                    .Where(x => !string.IsNullOrEmpty(x.ResultDefinitionGroup) &&
                                                        x.ResultDefinitionGroup.ToLowerInvariant().Contains("enforcement")
                                                        // is a relevant result for this feature
                                        && !(
                                              from cm in createdOrModifiedData
                                              select cm.MasterUUID
                                            ).Contains(x.MasterUUID) // is not already included
                                        &&
                                           !(
                                              from t in toggledResults
                                              select t.MasterUUID
                                            ).Contains(x.MasterUUID)) // is not already toggled in
                                       select x).ToList();
                        if (res13386.Any())
                        {
                            toggledResults.AddRange(res13386);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (!toggledResults.Any())
            {
                return;
            }
            createdOrModifiedData.AddRange(toggledResults);
        }

        public List<ResultDefinitionViewModel> ModifiedResults
        {
            get => CreatedOrModifiedResults.Where(x => x.ResultDefinition.IsNewItem == null || !x.ResultDefinition.IsNewItem.Value).ToList();
        }
        private List<ResultDefinitionViewModel> ModifiedPublishedPendingResults
        {
            get => ModifiedResults.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending).ToList();
        }
        private List<ResultDefinitionViewModel> ToggledResults
        {
            get => ModifiedResults.Where(x => x.PublishedStatus == PublishedStatus.Published).ToList();
        }
        public List<ResultDefinitionViewModel> NewResults
        {
            get => CreatedOrModifiedResults.Where(x => x.ResultDefinition.IsNewItem.HasValue && x.ResultDefinition.IsNewItem.Value).ToList();
        }
        public List<ResultPromptRuleViewModel> DeletedResultPrompts
        {
            get
            {
                if (deletedResultPrompts == null)
                {
                    deletedResultPrompts = new List<ResultPromptRuleViewModel>();

                    foreach (var item in CreatedOrModifiedResults.Where(x=>x.Prompts != null && x.Prompts.Any()))
                    {
                        deletedResultPrompts.AddRange(item.Prompts.Where(x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending && x.ResultPromptViewModel.DeletedDate != null));
                    }
                }
                return deletedResultPrompts;
            }
        }
        public List<ResultPromptRuleViewModel> RemovedResultPrompts
        {
            get
            {
                if (removedResultPrompts == null)
                {
                    removedResultPrompts = new List<ResultPromptRuleViewModel>();

                    foreach (var item in CreatedOrModifiedResults.Where(x => x.Prompts != null && x.Prompts.Any()))
                    {
                        removedResultPrompts.AddRange(item.Prompts.Where(x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.ResultPromptViewModel.DeletedDate == null));
                    }
                }
                return removedResultPrompts;
            }
        }
        public List<ResultPromptRuleViewModel> CreatedOrModifiedResultPrompts
        {
            get
            {
                if (createdOrModifiedResultPrompts == null)
                {
                    createdOrModifiedResultPrompts = new List<ResultPromptRuleViewModel>();

                    foreach (var item in CreatedOrModifiedResults.Where(x => x.Prompts != null && x.Prompts.Any()))
                    {
                        createdOrModifiedResultPrompts.AddRange(item.Prompts.Where(x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.ResultPromptViewModel.DeletedDate == null));
                    }
                }
                return createdOrModifiedResultPrompts;
            }
        }
        public List<ResultPromptRuleViewModel> ModifiedResultPrompts
        {
            get => CreatedOrModifiedResultPrompts.Where(x => x.ResultPromptViewModel.ResultPrompt.IsNewItem == null || !x.ResultPromptViewModel.ResultPrompt.IsNewItem.Value).ToList();
        }
        public List<ResultPromptRuleViewModel> NewResultPrompts
        {
            get
            {
                var res = CreatedOrModifiedResultPrompts.Where(x => x.ResultPromptViewModel.ResultPrompt.IsNewItem != null && x.ResultPromptViewModel.ResultPrompt.IsNewItem.Value).ToList();
                return res;
            }
        }

        #endregion

        #region nows data for patching
        private List<NowViewModel> deletedNows = null;
        private List<NowViewModel> createdOrModifiedNows = null;
        public List<NowViewModel> DeletedNows
        {
            get
            {
                if (deletedNows == null)
                {                    
                    var publishedPendingDeletedData_nows = treeModel.AllNowsViewModel.Nows.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every NOW that is already published will need to be deleted.
                        deletedNows = treeModel.AllNowsViewModel.Nows.Where(x => x.PublishedStatus == PublishedStatus.Published).ToList();

                        //Additionally, every now that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData_nows)
                        {
                            var matchedRevisionPending = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedNows.Add(matchedRevisionPending);
                        }

                        //Finally, every now that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingNows = treeModel.AllNowsViewModel.Nows.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingNows)
                        {
                            var matchedPublishedPending = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedNows.Add(item);
                        }
                    }
                    else
                    {
                        deletedNows = new List<NowViewModel>();
                        if (publishedPendingDeletedData_nows != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingDeletedData_nows)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedNows.Add(item);
                                }
                            }
                        }
                    }

                    var publishedPendingDeletedData_edts = treeModel.AllEDTsViewModel.EDTs.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every EDT that is already published will need to be deleted.
                        deletedNows.AddRange(treeModel.AllEDTsViewModel.EDTs.Where(x => x.PublishedStatus == PublishedStatus.Published));

                        //Additionally, every EDT that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData_nows)
                        {
                            var matchedRevisionPending = treeModel.AllEDTsViewModel.EDTs.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedNows.Add(matchedRevisionPending);
                        }

                        //Finally, every EDT that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingEdts = treeModel.AllEDTsViewModel.EDTs.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingEdts)
                        {
                            var matchedPublishedPending = treeModel.AllEDTsViewModel.EDTs.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedNows.Add(item);
                        }
                    }
                    else
                    {
                        if (publishedPendingDeletedData_edts != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingDeletedData_edts)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedNows.Add(item);
                                }
                            }
                        }
                    }
                }
                return deletedNows;
            }
        }
        public List<NowViewModel> CreatedOrModifiedNows
        {
            get
            {
                if (createdOrModifiedNows == null)
                {
                    var publishedPendingCreatedOrModifiedData_nows = treeModel.AllNowsViewModel.Nows.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every NOW that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedNows = publishedPendingCreatedOrModifiedData_nows;

                        //Additionally, every NOW that is already published will need to be published again.
                        createdOrModifiedNows.AddRange(treeModel.AllNowsViewModel.Nows.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));

                    }
                    else
                    {
                        createdOrModifiedNows = new List<NowViewModel>();
                        if (publishedPendingCreatedOrModifiedData_nows != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingCreatedOrModifiedData_nows)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedNows.Add(item);
                                }
                            }
                        }
                    }

                    var publishedPendingCreatedOrModifiedData_edts = treeModel.AllEDTsViewModel.EDTs.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every EDT that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedNows.AddRange(publishedPendingCreatedOrModifiedData_edts);

                        //Additionally, every EDT that is already published will need to be published again.
                        createdOrModifiedNows.AddRange(treeModel.AllEDTsViewModel.EDTs.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));

                    }
                    else
                    {
                        if (publishedPendingCreatedOrModifiedData_edts != null && PublicationTags != null)
                        { 
                            foreach (var item in publishedPendingCreatedOrModifiedData_edts)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedNows.Add(item);
                                }
                            }
                        }
                    }
                }
                return createdOrModifiedNows;
            }
        }

        public List<NowViewModel> ModifiedNows
        {
            get => CreatedOrModifiedNows.Where(x => x.Now.IsNewItem == null || !x.Now.IsNewItem.Value).ToList();
        }
        public List<NowViewModel> NewNows
        {
            get => CreatedOrModifiedNows.Where(x => x.Now.IsNewItem != null && x.Now.IsNewItem.Value).ToList();
        }


        #endregion

        #region now subscription data for patching
        private List<NowSubscriptionViewModel> deletedNowSubscriptions = null;
        private List<NowSubscriptionViewModel> createdOrModifiedNowSubscriptions = null;
        public List<NowSubscriptionViewModel> DeletedNowSubscriptions
        {
            get
            {
                if (deletedNowSubscriptions == null)
                {
                    deletedNowSubscriptions = new List<NowSubscriptionViewModel>();

                    var publishedPendingDeletedData_nows = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every NOW Subscription that is already published will need to be deleted.
                        deletedNowSubscriptions.AddRange(treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published));

                        //Additionally, every now subscription that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData_nows)
                        {
                            var matchedRevisionPending = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(matchedRevisionPending);
                        }

                        //Finally, every now subscription that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingNowSubs = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingNowSubs)
                        {
                            var matchedPublishedPending = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(item);
                        }
                    }
                    else
                    {
                        if (publishedPendingDeletedData_nows != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingDeletedData_nows)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }

                    var publishedPendingDeletedData_edts = treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every EDT Subscription that is already published will need to be deleted.
                        deletedNowSubscriptions.AddRange(treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published));

                        //Additionally, every now subscription that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData_nows)
                        {
                            var matchedRevisionPending = treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(matchedRevisionPending);
                        }

                        //Finally, every now subscription that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingNowSubs = treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingNowSubs)
                        {
                            var matchedPublishedPending = treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(item);
                        }
                    }
                    else
                    {
                        if (publishedPendingDeletedData_edts != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingDeletedData_edts)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }

                    var publishedPendingDeletedData_cr = treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Court Register Subscription that is already published will need to be deleted.
                        deletedNowSubscriptions.AddRange(treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published));

                        //Additionally, every Court Register subscription that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData_cr)
                        {
                            var matchedRevisionPending = treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(matchedRevisionPending);
                        }

                        //Finally, every Court Register subscription that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingSubs = treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingSubs)
                        {
                            var matchedPublishedPending = treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(item);
                        }
                    }
                    else
                    {
                        if (publishedPendingDeletedData_cr != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingDeletedData_cr)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }

                    var publishedPendingDeletedData_pcr = treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Prison Court Register Subscription that is already published will need to be deleted.
                        deletedNowSubscriptions.AddRange(treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published));

                        //Additionally, every Prison Court Register subscription that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData_cr)
                        {
                            var matchedRevisionPending = treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(matchedRevisionPending);
                        }

                        //Finally, every Prison Court Register subscription that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingSubs = treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingSubs)
                        {
                            var matchedPublishedPending = treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(item);
                        }
                    }
                    else
                    {
                        if (publishedPendingDeletedData_pcr != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingDeletedData_pcr)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }                    

                    var publishedPendingDeletedData_ir = treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Informant Register Subscription that is already published will need to be deleted.
                        deletedNowSubscriptions.AddRange(treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published));

                        //Additionally, every Informant subscription that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData_cr)
                        {
                            var matchedRevisionPending = treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(matchedRevisionPending);
                        }

                        //Finally, every Informant Register subscription that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingSubs = treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingSubs)
                        {
                            var matchedPublishedPending = treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedNowSubscriptions.Add(item);
                        }
                    }
                    else
                    {
                        if (publishedPendingDeletedData_ir != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingDeletedData_ir)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }                    
                }
                return deletedNowSubscriptions;
            }
        }
        public List<NowSubscriptionViewModel> CreatedOrModifiedNowSubscriptions
        {
            get
            {
                if (createdOrModifiedNowSubscriptions == null)
                {
                    createdOrModifiedNowSubscriptions = new List<NowSubscriptionViewModel>();
                    var publishedPendingCreatedOrModifiedData_nowSubs = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every NOW Subscription that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedNowSubscriptions.AddRange(publishedPendingCreatedOrModifiedData_nowSubs);

                        //Additionally, every NOW Subscription that is already published will need to be published again.
                        createdOrModifiedNowSubscriptions.AddRange(treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));

                    }
                    else
                    {
                        if (publishedPendingCreatedOrModifiedData_nowSubs != null && PublicationTags != null)
                        {                            
                            foreach (var item in publishedPendingCreatedOrModifiedData_nowSubs)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }

                    var publishedPendingCreatedOrModifiedData_edtSubs = treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every NOW Subscription that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedNowSubscriptions.AddRange(publishedPendingCreatedOrModifiedData_edtSubs);

                        //Additionally, every EDT Subscription that is already published will need to be published again.
                        createdOrModifiedNowSubscriptions.AddRange(treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));

                    }
                    else
                    {
                        if (publishedPendingCreatedOrModifiedData_edtSubs != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingCreatedOrModifiedData_edtSubs)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }

                    var publishedPendingCreatedOrModifiedData_cr = treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Court Register Subscription that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedNowSubscriptions.AddRange(publishedPendingCreatedOrModifiedData_cr);

                        //Additionally, every Court Register Subscription that is already published will need to be published again.
                        createdOrModifiedNowSubscriptions.AddRange(treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));
                    }
                    else
                    {
                        if (publishedPendingCreatedOrModifiedData_cr != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingCreatedOrModifiedData_cr)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }                    

                    var publishedPendingCreatedOrModifiedData_pcr = treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Prison Court Register Subscription that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedNowSubscriptions.AddRange(publishedPendingCreatedOrModifiedData_pcr);

                        //Additionally, every Prison Court Register Subscription that is already published will need to be published again.
                        createdOrModifiedNowSubscriptions.AddRange(treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));

                    }
                    else
                    {
                        if (publishedPendingCreatedOrModifiedData_pcr != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingCreatedOrModifiedData_pcr)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }                    

                    var publishedPendingCreatedOrModifiedData_ir = treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Informant Register Subscription that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedNowSubscriptions.AddRange(publishedPendingCreatedOrModifiedData_pcr);

                        //Additionally, every Informant Register Subscription that is already published will need to be published again.
                        createdOrModifiedNowSubscriptions.AddRange(treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));

                    }
                    else
                    {
                        if (publishedPendingCreatedOrModifiedData_ir != null && PublicationTags != null)
                        {
                            foreach (var item in publishedPendingCreatedOrModifiedData_ir)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedNowSubscriptions.Add(item);
                                }
                            }
                        }
                    }
                }

                //Append any toggled results i.e. include any published results that are subject to a feature toggle
                if (!IsFullExport)
                {
                    AppendGetToggledSubscriptions(createdOrModifiedNowSubscriptions);
                }

                return createdOrModifiedNowSubscriptions;
            }
        }

        private void AppendGetToggledSubscriptions(List<NowSubscriptionViewModel> createdOrModifiedData)
        {
            if (FeatureToggles == null || !FeatureToggles.Any())
            {
                return;
            }
            var toggledSubs = new List<NowSubscriptionViewModel>();
            var publishedPublishedPendingSubs = new List<NowSubscriptionViewModel>();
            publishedPublishedPendingSubs.AddRange(treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Where(x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) && x.DeletedDate == null));
            publishedPublishedPendingSubs.AddRange(treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Where(x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) && x.DeletedDate == null));
            publishedPublishedPendingSubs.AddRange(treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Where(x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) && x.DeletedDate == null));
            publishedPublishedPendingSubs.AddRange(treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) && x.DeletedDate == null));
            publishedPublishedPendingSubs.AddRange(treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Where(x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) && x.DeletedDate == null));

            //get published results for the enabled feature toggle, note we never append draft results or revisions
            foreach (var toggle in FeatureToggleCodes)
            {
                switch (toggle.ToLowerInvariant().Trim())
                {
                    case "cct-1399":
                        var res1399 = (from x in
                                publishedPublishedPendingSubs
                                    .Where(x => x.ApplySubscriptionRules && (x.ProsecutorMajorCreditor || x.NonProsecutorMajorCreditor || x.AnyMajorCreditor)
                                        // is a relevant subscription for this feature
                                        && !(
                                              from cm in createdOrModifiedData
                                              select cm.MasterUUID
                                            ).Contains(x.MasterUUID) // is not already included
                                        &&
                                           !(
                                              from t in toggledSubs
                                              select t.MasterUUID
                                            ).Contains(x.MasterUUID)) // is not already toggled in
                                       select x).ToList();
                        if (res1399.Any())
                        {
                            toggledSubs.AddRange(res1399);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (!toggledSubs.Any())
            {
                return;
            }
            createdOrModifiedData.AddRange(toggledSubs);
        }

        public List<NowSubscriptionViewModel> ModifiedNowSubscriptions
        {
            get => CreatedOrModifiedNowSubscriptions.Where(x => x.NowSubscription.IsNewItem == null || !x.NowSubscription.IsNewItem.Value).ToList();
        }

        public List<NowSubscriptionViewModel> NewNowSubscriptions
        {
            get => CreatedOrModifiedNowSubscriptions.Where(x => x.NowSubscription.IsNewItem != null && x.NowSubscription.IsNewItem.Value).ToList();
        }

        #endregion

        #region fixed list data for patching
        private List<FixedListViewModel> deletedFixedLists = null;
        private List<FixedListViewModel> createdOrModifiedFixedLists = null;
        
        public List<FixedListViewModel> DeletedFixedLists
        {
            get
            {
                if (deletedFixedLists == null)
                {                    
                    var publishedPendingDeletedData = treeModel.AllFixedListViewModel.FixedLists.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate != null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Fixed List that is already published will need to be deleted.
                        deletedFixedLists = treeModel.AllFixedListViewModel.FixedLists.Where(x => x.PublishedStatus == PublishedStatus.Published).ToList();

                        //Additionally, every Fixed List that is publishedPending and has a corresponding revisionPending will need to be deleted because by definition it is being amended
                        foreach (var item in publishedPendingDeletedData)
                        {
                            var matchedRevisionPending = treeModel.AllFixedListViewModel.FixedLists.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.RevisionPending && x.MasterUUID == item.MasterUUID);
                            deletedFixedLists.Add(matchedRevisionPending);
                        }

                        //Finally, every Fixed List that is revisionPending and does not have a corresponding publishedPending will need to be deleted because by definition it is being removed
                        var revisionPendingNows = treeModel.AllFixedListViewModel.FixedLists.Where(x => x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
                        foreach (var item in revisionPendingNows)
                        {
                            var matchedPublishedPending = treeModel.AllFixedListViewModel.FixedLists.FirstOrDefault(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.MasterUUID == item.MasterUUID);
                            deletedFixedLists.Add(item);
                        }
                    }
                    else
                    {
                        deletedFixedLists = new List<FixedListViewModel>();
                        if (publishedPendingDeletedData != null && PublicationTags != null)
                        {                           
                            foreach (var item in publishedPendingDeletedData)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    deletedFixedLists.Add(item);
                                }
                            }
                        }
                    }
                }
                return deletedFixedLists;
            }
        }
        public List<FixedListViewModel> CreatedOrModifiedFixedLists
        {
            get
            {
                if (createdOrModifiedFixedLists == null)
                {                    
                    var publishedPendingCreatedOrModifiedData = treeModel.AllFixedListViewModel.FixedLists.Where(x => x.PublishedStatus == PublishedStatus.PublishedPending && x.DeletedDate == null && x.PublicationTags != null).ToList();

                    if (IsFullExport)
                    {
                        //Every Fixed List that is publishedPending will need to be created because by definition it is being amended or created for the first time
                        createdOrModifiedFixedLists = publishedPendingCreatedOrModifiedData;

                        //Additionally, every Fixed List that is already published will need to be published again.
                        createdOrModifiedFixedLists.AddRange(treeModel.AllFixedListViewModel.FixedLists.Where(x => x.PublishedStatus == PublishedStatus.Published && x.DeletedDate == null));

                    }
                    else
                    {
                        createdOrModifiedFixedLists = new List<FixedListViewModel>();
                        if (publishedPendingCreatedOrModifiedData != null && PublicationTags != null)
                        {                            
                            foreach (var item in publishedPendingCreatedOrModifiedData)
                            {
                                if (item.PublicationTags != null && item.PublicationTags.Intersect(PublicationTags).Any())
                                {
                                    createdOrModifiedFixedLists.Add(item);
                                }
                            }
                        }
                    }
                }
                return createdOrModifiedFixedLists;
            }
        }

        public List<FixedListViewModel> ModifiedFixedLists
        {
            get => CreatedOrModifiedFixedLists.Where(x => x.FixedList.IsNewItem == null || !x.FixedList.IsNewItem.Value).ToList();
        }

        public List<FixedListViewModel> NewFixedLists
        {
            get => CreatedOrModifiedFixedLists.Where(x => x.FixedList.IsNewItem != null && x.FixedList.IsNewItem.Value).ToList();
        }

        #endregion

        #region Result Definition Synonyms for patching

        private List<DataLib.ResultDefinitionWordGroup> activeResultDefinitionSynonyms = null;

        public List<DataLib.ResultDefinitionWordGroup> ActiveResultDefinitionSynonyms
        {
            get
            {
                if (activeResultDefinitionSynonyms == null)
                {
                    var allPublishedItems = treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.FindAll(x => x.DeletedDate == null && x.Synonyms != null && x.Synonyms.Count > 0 && x.PublishedStatus == PublishedStatus.Published);
                    var publishedPendingItems = treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.FindAll(x => x.DeletedDate == null && x.Synonyms != null && x.Synonyms.Count > 0 && x.PublishedStatus == PublishedStatus.PublishedPending);
                    foreach (var pendingItem in publishedPendingItems)
                    {
                        allPublishedItems.RemoveAll(x => x.MasterUUID == pendingItem.MasterUUID);
                    }
                    allPublishedItems.AddRange(publishedPendingItems);
                    activeResultDefinitionSynonyms = allPublishedItems.Select(x => x.ResultDefinitionWordGroup).ToList();
                }
                return activeResultDefinitionSynonyms;
            }
        }
        #endregion

        #region Result Prompt Synonyms for patching

        private List<ResultPromptWordGroup> activeResultPromptSynonyms = null;

        public List<ResultPromptWordGroup> ActiveResultPromptSynonyms
        {
            get
            {
                if (activeResultPromptSynonyms == null)
                {
                    var allPublishedItems = treeModel.AllResultPromptWordGroupViewModel.WordGroups.FindAll(x => x.DeletedDate == null && x.Synonyms != null && x.Synonyms.Count > 0 && x.PublishedStatus == PublishedStatus.Published);
                    var publishedPendingItems = treeModel.AllResultPromptWordGroupViewModel.WordGroups.FindAll(x => x.DeletedDate == null && x.Synonyms != null && x.Synonyms.Count > 0 && x.PublishedStatus == PublishedStatus.PublishedPending);
                    foreach (var pendingItem in publishedPendingItems)
                    {
                        allPublishedItems.RemoveAll(x => x.MasterUUID == pendingItem.MasterUUID);
                    }
                    allPublishedItems.AddRange(publishedPendingItems);
                    activeResultPromptSynonyms = allPublishedItems.Select(x => x.ResultPromptWordGroup).ToList();
                }
                return activeResultPromptSynonyms;
            }
        }
        #endregion

        #region control dates

        public DateTime? StartDate(DateTime? inputDate)
        {
            return (inputDate == null || inputDate < PatchDate.Date) ? (DateTime?)null : inputDate.Value;
        }

        public DateTime? EndDate(DateTime? endDate, DateTime? startDate)
        {
            return endDate == null || startDate == null ? null : (endDate.Value < startDate ? null : endDate);
        }

        #endregion

    }
}