using DataLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a NowRequirement object.
    /// </summary>
    public class NowRequirementViewModel : ViewModelBase
    {
        NowViewModel nowEDT;
        AllResultDefinitionsViewModel allResultDefinitionsViewModel;
        private CascadeNowTextProgress cascadeProgress;

        public NowRequirementViewModel(ITreeModel treeModel, NowRequirement nowRequirement, ResultDefinitionViewModel resultDefinition, NowViewModel nowEDT, AllResultDefinitionsViewModel allResultDefinitionsViewModel)
        {
            this.treeModel = treeModel;
            this.nowEDT = nowEDT;
            NowRequirement = nowRequirement;

            if (resultDefinition == null)
            {
                resultDefinition = allResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == nowRequirement.ResultDefinition.UUID);
            }
            SetResultDefinition(resultDefinition);
            this.allResultDefinitionsViewModel = allResultDefinitionsViewModel;
            
            LoadTextModel();
            LoadAssociatedModels();
        }

        private void LoadTextModel()
        {
            if (NowRequirement.NowRequirementTextList != null)
            {
                TextValues = new SilentObservableCollection<NowRequirementTextViewModel>(
                    from x in NowRequirement.NowRequirementTextList
                     select new NowRequirementTextViewModel(x, this)
                    );
            }
            else
            {
                TextValues = new SilentObservableCollection<NowRequirementTextViewModel>();
            }

            TextValues.CollectionChanged += textValues_CollectionChanged;
        }

        #region Associated Models

        private void LoadAssociatedModels()
        {
            //initiate the now requirement prompt rules
            if (NowRequirement.NowRequirementPromptRules != null)
            {
                NowRequirementPromptRules = new List<NowRequirementPromptRuleViewModel>();
                var toDelete = new List<NowRequirementPromptRule>();
                foreach (var nrpr in NowRequirement.NowRequirementPromptRules)
                {
                    ResultPromptRuleViewModel rprVM = ResultDefinition.Prompts.ToList().Find(x => x.ResultPromptRule.UUID == nrpr.ResultPromptRule.UUID);
                    if (rprVM == null)
                    {
                        var promptMatch = treeModel.AllData.ResultPromptRules.Find(x => x.UUID == nrpr.ResultPromptRule.UUID && x.DeletedDate == null && x.ResultPrompt.DeletedDate == null);
                        if (promptMatch == null)
                        {
                            toDelete.Add(nrpr);
                        }
                    }
                    else
                    {
                        var nrprVM = new NowRequirementPromptRuleViewModel(nrpr, rprVM, this, treeModel);
                        NowRequirementPromptRules.Add(nrprVM);
                    }
                }

                if (toDelete.Count>0)
                {
                    toDelete.ForEach(x => NowRequirement.NowRequirementPromptRules.Remove(x));
                }
            }
        }

        private ResultDefinitionViewModel resultDefinition;
        public ResultDefinitionViewModel ResultDefinition 
        {
            get => resultDefinition;
            set
            {
                if (value != null && (resultDefinition == null || resultDefinition.UUID != value.UUID))
                {
                    SetResultDefinition(value);
                    ResetPromptRules();
                    LastModifiedDate = DateTime.Now;
                    OnPropertyChanged("ResultDefinition");
                }
            }
        }

        private void ResetPromptRules()
        {
            var newRulesVM = new List<NowRequirementPromptRuleViewModel>();
            foreach (var prompt in resultDefinition.Prompts.Where(x => !x.IsDeleted).OrderBy(x => x.PromptSequence))
            {
                var matched = NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.MasterUUID == (prompt.ResultPromptRule.MasterUUID ?? prompt.ResultPromptRule.UUID));
                if (matched == null)
                {
                    var newRule = new NowRequirementPromptRule(prompt.ResultPromptRule, NowRequirement);
                    matched = new NowRequirementPromptRuleViewModel(newRule, prompt, this, treeModel);

                }
                newRulesVM.Add(matched);
            }
            NowRequirementPromptRules = newRulesVM;
            NowRequirement.NowRequirementPromptRules = newRulesVM.Select(x => x.NowRequirementPromptRule).ToList();
        }

        private void SetResultDefinition(ResultDefinitionViewModel value)
        {
            if (value == null) { return; }
            resultDefinition = value;
            NowRequirement.SetResultDefinitionAndPromptRules(value.ResultDefinition);

            //when required realign the NowRequirementPromptRules
            if (NowRequirementPromptRules != null && NowRequirementPromptRules.Any())
            {
                if (value.Prompts == null || !value.Prompts.Where(x=>!x.IsDeleted).Any())
                {
                    NowRequirementPromptRules.Clear();
                    NowRequirement.NowRequirementPromptRules.Clear();
                }
                else
                {
                    if (NowRequirement.NowRequirementPromptRules != null)
                    {
                        //iterate the data to match the view models
                        foreach (var nrpr in NowRequirement.NowRequirementPromptRules)
                        {
                            //ensure that there is a matching vm
                            var matchedVM = NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.MasterUUID == (nrpr.ResultPromptRule.MasterUUID ?? nrpr.ResultPromptRule.UUID));
                            if (matchedVM != null && matchedVM.ResultPromptRule.UUID != nrpr.ResultPromptRule.UUID)
                            {
                                matchedVM.ResultPromptRule.ResultPromptRule = nrpr.ResultPromptRule;
                            }
                        }
                    }                    
                }
            }
        }

        public NowViewModel NowEdtVM
        {
            get
            {
                if (nowEDT != null)
                {
                    return nowEDT;
                }

                //try now view model
                nowEDT = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.UUID == NowRequirement.NOWUUID);
                if (nowEDT != null)
                {
                    return nowEDT;
                }

                //must be an EDT
                nowEDT = treeModel.AllEDTsViewModel.EDTs.First(x => x.UUID == NowRequirement.NOWUUID);
                return nowEDT;
            }
        }

        public void SetNowRequirements()
        {
            //create child now requirement view models based on the now requirement data
            if (NowRequirement.NowRequirements != null && NowRequirement.NowRequirements.Where(x=>x.DeletedDate == null).Count()>0)
            {
                var reqs = new List<NowRequirementViewModel>();
                foreach(var nr in NowRequirement.NowRequirements.Where(x => x.DeletedDate == null))
                {
                    var vm = new NowRequirementViewModel(treeModel, nr, null, nowEDT, allResultDefinitionsViewModel);
                    reqs.Add(vm);
                    vm.SetNowRequirements();
                }
                NowRequirements = reqs;
            }
        }

        public List<NowRequirementViewModel> NowRequirements { get; set; }

        public List<NowRequirementPromptRuleViewModel> NowRequirementPromptRules { get; set; }

        #endregion Associated Models

        private void textValues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var nowRequirementTextViewModel = (NowRequirementTextViewModel)item;
                    if (NowRequirement.NowRequirementTextList == null)
                    {
                        NowRequirement.NowRequirementTextList = new List<NowRequirementText>() { nowRequirementTextViewModel.NowRequirementText };
                    }
                    else
                    {
                        NowRequirement.NowRequirementTextList.Add(nowRequirementTextViewModel.NowRequirementText);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var nowRequirementTextViewModel = (NowRequirementTextViewModel)item;
                    if (NowRequirement.NowRequirementTextList != null) { NowRequirement.NowRequirementTextList.Remove(nowRequirementTextViewModel.NowRequirementText); }
                }
            }

            OnPropertyChanged("TextValues");
        }

        #region NowRequirement Text List CRUD

        NowRequirementTextViewModel selectedTextValue = null;
        public NowRequirementTextViewModel SelectedTextValue
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

        #endregion NowRequirement Text List CRUD

        public NowRequirement NowRequirement
        {
            get { return data as NowRequirement; }
            private set { data = value; }
        }

        public bool MandatoryVisibility
        {
            // mandatory setting are thought to no longer be required, soft removal by always making them invisible
            //get => IsChildRequirement ? false : true;
            get => false;
        }

        public bool PrimaryVisibility
        {
            get
            {
                if (!IsChildRequirement) { return true; }

                //allow children that are always published or are published for NOWs
                if (NowRequirement.ResultDefinition.IsAlwaysPublished || NowRequirement.ResultDefinition.IsPublishedForNows)
                {
                    return true;
                }

                //allow children that are always published but only when the parent is also published
                if (NowRequirement.ResultDefinition.IsPublishedAsAPrompt)
                {
                    if (NowRequirement.ParentNowRequirement.ParentNowRequirement == null ||
                        NowRequirement.ParentNowRequirement.ResultDefinition.IsAlwaysPublished || 
                        NowRequirement.ParentNowRequirement.ResultDefinition.IsPublishedForNows || 
                        NowRequirement.ParentNowRequirement.ResultDefinition.IsRollupPrompts)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool NowTextVisibility
        {
            get
            {
                if (NowRequirement.ResultDefinition.IsAlwaysPublished || NowRequirement.ResultDefinition.IsPublishedAsAPrompt || NowRequirement.ResultDefinition.IsPublishedForNows)
                {
                    return true;
                }

                if(NowRequirement.ResultDefinition.IsRollupPrompts)
                {
                    //when not a child requirement text should be available
                    if (!IsChildRequirement)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool ExcludeFromMDEVisibility
        {
            get
            {
                if (ParentNowRequirement == null && !PrimaryResult)
                {
                    return true;
                }

                return false;
            }
        }

        public void ResultPublishingRulesChanged()
        {
            if (NowRequirement.ResultDefinition.IsExcludedFromResults)
            {
                ClearExistingText();
                OnPropertyChanged("NowTextVisibility");
                return;
            }

            if(!NowRequirement.ResultDefinition.IsAlwaysPublished && !NowRequirement.ResultDefinition.IsPublishedAsAPrompt && !NowRequirement.ResultDefinition.IsPublishedForNows)
            {
                if (IsChildRequirement)
                {
                    //child requirement with nothing set for publishing rules
                    ClearExistingText();
                    OnPropertyChanged("NowTextVisibility");

                    //ensure that the now requirement is not primary
                    PrimaryResult = false;
                    return;
                }
            }
            OnPropertyChanged("NowTextVisibility");
        }

        private void ClearExistingText()
        {
            //now text is not visible, ensure that there is not any existing text
            this.TextValues.Clear();
            if (NowRequirement.NowRequirementTextList != null)
            {
                NowRequirement.NowRequirementTextList.Clear();
            }
            selectedTextValue = null;
        }

        public bool IsChildRequirement
        {
            get
            {
                return NowRequirement != null && NowRequirement.ParentNowRequirement != null;
            }
        }

        #region NowRequirement Properties

        public Guid? NOWUUID
        {
            get => NowRequirement.NOWUUID;
        }

        public bool ExcludeFromMDE
        {    
            get => NowRequirement.ExcludeFromMDE;
            set
                {
                    if (SetProperty(() => NowRequirement.ExcludeFromMDE == value, () => NowRequirement.ExcludeFromMDE = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool Mandatory
        {
            get => NowRequirement.Mandatory;
            set
            {
                if (SetProperty(() => NowRequirement.Mandatory == value, () => NowRequirement.Mandatory = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public bool PrimaryResult
        {
            get => NowRequirement.PrimaryResult;
            set
                {
                    if (SetProperty(() => NowRequirement.PrimaryResult == value, () => NowRequirement.PrimaryResult = value))
                    {
                        LastModifiedDate = DateTime.Now;

                        //cannot set a primary now requirement result to be excluded from MDE
                        if (value)
                        {
                            NowRequirement.ExcludeFromMDE = false;
                        }

                        OnPropertyChanged("ExcludeFromMDE");
                        OnPropertyChanged("ExcludeFromMDEVisibility");
                    }
                }
        }

        public bool DistinctResultTypes
        {
            get => NowRequirement.DistinctResultTypes;
            set
            {
                if (SetProperty(() => NowRequirement.DistinctResultTypes == value, () => NowRequirement.DistinctResultTypes = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public int? ResultDefinitionSequence
        {
            get => NowRequirement.ResultDefinitionSequence;
            set
                {
                    if (SetProperty(() => NowRequirement.ResultDefinitionSequence == value, () => NowRequirement.ResultDefinitionSequence = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public override DateTime? LastModifiedDate
        {
            get => data.LastModifiedDate;
            set
            {
                base.LastModifiedDate = value;
                if (NowEdtVM != null) { NowEdtVM.LastModifiedDate = value; }
                
            }
        }

        public SilentObservableCollection<NowRequirementTextViewModel> TextValues { get; private set; }

        public Guid? ParentNowRequirementUUID
        {
            get => NowRequirement.ParentNowRequirementUUID;
        }

        public Guid? RootParentNowRequirementUUID
        {
            get => NowRequirement.RootParentNowRequirementUUID;
        }

        public NowRequirement ParentNowRequirement
        {
            get => NowRequirement.ParentNowRequirement;
        }

        #region Copy Members
        public NowRequirementViewModel Draft(NowViewModel now, NowRequirement parentNr)
        {
            NowRequirementViewModel res = (NowRequirementViewModel)this.MemberwiseClone();
            //make new data with a complete child hierachy
            var newData = res.NowRequirement.Draft(now.Now, parentNr);
            res.NowRequirement = newData;
            RecursivelyMakeNowRequirementViewModels(res, now, newData.NowRequirements, true);

            return res;
        }

        public NowRequirementViewModel Copy(NowViewModel now, NowRequirement parentNr)
        {
            NowRequirementViewModel res = (NowRequirementViewModel)this.MemberwiseClone();
            //make new data with a complete child hierachy
            var newData = res.NowRequirement.Copy(now.Now, parentNr);
            res.NowRequirement = newData;
            RecursivelyMakeNowRequirementViewModels(res, now, newData.NowRequirements, false);

            return res;
        }

        private void RecursivelyMakeNowRequirementViewModels(NowRequirementViewModel parentNr, NowViewModel now, List<NowRequirement> childNrs, bool withDrafts)
        {
            //make new view collections for the child requirements
            if (childNrs != null)
            {
                var newReqs = new List<NowRequirementViewModel>();
                foreach (var nrChild in childNrs)
                {
                    NowRequirementViewModel newVM = null;
                    if (withDrafts)
                    {
                        //first determine if there is already a matching draft view model for the result definition
                        var existingDrafts = allResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.MasterUUID == nrChild.ResultDefinition.MasterUUID).OrderByDescending(x => x.CreatedDate).ToList();
                        if (existingDrafts.Count > 0)
                        {
                            nrChild.SetResultDefinitionAndPromptRules(existingDrafts.First().ResultDefinition);
                            newVM = new NowRequirementViewModel(treeModel, nrChild, existingDrafts.First(), now, allResultDefinitionsViewModel);
                        }
                    }
                    if (newVM == null)
                    {
                        //find the published view model
                        var rdVM = allResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == nrChild.ResultDefinition.UUID);
                        newVM = new NowRequirementViewModel(treeModel, nrChild, rdVM, now, allResultDefinitionsViewModel);
                    }
                    newReqs.Add(newVM);

                    //recursively make now requirement view nodels
                    RecursivelyMakeNowRequirementViewModels(newVM, now, nrChild.NowRequirements, withDrafts);
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

        #endregion Copy Members

        public NowRequirement RootParentNowRequirement
        {
            get => NowRequirement.RootParentNowRequirement;
        }

        public bool IsNowRequirementPublishedPending
        {
            get
            {
                if (IsDeleted) { return false; }
                return IsPublishedPending;
            }
        }

        #endregion // NowRequirement Properties


        #region cascade Now Text Properties

        public CascadeNowTextProgress CascadeNowTextProgress
        {
            set
            {
                cascadeProgress = value;
                OnPropertyChanged("CascadeProgressPercentage");
                OnPropertyChanged("CascadeProgressText");
            }
        }

        public int? CascadeProgressPercentage
        {
            get => cascadeProgress == null ? null : (int?)cascadeProgress.Percentage;
        }

        public string CascadeProgressText
        {
            get => cascadeProgress == null ? null : cascadeProgress.ProgressText;
        }

        #endregion
    }
}