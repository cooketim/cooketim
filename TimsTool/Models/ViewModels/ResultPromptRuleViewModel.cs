using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DataLib;

namespace Models.ViewModels
{
    public class ResultPromptRuleViewModel : ViewModelBase
    {
        List<string> allUserGroups;

        ResultDefinitionViewModel resultDefinitionViewModel;

        public ResultPromptRuleViewModel(ITreeModel treeModel, ResultPromptViewModel resultPromptViewModel, ResultPromptRule resultPromptRule, 
            List<string> allUserGroups, ResultDefinitionViewModel resultDefinitionViewModel)
        {
            if (treeModel == null)
            {
                throw new ArgumentException("treeModel cannot be null");
            }
            this.treeModel = treeModel;
            this.allUserGroups = allUserGroups;
            ResultPromptRule = resultPromptRule;
            ResultPromptViewModel = resultPromptViewModel;
            this.resultDefinitionViewModel = resultDefinitionViewModel;

            if (resultDefinitionViewModel == null)
            {
                throw new ArgumentException("resultDefinitionViewModel cannot be null");
            }

            LoadAssociatedModels();
        }

        public ResultPromptRuleViewModel(ITreeModel treeModel, ResultPromptRule resultPromptRule, List<string> allUserGroups, AllResultPromptWordGroupViewModel allWordGroups, List<ResultPromptViewModel> allPrompts)
        {
            if (treeModel == null)
            {
                throw new ArgumentException("treeModel cannot be null");
            }
            this.treeModel = treeModel;
            this.allUserGroups = allUserGroups;

            //Using existing result prompt view model when it exists
            var rpVM = allPrompts.FirstOrDefault(x => x.UUID == resultPromptRule.ResultPrompt.UUID);
            if (rpVM != null)
            {
                ResultPromptViewModel = rpVM;
            }
            else
            {
                ResultPromptViewModel = new ResultPromptViewModel(treeModel,resultPromptRule.ResultPrompt, allWordGroups);
                allPrompts.Add(ResultPromptViewModel);
            }

            ResultPromptRule = resultPromptRule;

            LoadAssociatedModels();
        }

        private void LoadAssociatedModels()
        {
            userGroups = ResultPromptRule.UserGroups == null ? new SilentObservableCollection<string>() : new SilentObservableCollection<string>(ResultPromptRule.UserGroups.OrderBy(x => x).ToList());

            UserGroups.CollectionChanged += userGroups_CollectionChanged;
        }

        private void userGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var user = (string)item;
                    if (ResultPromptRule.UserGroups == null)
                    {
                        ResultPromptRule.UserGroups = new List<string>() { user };
                    }
                    else
                    {
                        ResultPromptRule.UserGroups.Add(user);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var user = (string)item;
                    if (ResultPromptRule.UserGroups != null) { ResultPromptRule.UserGroups.Remove(user); }
                }
            }

