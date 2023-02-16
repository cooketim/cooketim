using Models.ViewModels;
using System;
using System.Windows;

namespace TimsTool
{
    /// <summary>
    /// Interaction logic for DataPatch.xaml
    /// </summary>
    public partial class DataPatchByTagWindow : Window
    {
        private DataPatchByTagViewModel model;
        public DataPatchByTagWindow(ITreeModel treeModel, string dataFileDirectory)
        {
            InitializeComponent();

            model = new DataPatchByTagViewModel(treeModel, dataFileDirectory);
            DataContext = model;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (model.IsProdExport)
            {
                ConfirmOK();
            }
            else
            {
                CloseDialogWithResult(this, true);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseDialogWithResult(this, false);
        }

        private void ConfirmOK()
        {
            var msgModel = new MessagesModel()
            {
                Title = "Confirm Export Data Patches",
                Messages = string.Format("Exporting a Prod Data Patch will make permanent updates to the status of the changed items.{0}Do you wish to continue?", Environment.NewLine),
                DetailedMessages = null,
                CancelVisible = true
            };

            MessageWindow win = new MessageWindow(msgModel);
            win.Owner = Window.GetWindow(this);
            win.Title = "Confirm Export Data Patches";

            var res = win.ShowDialog();

            if (res.HasValue && res.Value)
            {
                CloseDialogWithResult(this, true);
            }
        }

        private void CloseDialogWithResult(Window dialog, bool result)
        {
            if (dialog != null)
                dialog.DialogResult = result;
        }
    }
}
