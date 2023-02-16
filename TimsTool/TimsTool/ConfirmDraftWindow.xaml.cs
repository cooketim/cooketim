using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimsTool
{
    /// <summary>
    /// Interaction logic for ConfirmDraftWindow.xaml
    /// </summary>
    public partial class ConfirmDraftWindow : Window
    {
        private PublicationTagsModel publicationTagsModel;
        public ConfirmDraftWindow(PublicationTagsModel publicationTagsModel, string instruction)
        {
            InitializeComponent();
            this.publicationTagsModel = publicationTagsModel;
            this.DataContext = publicationTagsModel;
            this.txt_instruction.Text = instruction;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            CloseDialogWithResult(this, true);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseDialogWithResult(this, false);
        }

        private void CloseDialogWithResult(Window dialog, bool result)
        {
            if (dialog != null)
                dialog.DialogResult = result;
        }
    }
}
