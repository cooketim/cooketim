using System.Windows;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Collections.Specialized;
using Models.ViewModels;

namespace TimsTool.Tree.Behaviors
{
    public class TreeViewScrollBehaviour : Behavior<TreeView>
    {
        public object BoundSelectedItem
        {
            get => GetValue(BoundSelectedItemProperty);
            set => SetValue(BoundSelectedItemProperty, value);
        }

        public static readonly DependencyProperty BoundSelectedItemProperty =
            DependencyProperty.Register("BoundSelectedItem",
                typeof(object),
                typeof(TreeViewScrollBehaviour),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnBoundSelectedItemChanged));

        private static void OnBoundSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is TreeViewItemViewModel item)
                item.IsSelected = true;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            base.OnDetaching();
        }

        private void OnTreeViewSelectedItemChanged(object obj, RoutedPropertyChangedEventArgs<object> args)
        {
            BoundSelectedItem = args.NewValue;
        }
    }
}
