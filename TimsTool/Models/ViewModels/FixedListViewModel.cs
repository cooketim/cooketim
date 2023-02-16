using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using DataLib;
using System.Collections.Specialized;

namespace Models.ViewModels
{
    public class FixedListViewModel : ViewModelBase
    {
        private ICommand addFixedListValueCommand, deleteFixedListValueCommand;

        public FixedListViewModel(ITreeModel treeModel, FixedList fixedList)
        {
            this.treeModel = treeModel;
            FixedList = fixedList;
            fixedListValues = new SilentObservableCollection<FixedListValueViewModel>(
                 from value in fixedList.Values.FindAll((x => x.DeletedDate == null))
                 select new FixedListValueViewModel(value, this));
            fixedListValues.CollectionChanged += FixedListValues_CollectionChanged;

            //set up the publication tag model
            PublicationTagsModel = new PublicationTagsModel(treeModel, this);

            //set up the comments model
            AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, this);
        }

        public PublicationTagsModel PublicationTagsModel { get; set; }

        public AssociatedCommentsModel AssociatedCommentsModel { get; set; }

        public ICommand AddFixedListValueCommand
        {
            get
            {
                if (addFixedListValueCommand == null)
                {
                    addFixedListValueCommand = new RelayCommand<FixedListValueViewModel>(x => AddFixedListValue(x));
                }
                return addFixedListValueCommand;
            }
        }
        public ICommand DeleteFixedListValueCommand
        {
            get
            {
                if (deleteFixedListValueCommand == null)
                {
                    deleteFixedListValueCommand = new RelayCommand<FixedListValueViewModel>(x => DeleteFixedListValue(x));
                }
                return deleteFixedListValueCommand;
            }
        }

        private void AddFixedListValue(FixedListValueViewModel flVM)
        {
            if (flVM == null)
            {
                flVM = CreateNewFixedListValue();
            }
            FixedListValues.Add(flVM);

            LastModifiedDate = DateTime.Now;
        }

        internal FixedListViewModel Draft()
        {
            FixedListViewModel res = (FixedListViewModel)this.MemberwiseClone();

            //make new data
            res.FixedList = res.FixedList.Draft();

            //reset the collection of values
            var newValues = new SilentObservableCollection<FixedListValueViewModel>();
            foreach (var item in res.FixedList.Values)
            {
                newValues.Add(new FixedListValueViewModel(item, res));
            }
            res.fixedListValues = newValues;

            //set a new publication tags model
            res.PublicationTagsModel = new PublicationTagsModel(treeModel, res);

            //Set a new comments model
            res.AssociatedCommentsModel = new AssociatedCommentsModel(treeModel, res);

            //save the vms and data
            treeModel.AllFixedListViewModel.FixedLists.Add(res);
            treeModel.AllData.FixedLists.Add(res.FixedList);

            return res;
        }

        private FixedListValueViewModel CreateNewFixedListValue()
        {
            //Make a new item 
            var data = new FixedListValue();
            var vm = new FixedListValueViewModel(data, this);

            //ensure that value is unique
            var duplicateValue = false;
            int i = 1;
            do
            {
                duplicateValue = FixedListValues.FirstOrDefault(x => x.Value == data.Value) != null;
                if (duplicateValue)
                {
                    i++;
                    data.Value = string.Format("New Fixed List Value ({0})", i);
                }

            } while (duplicateValue);

            return vm;
        }

        private void DeleteFixedListValue(FixedListValueViewModel flVM)
        {
            if (flVM == null)
            {
                flVM = SelectedFixedListValue;
            }
            if (flVM != null)
            {
                FixedListValues.Remove(flVM);
            }
            SelectedFixedListValue = null;
            OnPropertyChanged("SelectedFixedListValue");

            LastModifiedDate = DateTime.Now;
        }

        private SilentObservableCollection<FixedListValueViewModel> fixedListValues;
        public SilentObservableCollection<FixedListValueViewModel> FixedListValues
        {
            get => fixedListValues;
            set
            {
                if (value == null)
                {
                    fixedListValues.Clear();
                }
                else
                {
                    fixedListValues = value;
                    fixedListValues.CollectionChanged += FixedListValues_CollectionChanged;
                }
            }
        }

        private void FixedListValues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastModifiedDate = DateTime.Now;

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var fixedListValue = (FixedListValueViewModel)item;
                    if (FixedList.Values == null)
                    {
                        FixedList.Values = new List<FixedListValue>() { fixedListValue.FixedListValue };
                    }
                    else
                    {
                        var index = 0;
                        if (e.NewStartingIndex != -1)
                        {
                            index = e.NewStartingIndex;
                        }
                        FixedList.Values.Insert(index, fixedListValue.FixedListValue);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var fixedListValue = (FixedListValueViewModel)item;
                    if (FixedList.Values != null) { FixedList.Values.Remove(fixedListValue.FixedListValue); }
                    if (FixedList.Values.Count() == 0) { FixedList.Values = null; }
                }
            }

            OnPropertyChanged("FixedListValues");
        }

        public FixedListValueViewModel SelectedFixedListValue { get; set; }

        public FixedList FixedList
        {
            get { return data as FixedList; }
            private set { data = value; }
        }

        public bool IsFixedListPublishedPending
        {
            get
            {
                if (IsDeleted) { return false; }
                if (PublishedStatus == PublishedStatus.PublishedPending) { return true; }
                return false;
            }
        }

        public string Label
        {
            get => FixedList.Label;
            set
                {
                    if (SetProperty(() => FixedList.Label == value, () => FixedList.Label = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public DateTime? StartDate
        {
            get => FixedList.StartDate;
            set
            {
                if (SetProperty(() => FixedList.StartDate == value, () => FixedList.StartDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        public DateTime? EndDate
        {
            get => FixedList.EndDate;
            set
            {
                if (SetProperty(() => FixedList.EndDate == value, () => FixedList.EndDate = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }

        #region Copy Members

        public FixedListViewModel Copy()
        {
            FixedListViewModel res = (FixedListViewModel)this.MemberwiseClone();

            //make new data view model
            res.FixedList = res.FixedList.Copy();
            return res;
        }

        #endregion Copy Members
    }
}