using DataLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AllResultPromptViewModel : ViewModelBase
    {

        public AllResultPromptViewModel(ITreeModel treeModel,List<ResultPromptRule> resultPromptRules, AllResultPromptWordGroupViewModel allWordGroups, List<string> allUserGroups)
        {
            var allPrompts = new List<ResultPromptViewModel>();
            Prompts = new List<ResultPromptRuleViewModel>(
                 from x in resultPromptRules
                 select new ResultPromptRuleViewModel(treeModel, x, allUserGroups, allWordGroups, allPrompts));

        }

        public List<ResultPromptRuleViewModel> Prompts
        {
            get;
        }

        public List<ResultPromptRuleViewModel> GetOtherParents(ResultPromptTreeViewModel child)
        {          
            return Prompts.FindAll(x => x.DeletedDate == null && x.ResultPromptViewModel.UUID == child.ResultPromptRuleViewModel.ResultPromptViewModel.UUID && x.ResultPromptRule.ResultDefinitionUUID != null && x.ResultPromptRule.ResultDefinitionUUID != child.ResultPromptRuleViewModel.ResultPromptRule.ResultDefinitionUUID);
        }

        internal List<ResultPromptRuleViewModel> GetOtherParents(ResultPromptWordGroupTreeViewModel child)
        {
            return Prompts.FindAll(x => x.DeletedDate == null && x.ResultPromptViewModel.UUID != child.ParentUUID && x.ResultPromptViewModel.WordGroups != null && x.ResultPromptViewModel.WordGroups.FirstOrDefault(y => y.ResultPromptWord == child.ResultPromptWordGroupViewModel.ResultPromptWord) != null);
        }

        public List<ResultPromptRuleViewModel> GetOtherParents(FixedListTreeViewModel child)
        {
            return Prompts.FindAll(x => x.DeletedDate == null && x.ResultPromptViewModel.UUID != child.ParentUUID && x.ResultPromptViewModel.FixedList != null && x.ResultPromptViewModel.FixedList.UUID == child.FixedListViewModel.UUID);
        }
    }
}
