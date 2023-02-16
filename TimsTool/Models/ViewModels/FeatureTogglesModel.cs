using System.Windows.Input;
using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Command;

namespace Models.ViewModels
{
    public class FeatureTogglesModel : ViewModelBase
    {
        private ICommand addToggleCommand, removeToggleCommand;

        //command for adding a toggle to the collection of selected toggle
        public ICommand AddToggleCommand
        {
            get
            {
                if (addToggleCommand == null)
                {
                    addToggleCommand = new RelayCommand(AddToggle);
                }
                return addToggleCommand;
            }
        }

        //command for removing a toggle from the collection of selected toggle
        public ICommand RemoveToggleCommand
        {
            get
            {
                if (removeToggleCommand == null)
                {
                    removeToggleCommand = new RelayCommand(RemoveToggle, RemoveToggleEnabled);
                }
                return removeToggleCommand;
            }
        }

        public FeatureTogglesModel(ITreeModel treeModel)
        {
            this.treeModel = treeModel;

            //load a copy of all the available toggles, UI will bind to this to make its selections
            AvailableFeatureToggles = treeModel.AvailableFeatureToggles == null ? new SilentObservableCollection<ComboBoxItemString>() :
                new SilentObservableCollection<ComboBoxItemString>(treeModel.AvailableFeatureToggles.OrderBy(x => x.CodeString));

            //create the collection of selected toggles 
            SelectedFeatureToggles = new SilentObservableCollection<ComboBoxItemString>();

            SelectedFeatureToggles.CollectionChanged += selectedFeatureToggles_CollectionChanged;
        }

        public bool RemoveToggleEnabled
        {
            get => selectedToggle != null ? true : false;
        }

        public bool AddToggleEnabled
        {
            get => selectedAvailableToggle != null ? true : false;
        }

        private ComboBoxItemString selectedAvailableToggle = null;
        public ComboBoxItemString SelectedAvailableToggle
        {
            get => selectedAvailableToggle;
            set
            {
                SetProperty(ref selectedAvailableToggle, value);
                OnPropertyChanged("AddToggleEnabled");
            }
        }

        private ComboBoxItemString selectedToggle = null;

        public ComboBoxItemString SelectedToggle
        {
            get => selectedToggle;
            set
            {
                SetProperty(ref selectedToggle, value);
                OnPropertyChanged("RemoveToggleEnabled");
            }
        }

        public SilentObservableCollection<ComboBoxItemString> AvailableFeatureToggles { get; set; }

        public SilentObservableCollection<ComboBoxItemString> SelectedFeatureToggles { get; set; }


        private void selectedFeatureToggles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var toggle = (ComboBoxItemString)item;

                    //remove from the available toggle
                    AvailableFeatureToggles.Remove(toggle);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var toggle = (ComboBoxItemString)item;

                    //add to the available toggle
                    AvailableFeatureToggles.Add(toggle);
                }
            }

            OnPropertyChanged("SelectedFeatureToggles");
        }

        private void AddToggle()
        {
            if (selectedAvailableToggle != null)
            {
                SelectedFeatureToggles.Add(selectedAvailableToggle);
            }
        }

        private void RemoveToggle()
        {
            if (selectedToggle != null)
            {
                SelectedFeatureToggles.Remove(selectedToggle);
            }
        }
    }
}
