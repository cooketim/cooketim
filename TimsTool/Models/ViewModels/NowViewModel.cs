using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DataLib;
using Models.Commands;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a Now object.
    /// </summary>
    public class NowViewModel : ViewModelBase
    {
        readonly AllNowRequirementsViewModel allNowRequirements;
        readonly AllResultDefinitionsViewModel allResultDefinitionsViewModel;

        public NowViewModel(ITreeModel treeModel, Now now, AllNowRequirementsViewModel allNowRequirements, AllResultDefinitionsViewModel allResultDefinitionsViewModel)
        {
            this.treeModel = treeModel;
            Now = now;
            this.allNowRequirements = allNowRequirements;
            this.allResultDefinitionsViewModel = allResultDefinitionsViewModel;
            LoadTextModel();
            LoadAssociatedModels();
        }
        private void LoadTextModel()
        {
            if (Now.NowTextList != null)
            {
                TextValues = new SilentObservableCollection<NowTextViewModel>(
                    (from x in Now.NowTextList
                     select new NowTextViewModel(x, this)).ToList());
            }
            else
            {
                TextValues = new SilentObservableCollection<NowTextViewModel>();
            }

            TextValues.CollectionChanged += textValues_CollectionChanged;
        }

        private void textValues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var nowTextViewModel = (NowTextViewModel)item;
                    if (Now.NowTextList == null)
                    {
                        Now.NowTextList = new List<NowText>() { nowTextViewModel.NowText };
                    }
                    else
                    {
                        Now.NowTextList.Add(nowTextViewModel.NowText);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var nowTextViewModel = (NowTextViewModel)item;
                    if (Now.NowTextList != null) { Now.NowTextList.Remove(nowTextViewModel.NowText); }
                }
            }

            OnPropertyChanged("TextValues");
        }

        public SilentObservableCollection<NowTextViewModel> TextValues { get; private set; }

        #region Now Text List CRUD

        NowTextViewModel selectedTextValue = null;
        public NowTextViewModel SelectedTextValue
        {
            get => selectedTextValue;
            set
            {
                SetProperty(ref selectedTextValue, value);
                OnPropertyChanged("SelectedTextValueVisibility");
            }
        }
        public bool SelectedTextValueVisibility
        {
            get { return selectedTextValue != null ? true : false; }
        }

        #endregion Now Text List CRUD

        #region Associated Models

        private void LoadAssociatedModels()
        {
            //only take the root now requirements
            NowRequirements = allNowRequirements.NowRequirements.Where(x => x.NOWUUID == Now.UUID && !x.IsChildRequirement).ToList();

            //set up the publication tag model
            PublicationTagsModel = new PublicationTagsModel(treeModel, this);

            //set up the comments model
            AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, this);
        }

        public PublicationTagsModel PublicationTagsModel { get; set; }

        public AssociatedCommentsModel AssociatedCommentsModel { get; set; }

        public List<NowRequirementViewModel> NowRequirements { get; set; }

        public List<NowRequirementViewModel> AllNowRequirements 
        { 
            get
            {
                var res = new List<NowRequirementViewModel>();

                if (NowRequirements != null)
                {
                    FlattenNowRequirements(NowRequirements, res);
                }

                return res;
            }
        }

        private void FlattenNowRequirements(List<NowRequirementViewModel> nowRequirements, List<NowRequirementViewModel> flattened)
        {
            if (nowRequirements == null) { return; }

            flattened.AddRange(nowRequirements);
            foreach(var nr in nowRequirements)
            {
                FlattenNowRequirements(nr.NowRequirements, flattened);
            }
        }

        #endregion Associated Models

        public Now Now
        {
            get { return data as Now; }
            private set { data = value; }
        }

        #region Now Properties

        public string Name
        {    
            get => Now.Name;
            set
                {
                    if (SetProperty(() => Now.Name == value, () => Now.Name = value))
                    {
                        LastModifiedDate = DateTime.Now;

                        //null out the welsh translation since this item is now required to be retranslated
                        WelshNowName = null;
                    }
                }
        }

        public bool IsStorageRequired
        {
            get => Now.IsStorageRequired;
            set
            {
                if (SetProperty(() => Now.IsStorageRequired == value, () => Now.IsStorageRequired = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeAllResults
        {
            get => Now.IncludeAllResults;
            set
            {
                if (SetProperty(() => Now.IncludeAllResults == value, () => Now.IncludeAllResults = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IsEDT
        {
            get => Now.IsEDT;
            set
            {
                if (SetProperty(() => Now.IsEDT == value, () => Now.IsEDT = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool Financial
        {
            get => Now.Financial;
            set
            {
                if (SetProperty(() => Now.Financial == value, () => Now.Financial = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string WelshNowName
        {
            get => Now.WelshNowName;
            set
                {
                    if (SetProperty(() => Now.WelshNowName == value, () => Now.WelshNowName = value))
                    {
                        LastModifiedDate = DateTime.Now;

                    }
                }
        }

        public string TemplateName
        {
            get => Now.TemplateName;
            set
                {
                    if (SetProperty(() => Now.TemplateName == value, () => Now.TemplateName = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string SubTemplateName
        {
            get => Now.SubTemplateName;
            set
                {
                    if (SetProperty(() => Now.SubTemplateName == value, () => Now.SubTemplateName = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string BilingualTemplateName
        {
            get => Now.BilingualTemplateName;
            set
                {
                    if (SetProperty(() => Now.BilingualTemplateName == value, () => Now.BilingualTemplateName = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string Jurisdiction
        {
            get => Now.Jurisdiction;
            set
                {
                    if (SetProperty(() => Now.Jurisdiction == value, () => Now.Jurisdiction = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }        

        public int? Rank
        {
            get => Now.Rank;
            set
                {
                    if (SetProperty(() => Now.Rank == value, () => Now.Rank = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public int? UrgentTimeLimitInMinutes
        {
            get => Now.UrgentTimeLimitInMinutes;
            set
            {
                if (SetProperty(() => Now.UrgentTimeLimitInMinutes == value, () => Now.UrgentTimeLimitInMinutes = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }
        
        public bool RemotePrintingRequired
        {
            get => Now.RemotePrintingRequired;
            set
                {
                    if (SetProperty(() => Now.RemotePrintingRequired == value, () => Now.RemotePrintingRequired = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool IncludeCaseMarkers
        {
            get => Now.IncludeCaseMarkers;
            set
            {
                if (SetProperty(() => Now.IncludeCaseMarkers == value, () => Now.IncludeCaseMarkers = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludePNCID
        {
            get => Now.IncludePNCID;
            set
            {
                if (SetProperty(() => Now.IncludePNCID == value, () => Now.IncludePNCID = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeConvictionStatus
        {
            get => Now.IncludeConvictionStatus;
            set
            {
                if (SetProperty(() => Now.IncludeConvictionStatus == value, () => Now.IncludeConvictionStatus = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeNationality
        {
            get => Now.IncludeNationality;
            set
            {
                if (SetProperty(() => Now.IncludeNationality == value, () => Now.IncludeNationality = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDefendantASN
        {
            get => Now.IncludeDefendantASN;
            set
            {
                if (SetProperty(() => Now.IncludeDefendantASN == value, () => Now.IncludeDefendantASN = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDefendantMobileNumber
        {
            get => Now.IncludeDefendantMobileNumber;
            set
            {
                if (SetProperty(() => Now.IncludeDefendantMobileNumber == value, () => Now.IncludeDefendantMobileNumber = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDefendantLandlineNumber
        {
            get => Now.IncludeDefendantLandlineNumber;
            set
            {
                if (SetProperty(() => Now.IncludeDefendantLandlineNumber == value, () => Now.IncludeDefendantLandlineNumber = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDefendantNINO
        {
            get => Now.IncludeDefendantNINO;
            set
            {
                if (SetProperty(() => Now.IncludeDefendantNINO == value, () => Now.IncludeDefendantNINO = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDefendantEthnicity
        {
            get => Now.IncludeDefendantEthnicity;
            set
            {
                if (SetProperty(() => Now.IncludeDefendantEthnicity == value, () => Now.IncludeDefendantEthnicity = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDefendantGender
        {
            get => Now.IncludeDefendantGender;
            set
            {
                if (SetProperty(() => Now.IncludeDefendantGender == value, () => Now.IncludeDefendantGender = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeSolicitorsNameAddress
        {
            get => Now.IncludeSolicitorsNameAddress;
            set
            {
                if (SetProperty(() => Now.IncludeSolicitorsNameAddress == value, () => Now.IncludeSolicitorsNameAddress = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDVLAOffenceCode
        {
            get => Now.IncludeDVLAOffenceCode;
            set
            {
                if (SetProperty(() => Now.IncludeDVLAOffenceCode == value, () => Now.IncludeDVLAOffenceCode = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeDriverNumber
        {
            get => Now.IncludeDriverNumber;
            set
            {
                if (SetProperty(() => Now.IncludeDriverNumber == value, () => Now.IncludeDriverNumber = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludePlea
        {
            get => Now.IncludePlea;
            set
            {
                if (SetProperty(() => Now.IncludePlea == value, () => Now.IncludePlea = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool IncludeVehicleRegistration
        {
            get => Now.IncludeVehicleRegistration;
            set
            {
                if (SetProperty(() => Now.IncludeVehicleRegistration == value, () => Now.IncludeVehicleRegistration = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool ApplyMDEOffenceFiltering
        {
            get => Now.ApplyMDEOffenceFiltering;
            set
            {
                if (SetProperty(() => Now.ApplyMDEOffenceFiltering == value, () => Now.ApplyMDEOffenceFiltering = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }        

        public string EmailTemplateName
        {
            get => Now.EmailTemplateName;
            set
            {
                if (SetProperty(() => Now.EmailTemplateName == value, () => Now.EmailTemplateName = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string BilingualEmailTemplateName
        {
            get => Now.BilingualEmailTemplateName;
            set
            {
                if (SetProperty(() => Now.BilingualEmailTemplateName == value, () => Now.BilingualEmailTemplateName = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public DateTime? StartDate
        {
            get => Now.StartDate;
            set
            {
                if (SetProperty(() => Now.StartDate == value, () => Now.StartDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public DateTime? EndDate
        {
            get => Now.EndDate;
            set
            {
                if (SetProperty(() => Now.EndDate == value, () => Now.EndDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #endregion // Now Properties

        #region Copy Members
        public NowViewModel Draft(List<string> publicationTags)
        {
            NowViewModel res = (NowViewModel)this.MemberwiseClone();

            //make new data
            var newData = res.Now.Draft();

            //set the publication tags and set the data on the view model
            newData.PublicationTags = publicationTags;
            res.Now = newData;

            //make view models
            MakeViewModels(res, true);

            return res;
        }

        public NowViewModel Copy()
        {
            NowViewModel res = (NowViewModel)this.MemberwiseClone();
            //make new data
            var newData = res.Now.Copy();
            res.Now = newData;
            MakeViewModels(res, false);
            return res;
        }

        /// <summary>
        /// Make now requirement view models for the collection of now requirements
        /// </summary>
        /// <param name="res"></param>
        /// <param name="newData"></param>
        /// <param name="withDrafts"></param>
        private void MakeViewModels(NowViewModel res, bool withDrafts)
        {
            if (withDrafts)
            {
                //rebuild the now requirement tree based on the root results for the now
                if (res.Now.NowRequirements != null)
                {
                    List<NowRequirementTreeItem> tree = new List<NowRequirementTreeItem>();
                    RecursivelyResetNowRequirements(res.Now.NowRequirements.Where(x => x.DeletedDate == null).Select(x => x.ResultDefinition).ToList(), res.Now.AllNowRequirements, res, null, null, null,tree);
                    foreach(var item in tree)
                    {
                        item.SetSequencing();
                    }
                }
            }
            else
            {
                RecursivelyMakeNowRequirementViewModels(null, res, res.Now.NowRequirements);
            }            

            //reset the publication tags model so that the model reflects the new parent view and also sets its event listeners according to the new publication status
            res.PublicationTagsModel = new PublicationTagsModel(treeModel, res);

            //reset the associated comments model so that the model reflects the new parent view and also sets its context model according to the new publication status
            res.AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, res);
        }

        private class NowRequirementTreeItem
        {
            public NowRequirementTreeItem(NowRequirementViewModel parentVM, NowViewModel parentNow)
            {
                if (parentVM != null)
                {
                    ParentVM = parentVM;
                    ParentVM.NowRequirements = new List<NowRequirementViewModel>();
                    ParentVM.NowRequirement.NowRequirements = new List<NowRequirement>();
                }
                if (parentNow != null)
                {
                    ParentNow = parentNow;
                    ParentNow.NowRequirements = new List<NowRequirementViewModel>();
                    ParentNow.Now.NowRequirements = new List<NowRequirement>();
                }
            }

            public NowRequirementViewModel ParentVM { get; private set; }
            public NowViewModel ParentNow { get; private set; }

            internal void AddChild(NowRequirementViewModel item)
            {
                if (ParentNow != null)
                {
                    ParentNow.NowRequirements.Add(item);
                    ParentNow.Now.NowRequirements.Add(item.NowRequirement);
                }
                else
                {
                    ParentVM.NowRequirements.Add(item);
                    ParentVM.NowRequirement.NowRequirements.Add(item.NowRequirement);
                }
            }

            internal void SetSequencing()
            {
                var vms = ParentNow == null ? ParentVM.NowRequirements : ParentNow.NowRequirements;
                var tochange = vms.Where(x => x.ResultDefinitionSequence == 0).ToList();
                foreach (var item in tochange)
                {
                    var max = vms.Select(x => x.ResultDefinitionSequence).Max();
                    item.ResultDefinitionSequence = max + 100;
                }
            }
        }

        private void RecursivelyResetNowRequirements(List<ResultDefinition> childResults, List<NowRequirement> allNowRequirementsForNow, NowViewModel now, NowRequirementViewModel parentNr, NowRequirement rootNr, ResultDefinition parentRd, List<NowRequirementTreeItem> tree)
        {
            NowRequirementTreeItem toUpdate = null;
            if (parentNr == null)
            {
                toUpdate = tree.FirstOrDefault(x => x.ParentNow != null);
                if (toUpdate == null)
                {
                    toUpdate = new NowRequirementTreeItem(null, now);                    
                }
            }
            else
            {
                toUpdate = tree.FirstOrDefault(x => x.ParentVM != null && x.ParentVM.UUID == parentNr.UUID);
                if (toUpdate == null)
                {
                    toUpdate = new NowRequirementTreeItem(parentNr, null);
                }
            }

            if (childResults != null)
            {
                foreach (var rdChild in childResults.Where(x => x.DeletedDate == null))
                {
                    var rdChildMasterId = rdChild.MasterUUID ?? rdChild.UUID;
                    //first match any existing now requirement
                    NowRequirement childNr = null;
                    if (parentRd == null)
                    {
                        childNr = allNowRequirementsForNow.Where(x => x.ParentNowRequirement == null).FirstOrDefault(x => (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) == rdChildMasterId);
                    }
                    else
                    {
                        var parentMasterId = parentRd.MasterUUID ?? parentRd.UUID;
                        if (rootNr != null)
                        {
                            var rootMasterId = rootNr.ResultDefinition.MasterUUID ?? rootNr.ResultDefinition.UUID;
                            childNr = allNowRequirementsForNow.Where(x => x.ParentNowRequirement != null && x.RootParentNowRequirement != null).FirstOrDefault(
                                    x =>
                                            (x.ParentNowRequirement.ResultDefinition.MasterUUID ?? x.ParentNowRequirement.ResultDefinition.UUID) == parentMasterId &&
                                            (x.RootParentNowRequirement.ResultDefinition.MasterUUID ?? x.RootParentNowRequirement.ResultDefinition.UUID) == rootMasterId &&
                                            (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) == rdChildMasterId
                                        );
                        }
                        else
                        {
                            childNr = allNowRequirementsForNow.Where(x => x.ParentNowRequirement != null).FirstOrDefault(
                                    x => 
                                            (x.ParentNowRequirement.ResultDefinition.MasterUUID ?? x.ParentNowRequirement.ResultDefinition.UUID) == parentMasterId &&
                                            (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) == rdChildMasterId
                                       );
                        }
                    }

                    //attempt to find a draft version of the child result in preference to the child result that already exists
                    var existingDrafts = allResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.MasterUUID == rdChildMasterId).OrderByDescending(x => x.CreatedDate).ToList();
                    ResultDefinition draftRd = null;
                    if (existingDrafts.Count > 0)
                    {
                        draftRd = existingDrafts.First().ResultDefinition;
                    }

                    if (draftRd != null && draftRd.DeletedDate != null)
                    {
                        //Draft result is Deleted, no childNR is required
                        continue;
                    }

                    if (childNr == null)
                    {
                        //create a new child now requirement without any children
                        var parentNrData = parentNr == null ? null : parentNr.NowRequirement;
                        var rootNrData = rootNr == null ? parentNrData : rootNr;
                        childNr = new NowRequirement(draftRd == null ? rdChild : draftRd, now.Now, parentNrData, rootNrData);
                    }
                    else
                    {
                        //when required reset the result prompt rules by setting the result to the child result
                        if (draftRd != null)
                        {
                            childNr.SetResultDefinitionAndPromptRules(draftRd);
                        }
                        childNr.ParentNowRequirement = parentNr == null ? null : parentNr.NowRequirement;
                        childNr.RootParentNowRequirement = rootNr;
                        childNr.NowRequirements = new List<NowRequirement>();
                    }

                    //create a new view model
                    var rdVM = allResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == childNr.ResultDefinition.UUID);
                    var newVM = new NowRequirementViewModel(treeModel, childNr, rdVM, now, allResultDefinitionsViewModel);
                    toUpdate.AddChild(newVM);
                    tree.Add(toUpdate);

                    //recursively reset now requirements
                    var nextChildResults = GetNextChildResults(draftRd == null ? rdChild : draftRd);
                    RecursivelyResetNowRequirements(nextChildResults,
                                                    allNowRequirementsForNow,
                                                    now,
                                                    newVM,
                                                    childNr.RootParentNowRequirement == null ? childNr : childNr.RootParentNowRequirement,
                                                    rdVM.ResultDefinition,
                                                    tree);
                }
            }
        }

        private List<ResultDefinition> GetNextChildResults(ResultDefinition rd)
        {
            if (rd.ResultDefinitionRules == null || !rd.ResultDefinitionRules.Any())
            {
                return null;
            }

            var res = new List<ResultDefinition>();
            foreach (var childId in rd.ResultDefinitionRules.Where(x=>x.DeletedDate == null).Select(x=>x.ChildResultDefinitionUUID))
            {
                var nextRd = allResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == childId);
                if (nextRd != null) { res.Add(nextRd.ResultDefinition); }
            }
            return res;
        }

        private void RecursivelyMakeNowRequirementViewModels(NowRequirementViewModel parentNr, NowViewModel now, List<NowRequirement> childNrs)
        {
            //make new view collections for the child requirements
            if (childNrs != null)
            {
                var newReqs = new List<NowRequirementViewModel>();
                foreach (var nrChild in childNrs)
                {
                    //find the published view model
                    var rdVM = allResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == nrChild.ResultDefinition.UUID);
                    var newVM = new NowRequirementViewModel(treeModel, nrChild, rdVM, now, allResultDefinitionsViewModel);
                    newReqs.Add(newVM);

                    //recursively make now requirement view nodels
                    RecursivelyMakeNowRequirementViewModels(newVM, now, nrChild.NowRequirements);
                }
                if (parentNr == null)
                {
                    now.NowRequirements = newReqs;
                }
                else
                {
                    parentNr.NowRequirements = newReqs;
                }
            }
        }

        //private void RecursivelyMakeNowRequirementViewModels(NowRequirementViewModel parentNr, NowViewModel now, List<NowRequirement> childNrs, bool withDrafts)
        //{
        //    //make new view collections for the child requirements
        //    if (childNrs != null)
        //    {
        //        var newReqs = new List<NowRequirementViewModel>();
        //        foreach (var nrChild in childNrs)
        //        {
        //            NowRequirementViewModel newVM = null;
        //            if (withDrafts)
        //            {
        //                var rdMasterId = nrChild.ResultDefinition.MasterUUID == null ? nrChild.ResultDefinition.UUID : nrChild.ResultDefinition.MasterUUID;
        //                //first determine if there is already a matching draft view model for the result definition
        //                var existingDrafts = allResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.MasterUUID == rdMasterId).OrderByDescending(x => x.CreatedDate).ToList();
        //                if (existingDrafts.Count > 0)
        //                {
        //                    nrChild.ResultDefinition = existingDrafts.First().ResultDefinition;
        //                    newVM = new NowRequirementViewModel(nrChild, existingDrafts.First(), now, allResultDefinitionsViewModel);
        //                }
        //            }
        //            if (newVM == null)
        //            {
        //                //find the published view model
        //                var rdVM = allResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == nrChild.ResultDefinition.UUID);
        //                newVM = new NowRequirementViewModel(nrChild, rdVM, now, allResultDefinitionsViewModel);
        //            }
        //            newReqs.Add(newVM);

        //            //recursively make now requirement view nodels
        //            RecursivelyMakeNowRequirementViewModels(newVM, now, nrChild.NowRequirements, withDrafts);
        //        }
        //        if (parentNr == null)
        //        {
        //            now.NowRequirements = newReqs;
        //        }
        //        else
        //        {
        //            parentNr.NowRequirements = newReqs;
        //        }
        //    }
        //}

        #endregion Copy Members

        public bool IsNowPublishedPending
        {
            get
            {
                if (IsDeleted) { return false; }
                return IsPublishedPending;
            }
        }

        public void ResetStatus()
        {
            OnPropertyChanged("IsNowPublishedPending");
        }
    }
}