using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DataLib;

namespace Models.ViewModels
{
    public class NowRequirementPromptRuleTreeViewModel : TreeViewItemViewModel
    {
        NowRequirementPromptRuleViewModel nowRequirementPromptRuleVM;

        public NowRequirementPromptRuleTreeViewModel(ITreeModel treeModel, NowRequirementPromptRuleViewModel nowRequirementPromptRuleVM, TreeViewItemViewModel parent)
            : base(parent, true, nowRequirementPromptRuleVM.ResultPromptRule.ResultPromptViewModel.Label, nowRequirementPromptRuleVM.ResultPromptRule.ResultPromptViewModel.UUID.ToString())
        {
            this.treeModel = treeModel;
            this.nowRequirementPromptRuleVM = nowRequirementPromptRuleVM;
            //LoadChildren();
        }

        protected override void LoadChildren()
        {
            if (nowRequirementPromptRuleVM.ResultPromptRule != null)
            {
                Children.Add(new ResultPromptTreeViewModel(treeModel, nowRequirementPromptRuleVM.ResultPromptRule, this));
            }
        }

        #region NowRequirementPromptRuleViewModel Properties

        public NowRequirementPromptRuleViewModel NowRequirementPromptRuleViewModel
        {
            get => nowRequirementPromptRuleVM;
            set => nowRequirementPromptRuleVM = value; 
        }

        #endregion NowRequirementViewModel Properties
    }
}