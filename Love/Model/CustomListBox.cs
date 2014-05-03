using System.Windows.Controls;

namespace Love.Model
{
    public delegate void PrepareContainerForItemDelegate(System.Windows.DependencyObject element, object item);
    public class CustomListBox : ListBox
    {
        public event PrepareContainerForItemDelegate PrepareContainerForItem;

        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (PrepareContainerForItem != null)
                PrepareContainerForItem(element, item);
        }
    }
}
