using DataLib;
using System;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a NowRequirementPromptRule object.
    /// </summary>
    public class NowRequirementPromptRuleViewModel : ViewModelBase
    {
        public NowRequirementPromptRuleViewModel(NowRequirementPromptRule nowRequirementPromptRule, ResultPromptRuleViewModel resultPromptRule, 
            NowRequirementViewModel parentNowRequirement, ITreeModel treeModel)
        {
            this.treeModel = treeModel;
            NowRequirementPromptRule = nowRequirementPromptRule;
            ResultPromptRule = resultPromptRule;
            ParentNowRequirement = parentNowRequirement;
        }

        public ResultPromptRuleViewModel ResultPromptRule { get; }

        public NowRequirementViewModel ParentNowRequirement
        {
            get;
            private set;
        }


        public NowRequirementPromptRule NowRequirementPromptRule
        {
            get { return data as NowRequirementPromptRule; }
            private set { data = value; }
        }

        #region NowRequirementPromptRule Properties        

        public bool IsVariantData
        {
            get => NowRequirementPromptRule.IsVariantData;
            set
                {
                    if (SetProperty(() => NowRequirementPromptRule.IsVariantData == value, () => NowRequirementPromptRule.IsVariantData = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public bool DistinctPromptTypes
        {
            get => NowRequirementPromptRule.DistinctPromptTypes;
            set
            {
                if (SetProperty(() => NowRequirementPromptRule.DistinctPromptTypes == value, () => NowRequirementPromptRule.DistinctPromptTypes = value))
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

                //Also set the parent as modified
                SetProperty(() => ParentNowRequirement.LastModifiedDate == value, () => ParentNowRequirement.LastModifiedDate = value);
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public override DateTime? DeletedDate
        {
            get => base.DeletedDate;
            set
            {
                base.DeletedDate = value;

                //Also set the parent as modified
                SetProperty(() => ParentNowRequirement.LastModifiedDate == value, () => ParentNowRequirement.LastModifiedDate = value);
            }
        }

        public bool IsNowRequirementPromptRulePublishedPending
        {
            get
            {
                if (IsDeleted) { return false; }
                return IsPublishedPending;
            }
        }

        #endregion // NowRequirement Properties

    }
}