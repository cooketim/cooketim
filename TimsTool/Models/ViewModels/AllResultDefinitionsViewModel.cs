using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using DataLib;

namespace Models.ViewModels
{
    public class AllResultDefinitionsViewModel : ViewModelBase
    {
        readonly AllResultPromptViewModel allPrompts;
        readonly AllResultRuleViewModel allResultRuleViewModel;

        public AllResultDefinitionsViewModel(ITreeModel treeModel, AllResultRuleViewModel allResultRuleViewModel, List<ResultDefinition> resultDefinitions, AllResultPromptViewModel allPrompts, AllResultDefinitionWordGroupViewModel allWordGroups, List<string> allUserGroups)
        {
            this.treeModel = treeModel;
            this.allResultRuleViewModel = allResultRuleViewModel;

            //result definition roots
            Definitions = new List<ResultDefinitionViewModel>(
                 (from x in resultDefinitions
                 select new ResultDefinitionViewModel(treeModel, x, allPrompts, allWordGroups, allUserGroups, allResultRuleViewModel)));

            //set the child result definitions i.e. the rules
            var parentDefinitions = Definitions.FindAll(x => x.Rules != null && x.Rules.Count > 0);
            foreach(var parent in parentDefinitions)
            {
                foreach (var rule in parent.Rules)
                {
                    //find the resultDefinition view model for the rule
                    var resultDefinitionVM = Definitions.FirstOrDefault(x => x.UUID == rule.ResultDefinitionRule.ChildResultDefinitionUUID);
                    rule.ChildResultDefinitionViewModel = resultDefinitionVM;
                }
            }

            this.allPrompts = allPrompts;     
        }

        public List<ResultDefinitionViewModel> Definitions { get; }

        public List<ResultDefinitionViewModel> GetOtherParents(ResultDefinitionWordGroupTreeViewModel child)
        {
            return Definitions.FindAll(x => x.UUID != child.ParentUUID && x.WordGroups != null && x.WordGroups.FirstOrDefault(y => y.ResultDefinitionWordGroups.FirstOrDefault(z=>z.UUID == child.ResultDefinitionWordGroupViewModel.UUID) != null) != null);
        }

        public List<ResultDefinitionViewModel> GetOtherParents(ResultDefinitionWordGroupViewModel child, Guid? parentResultDefinition)
        {
            return Definitions.FindAll(x => x.UUID != parentResultDefinition && x.WordGroups != null && x.WordGroups.FirstOrDefault(y => y.ResultDefinitionWordGroups.FirstOrDefault(z => z.UUID == child.UUID) != null) != null);
        }
    }
}
