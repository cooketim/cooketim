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
    /// Interaction logic for requesting application nows
    /// </summary>
    public partial class ApplicationNowWindow : Window
    {
        public ApplicationNowWindow(bool isNow)
        {
            InitializeComponent();

            base.DataContext = new ApplicationRequestViewModel(true,false) { WithDebug = false };

            if (!isNow)
            {
                this.Title = "Sample Application EDT";
            }
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
