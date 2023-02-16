using DataLib;
using Identity;
using System;
using System.Linq;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a ResultDefinition Rule object.
    /// </summary>
    public class ResultRuleViewModel : ViewModelBase
    {
        private ResultDefinitionViewModel childResultDefinitionViewModel;
        public ResultRuleViewModel(ITreeModel treeModel, ResultDefinitionViewModel childResultDefinitionViewModel, ResultDefinitionViewModel parentResultDefinitionViewModel, ResultDefinitionRule resultDefinitionRule=null)
        {
            this.treeModel = treeModel;
            ParentResultDefinitionViewModel = parentResultDefinitionViewModel;
            this.childResultDefinitionViewModel = childResultDefinitionViewModel;
            ResultDefinitionRule = resultDefinitionRule == null ? 
                    new ResultDefinitionRule
                        (
                            parentResultDefinitionViewModel == null ? null : parentResultDefinitionViewModel.UUID,
                            childResultDefinitionViewModel.ResultDefinition,
                            ResultDefinitionRuleType.atleastOneOf
                        ) 
                            { 
                                PublishedStatus = childResultDefinitionViewModel.ResultDefinition.PublishedStatus,
                                PublishedStatusDate = childResultDefinitionViewModel.ResultDefinition.PublishedStatusDate
                            } 
                    : resultDefinitionRule;
        }

        public void ResetDeleted()
        {
            OnPropertyChanged("IsReadOnly");
            OnPropertyChanged("IsEnabled");
            OnPropertyChanged("IsDeleted");
            OnPropertyChanged("IsResultDefinitionGroupDeleted");
            OnPropertyChanged("IsStandaloneResultDefinitionDeleted");
            OnPropertyChanged("IsResultDefinitionGroup");
        }

        /// <summary>
        /// For partial build of the rules before all result definition view models are built 
        /// </summary>
        /// <param name="resultDefinitionRule"></param>
        public ResultRuleViewModel(ITreeModel treeModel, ResultDefinitionRule resultDefinitionRule, ResultDefinitionViewModel parentResultDefinitionViewModel)
        {
            this.treeModel = treeModel;
            ParentResultDefinitionViewModel = parentResultDefinitionViewModel;
            ResultDefinitionRule = resultDefinitionRule;
        }        


        #region ResultDefinitionRule Properties

        public ResultDefinitionViewModel ChildResultDefinitionViewModel 
        {
            get => childResultDefinitionViewModel;
            set
            {
                //for initial set up
                if (childResultDefinitionViewModel == null)
                {
                    childResultDefinitionViewModel = value;
                    return;
                }

                if (childResultDefinitionViewModel != null && childResultDefinitionViewModel.UUID != value.UUID)
                {
                    LastModifiedDate = DateTime.Now;
                    childResultDefinitionViewModel = value;
                    ResultDefinitionRule.ResultDefinition = ChildResultDefinitionViewModel.ResultDefinition;
                }
            }
        }

        public ResultDefinitionViewModel ParentResultDefinitionViewModel { get; private set; }

        public ResultDefinitionRule ResultDefinitionRule
        {
            get { return data as ResultDefinitionRule; }
            private set { data = value; }
        }

        public ResultDefinitionRuleType? RuleType
        {
            get => ResultDefinitionRule.Rule;
            set
            {
                if (SetProperty(() => ResultDefinitionRule.Rule == value, () => ResultDefinitionRule.Rule = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public ResultDefinitionRuleType? RuleTypeMags
        {
            get => ResultDefinitionRule.RuleMags ?? ResultDefinitionRule.Rule;
            set
            {
                //when the value is the same as crown, set the common rule and remove the separate rules
                if (RuleTypeCrown == value)
                {
                    ResultDefinitionRule.Rule = value;
                    ResultDefinitionRule.RuleCrown = null;
                    ResultDefinitionRule.RuleMags = null;
                    OnPropertyChanged("Rule");
                    OnPropertyChanged("RuleCrown");
                    OnPropertyChanged("RuleMags");
                    LastModifiedDate = DateTime.Now;
                    return;
                }

                //value is different to crown, set separate values and set the common value to null
                ResultDefinitionRule.RuleMags = value;
                if (ResultDefinitionRule.RuleCrown == null)
                {
                    ResultDefinitionRule.RuleCrown = ResultDefinitionRule.Rule;
                    ResultDefinitionRule.Rule = null;
                    OnPropertyChanged("Rule");
                    OnPropertyChanged("RuleCrown");
                }

                OnPropertyChanged("RuleMags");
                LastModifiedDate = DateTime.Now;
            }
        }

        public ResultDefinitionRuleType? RuleTypeCrown
        {
            get => ResultDefinitionRule.RuleCrown ?? ResultDefinitionRule.Rule;
            set
            {
                //when the value is the same as mags, set the common rule and remove the separate rules
                if (RuleTypeMags == value)
                {
                    ResultDefinitionRule.Rule = value;
                    ResultDefinitionRule.RuleCrown = null;
                    ResultDefinitionRule.RuleMags = null;
                    OnPropertyChanged("Rule");
                    OnPropertyChanged("RuleCrown");
                    OnPropertyChanged("RuleMags");
                    LastModifiedDate = DateTime.Now;
                    return;
                }

                //value is different to mags, set separate values and set the common value to null
                ResultDefinitionRule.RuleCrown = value;
                if (ResultDefinitionRule.RuleMags == null)
                {
                    ResultDefinitionRule.RuleMags = ResultDefinitionRule.Rule;
                    ResultDefinitionRule.Rule = null;
                    OnPropertyChanged("Rule");
                    OnPropertyChanged("RuleMags");
                }

                OnPropertyChanged("RuleCrown");
                LastModifiedDate = DateTime.Now;
            }
        }

        public Guid? ParentUUID
        {
            get => ResultDefinitionRule.ParentUUID;
            set
            {
                if (SetProperty(() => ResultDefinitionRule.ParentUUID == value, () => ResultDefinitionRule.ParentUUID = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string DeleteHeader
        {
            get => ResultDefinitionRule.ParentUUID == null ? "Delete Draft Result Definition" : "Remove From Parent Result";
        }

        public DateTime? StartDate
        {
            get => ResultDefinitionRule.StartDate;
            set
            {
                if (SetProperty(() => ResultDefinitionRule.StartDate == value, () => ResultDefinitionRule.StartDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public DateTime? EndDate
        {
            get => ResultDefinitionRule.EndDate;
            set
            {
                if (SetProperty(() => ResultDefinitionRule.EndDate == value, () => ResultDefinitionRule.EndDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Fire property changed to ensure that the UI is refreshed
        /// </summary>
        public override DateTime? LastModifiedDate
        {
            get => base.LastModifiedDate;
            set
            {
                base.LastModifiedDate = value;

                //Set the parent result definition as modified so that the data patches for the rule change
                SetParentModified(value);
            }
        }

        /// <summary>
        /// Deleted dates are set by delete commands rather than UI actions
        /// </summary>
        public override DateTime? DeletedDate
        {
            get => base.DeletedDate;
            set
            {
                if (SetProperty(() => ResultDefinitionRule.DeletedDate == value, () => ResultDefinitionRule.DeletedDate = value))
                {
                    OnPropertyChanged("IsDeleted");
                    OnPropertyChanged("IsResultDefinitionGroupDeleted");
                    OnPropertyChanged("IsStandaloneResultDefinitionDeleted");
                    OnPropertyChanged("IsResultDefinitionGroup");

                    if (value == null) { DeletedUser = null; }
                    else { DeletedUser = IdentityHelper.SignedInUser.Email; }

                    //Set the parent result definition as modified so that the data patches for the rule change
                    SetParentModified(value);
                }
            }
        }

        private void SetParentModified(DateTime? value)
        {
            if (ParentResultDefinitionViewModel == null && ResultDefinitionRule.ParentUUID != null)
            {
                ParentResultDefinitionViewModel = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == ParentUUID);
            }

            if (ParentResultDefinitionViewModel != null)
            {
                ParentResultDefinitionViewModel.LastModifiedDate = value;
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public override bool IsDeleted
        {
            get
            {
                if (ResultDefinitionRule.ParentUUID == null)
                {
                    return ChildResultDefinitionViewModel.DeletedDate != null;
                }
                return DeletedDate != null;
            }
        }

        public bool IsResultDefinitionGroup
        {
            get
            {
                if (ChildResultDefinitionViewModel.IsBooleanResult) { return false; }
                if (ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules != null && ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null).Count() > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsResultDefinitionGroupDeleted
        {
            get
            {
                if (ChildResultDefinitionViewModel.IsBooleanResult) { return false; }
                if (IsDeleted && ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules != null && ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Count() > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsStandaloneResultDefinitionDeleted
        {
            get
            {
                if (ChildResultDefinitionViewModel.IsBooleanResult) { return false; }
                if (IsDeleted && (ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules == null || ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Count() == 0))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsResultDefinitionGroupPublishedPending
        {
            get
            {
                if (ChildResultDefinitionViewModel.IsBooleanResult) { return false; }
                if (IsDeleted) { return false; }
                if (!ChildResultDefinitionViewModel.IsPublishedPending) { return false; }
                if (ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules != null && ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Count() > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsStandaloneResultDefinitionPublishedPending
        {
            get
            {
                if (ChildResultDefinitionViewModel.IsBooleanResult) { return false; }
                if (IsDeleted) { return false; }
                if (!ChildResultDefinitionViewModel.IsPublishedPending) { return false; }
                if (ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules == null || ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Count() == 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsBooleanResultNotPublishedPending
        {
            get
            {
                if (!ChildResultDefinitionViewModel.IsBooleanResult) { return false; }
                if (IsDeleted) { return false; }
                if (!ChildResultDefinitionViewModel.IsPublishedPending) { return true; }
                return false;
            }
        }

        public bool IsBooleanResultPublishedPending
        {
            get
            {
                if (!ChildResultDefinitionViewModel.IsBooleanResult) { return false; }
                if (IsDeleted) { return false; }
                if (!ChildResultDefinitionViewModel.IsPublishedPending) { return false; }
                return true;
            }
        }

        public void ResetStatus()
        {
            OnPropertyChanged("IsResultDefinitionGroupDeleted");
            OnPropertyChanged("IsResultDefinitionGroupPublishedPending");
            OnPropertyChanged("IsStandaloneResultDefinitionDeleted");
            OnPropertyChanged("IsStandaloneResultDefinitionPublishedPending");
            OnPropertyChanged("IsBooleanResultNotPublishedPending");
            OnPropertyChanged("IsBooleanResultPublishedPending");
            OnPropertyChanged("IsResultDefinitionGroup");
        }

        #endregion ResultDefinitionRule Properties

        public void SetDraftResult(ResultDefinitionViewModel draft)
        {
            if(ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft ||
               ChildResultDefinitionViewModel.MasterUUID != draft.MasterUUID ||
               ChildResultDefinitionViewModel.UUID == draft.UUID)
            {
                return;
            }
            this.ChildResultDefinitionViewModel = draft;
            this.OnPropertyChanged("ResultDefinitionViewModel");
        }
    }
}