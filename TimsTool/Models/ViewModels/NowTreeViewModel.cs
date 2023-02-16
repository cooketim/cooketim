using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DataLib;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a NowTreeViewModel object.
    /// </summary>
    public class NowTreeViewModel : TreeViewItemViewModel
    {
        public NowTreeViewModel(ITreeModel treeModel, NowViewModel nowViewModel)
            : base(null, true, nowViewModel.Name, nowViewModel.UUID.ToString())
        {
            this.treeModel = treeModel;
            NowViewModel = nowViewModel;
            //LoadChildren();
        }

        protected override void LoadChildren()
        {
            if (NowViewModel != null && NowViewModel.NowRequirements != null)
            {
                var nowRequirements = NowViewModel.NowRequirements.OrderBy(x => x.ResultDefinitionSequence);

                foreach (var nowRequirement in nowRequirements)
                {
                    Children.Add(new NowRequirementTreeViewModel(treeModel, nowRequirement, this));
                }
            }
        }

        #region nowViewModel Properties

        public NowViewModel NowViewModel { get; set; }

        #endregion // nowViewModel Properties

        #region Presentation Members

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(NowViewModel.Name))
                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //test for guid
            Guid guidSearch = Guid.NewGuid();
            var isGuid = Guid.TryParse(text, out guidSearch);
            if (isGuid)
            {
                if (NowViewModel.UUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }
                if (NowViewModel.MasterUUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }

                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
            }

            //match by label
            var isEqual = String.Equals(NowViewModel.Name, text, StringComparison.InvariantCultureIgnoreCase);
            if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (NowViewModel.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }
            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        public NowTreeViewModel Draft(List<string> publicationTags)
        {
            //make new draft data view model
            var nowVM = NowViewModel.Draft(publicationTags);
            return new NowTreeViewModel(treeModel, nowVM);
        }

        #endregion Presentation Members

        #region Copy / Paste Members

        public NowTreeViewModel Copy(TreeViewItemViewModel parent)
        {
            NowTreeViewModel now = (NowTreeViewModel)this.MemberwiseClone();

            ////make new data view model
            //now.NowViewModel = now.NowViewModel.Copy();
            
            //set the parentage
            if (parent!=null)
            {
                now.ResetParentage(parent);
            }
            return now;
        }

        public NowTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as NowTreeViewModel;
            set
            {
                treeModel.CopiedTreeViewModel = value;
            }
        }

        #endregion Copy / Paste Members

        internal void ResetChildren()
        {
            Children.Clear();
            LoadChildren();
            IsExpanded = false;
        }

        //#region Drag Drop Members

        //public override string GetDragName()
        //{
        //    return nowViewModel.Name;
        //}

        //public override bool CheckDropTarget(TreeViewItemViewModel sourceItem)
        //{
        //    //Check whether the source item can be pasted to this target item
        //    var sourceType = sourceItem.GetType();
        //    if (sourceType == GetType() || sourceType == typeof(NowRequirementTreeViewModel)
        //        || sourceType == typeof(ResultDefinitionTreeViewModel)) { return true; }
        //    return false;
        //}

        //public override ICommand DoDrop(TreeViewItemViewModel sourceItem, TreeViewItemViewModel targetItem)
        //{
        //    if (targetItem.GetType() != GetType())
        //    {
        //        //target must be a now to continue
        //        return null;
        //    }

        //    //Check whether the source item can be pasted to this target item
        //    if (!CheckDropTarget(sourceItem)) { return null; }

        //    //set the pending copy and target models
        //    TreeModel.Instance.CopiedTreeViewModel = sourceItem;
        //    TreeModel.Instance.SelectedItem = targetItem;
        //    targetItem.IsSelected = true;

        //    //determine the command based on the sourceItem
        //    var sourceType = sourceItem.GetType();

        //    if (sourceType == typeof(ResultDefinitionTreeViewModel))
        //    {
        //        return TreeModel.Instance.PasteResultDefinitionCommand;
        //    }

        //    return null;
        //}

        //#endregion Drag Drop Members


    }
}