            OnPropertyChanged("AvailableUserGroups");
        }

        public SilentObservableCollection<string> AvailableUserGroups
        {
            get
            {
                var unselected = allUserGroups.Except(UserGroups).ToList();
                return new SilentObservableCollection<string>(unselected.OrderBy(x => x).ToList());
            }
        }

        private SilentObservableCollection<string> userGroups;
        public SilentObservableCollection<string> UserGroups
        {
            get => userGroups;
            set
            {
                userGroups = value;
                userGroups.CollectionChanged += userGroups_CollectionChanged;
            }
        }

        public ResultPromptRule ResultPromptRule
        {
            get { return data as ResultPromptRule; }
            set { data = value; }
        }

        public ResultPromptViewModel ResultPromptViewModel { get; set; }

        public void SetDraftResultPrompt(ResultPromptViewModel draft)
        {
            if (PublishedStatus != PublishedStatus.Draft || ResultPromptViewModel.MasterUUID != draft.MasterUUID)
            {
                return;
            }
            ResultPromptViewModel = draft;
            ResultPromptRule.ResultPrompt = draft.ResultPrompt;
            LastModifiedDate = DateTime.Now;
            OnPropertyChanged("ResultPromptViewModel");
        }

        public ResultPromptRuleType? RuleType
        {
            get => ResultPromptRule.Rule;
        }

        public ResultPromptRuleType? RuleTypeMags
        {
            get => ResultPromptRule.RuleMags ?? ResultPromptRule.Rule;
            set
            {
                if (value != null)
                {
                    //when the value is the same as crown, set the common rule and remove the separate rules
                    if (RuleTypeCrown == value)
                    {
                        ResultPromptRule.Rule = value;
                        ResultPromptRule.RuleCrown = null;
                        ResultPromptRule.RuleMags = null;
                        OnPropertyChanged("Rule");
                        OnPropertyChanged("RuleCrown");
                        OnPropertyChanged("RuleMags");
                        LastModifiedDate = DateTime.Now;
                        return;
                    }

                    //value is different to crown, set separate values and set the common value to null
                    ResultPromptRule.RuleMags = value;
                    if (ResultPromptRule.RuleCrown == null)
                    {
                        ResultPromptRule.RuleCrown = ResultPromptRule.Rule;
                        ResultPromptRule.Rule = null;
                        OnPropertyChanged("Rule");
                        OnPropertyChanged("RuleCrown");
                    }

                    OnPropertyChanged("RuleMags");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public ResultPromptRuleType? RuleTypeCrown
        {
            get => ResultPromptRule.RuleCrown ?? ResultPromptRule.Rule;
            set
            {
                if (value != null)
                {
                    //when the value is the same as mags, set the common rule and remove the separate rules
                    if (RuleTypeMags == value)
                    {
                        ResultPromptRule.Rule = value;
                        ResultPromptRule.RuleCrown = null;
                        ResultPromptRule.RuleMags = null;
                        OnPropertyChanged("Rule");
                        OnPropertyChanged("RuleCrown");
                        OnPropertyChanged("RuleMags");
                        LastModifiedDate = DateTime.Now;
                        return;
                    }

                    //value is different to mags, set separate values and set the common value to null
                    ResultPromptRule.RuleCrown = value;
                    if (ResultPromptRule.RuleMags == null)
                    {
                        ResultPromptRule.RuleMags = ResultPromptRule.Rule;
                        ResultPromptRule.Rule = null;
                        OnPropertyChanged("Rule");
                        OnPropertyChanged("RuleMags");
                    }

                    OnPropertyChanged("RuleCrown");
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public int? PromptSequence
        {
            get => ResultPromptRule.PromptSequence;
            set 
                {
                    if (SetProperty(() => ResultPromptRule.PromptSequence == value, () => ResultPromptRule.PromptSequence = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public ResultDefinitionViewModel ResultDefinitionViewModel
        {
            get => resultDefinitionViewModel;
            set
            {
                if (resultDefinitionViewModel == null) { resultDefinitionViewModel = value; }
                else if (SetProperty(() => resultDefinitionViewModel.UUID == value.UUID, () => resultDefinitionViewModel = value))
                {
                    LastModifiedDate = DateTime.Now;                    
                }
            }
        }

        public bool IsResultPromptPublishedPending
        {
            get
            {
                if (ResultPromptViewModel.IsDeleted) { return false; }
                if (IsDeleted) { return false; }
                return ResultPromptViewModel.IsPublishedPending;
            }
        }

        public void ResetPending()
        {
            OnPropertyChanged("IsResultPromptPublishedPending");
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

                //set the parent result definition as modified
                resultDefinitionViewModel.LastModifiedDate = DateTime.Now;
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
                base.DeletedDate = value;

                //set the parent result definition as modified
                resultDefinitionViewModel.LastModifiedDate = DateTime.Now;
            }
        }

        #region Copy Members / Paste Members

        public ResultPromptRuleViewModel Copy()
        {
            //make new data view model
            var ruleData = this.ResultPromptRule.Copy();
            var promptData = this.ResultPromptRule.ResultPrompt.Copy();
            ruleData.ResultPrompt = promptData;

            //make a new view model
            var vm = new ResultPromptViewModel(treeModel,promptData, treeModel.AllResultPromptWordGroupViewModel);
            var res = new ResultPromptRuleViewModel(treeModel, vm, ruleData, allUserGroups, ResultDefinitionViewModel);
            //make new view collections for the user groups
            if (ruleData.UserGroups != null)
            {
                res.UserGroups = new SilentObservableCollection<string>(ruleData.UserGroups.OrderBy(x=>x));
            }
            else
            {
                res.UserGroups = new SilentObservableCollection<string>();
            }
            return res;
        }

        public ResultPromptRuleViewModel Clone()
        {
            //make new data view model, keeping the same prompt
            var ruleData = this.ResultPromptRule.Copy();
            var res = new ResultPromptRuleViewModel(treeModel, ResultPromptViewModel, ruleData, allUserGroups, ResultDefinitionViewModel);
            //make new view collections for the user groups
            if (ruleData.UserGroups != null)
            {
                res.UserGroups = new SilentObservableCollection<string>(ruleData.UserGroups.OrderBy(x => x));
            }
            else
            {
                res.UserGroups = new SilentObservableCollection<string>();
            }
            return res;
        }

        #endregion Copy Members / Paste Members
    }
}