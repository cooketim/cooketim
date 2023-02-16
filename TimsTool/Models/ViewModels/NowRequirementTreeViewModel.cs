using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DataLib;

namespace Models.ViewModels
{
    public class NowRequirementTreeViewModel : TreeViewItemViewModel
    {
        public NowRequirementTreeViewModel(ITreeModel treeModel, NowRequirementViewModel nowRequirementVM, TreeViewItemViewModel parent)
            : base(parent, true, nowRequirementVM.ResultDefinition.Label, nowRequirementVM.ResultDefinition.UUID.ToString())
        {
            this.treeModel = treeModel;
            NowRequirementViewModel = nowRequirementVM;
            //LoadChildren();
        }

        protected override void LoadChildren()
        {
            if (NowRequirementViewModel.ResultDefinition != null)
            {
                //look for the root treeview item for the result definition, i.e. draft, published or historical revision
                var root = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.First(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == NowRequirementViewModel.ResultDefinition.UUID);
                Children.Add(root);
            }

            if (NowRequirementViewModel.NowRequirementPromptRules != null)
            {
                var toProcess = NowRequirementViewModel.NowRequirementPromptRules.Where(x=> x.DeletedDate == null && x.ResultPromptRule.PromptSequence != null).OrderBy(x => x.ResultPromptRule.PromptSequence).ToList();
                if (NowRequirementViewModel.NowRequirementPromptRules.Where(x=> x.ResultPromptRule.PromptSequence == null).Count()>0)
                {
                    NowRequirementViewModel.NowRequirementPromptRules.Where(x => x.ResultPromptRule.PromptSequence == null).ToList().ForEach(x => toProcess.Add(x));
                }

                foreach (var nowReqPromptRuleVM in toProcess)
                {
                    Children.Add(new NowRequirementPromptRuleTreeViewModel(treeModel, nowReqPromptRuleVM, this));
                }
            }

            if (NowRequirementViewModel.NowRequirements != null)
            {
                foreach (var nowReqVM in NowRequirementViewModel.NowRequirements.Where(x=>x.DeletedDate == null).OrderBy(x => x.ResultDefinitionSequence))
                {
                    Children.Add(new NowRequirementTreeViewModel(treeModel, nowReqVM, this));
                }
            }

            //base.Children.CollectionChanged += children_CollectionChanged;
        }

        #region NowRequirementViewModel Properties

        public NowRequirementViewModel NowRequirementViewModel { get; set; }

        #endregion NowRequirementViewModel Properties

        #region Copy / Paste Members

        public NowRequirementTreeViewModel Clone()
        {
            NowRequirementTreeViewModel res = (NowRequirementTreeViewModel)this.MemberwiseClone();
            return res;
        }

        public NowRequirementTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as NowRequirementTreeViewModel;
            set => treeModel.CopiedTreeViewModel = value;
        }

        #endregion Copy / Paste Members

    }
}