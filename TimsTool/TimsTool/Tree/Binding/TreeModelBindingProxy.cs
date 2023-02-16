using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimsTool.Tree.Binding
{
    public class TreeModelBindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new TreeModelBindingProxy();
        }

        public TreeModel Data
        {
            get { return (TreeModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = 
            DependencyProperty.Register("Data", typeof(TreeModel), typeof(TreeModelBindingProxy), new UIPropertyMetadata(null));
    }
}
