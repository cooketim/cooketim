namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly model for exporting to excel
    /// </summary>
    public class ExcelExportViewModel : ViewModelBase
    {
        bool isFullExport = true, isWelshExport, isSynonymExport;

        #region ExcelExportViewModel Properties

        public bool IsFullExport
        {
            get => isFullExport;
            set
            {
                SetProperty(() => isFullExport == value, () => isFullExport = value);
                if (value)
                {
                    //esure that check boxes are mutually exclusive
                    SetProperty(() => isWelshExport == false, () => isWelshExport = false);
                    OnPropertyChanged("IsWelshExport");

                    SetProperty(() => isSynonymExport == false, () => isSynonymExport = false);
                    OnPropertyChanged("IsSynonymExport");
                }
                OnPropertyChanged("BtnOKEnabled");
            }
        }

        public bool IsWelshExport
        {
            get => isWelshExport;
            set
            {
                SetProperty(() => isWelshExport == value, () => isWelshExport = value);
                if (value)
                {
                    //esure that check boxes are mutually exclusive
                    SetProperty(() => isFullExport == false, () => isFullExport = false);
                    OnPropertyChanged("IsFullExport");

                    SetProperty(() => isSynonymExport == false, () => isSynonymExport = false);
                    OnPropertyChanged("IsSynonymExport");
                }
                OnPropertyChanged("BtnOKEnabled");
            }
        }

        public bool IsSynonymExport
        {
            get => isSynonymExport;
            set
            {
                SetProperty(() => isSynonymExport == value, () => isSynonymExport = value);
                if (value)
                {
                    //esure that check boxes are mutually exclusive
                    SetProperty(() => isFullExport == false, () => isFullExport = false);
                    OnPropertyChanged("IsFullExport");

                    SetProperty(() => isWelshExport == false, () => isWelshExport = false);
                    OnPropertyChanged("IsWelshExport");
                }
                OnPropertyChanged("BtnOKEnabled");
            }
        }

        public bool BtnOKEnabled
        {
            get => isFullExport || isWelshExport || isSynonymExport;
        }

        #endregion ExcelExportViewModel Properties

    }
}