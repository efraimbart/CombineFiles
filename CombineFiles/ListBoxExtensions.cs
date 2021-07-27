using System.Linq;
using System.Windows.Forms;

namespace CombineFiles
{
    public static class ListBoxExtensions
    {
        public static void MoveSelectedItemUp(this ListBox listBox)
        {
            MoveSelectedItem(listBox, -1);
        }

        public static void MoveSelectedItemDown(this ListBox listBox)
        {
            MoveSelectedItem(listBox, 1);
        }

        private static void MoveSelectedItem(ListBox listBox, int direction)
        {
            var selectedItems = listBox.SelectedItems.Cast<object>().ToList();
            
            if (direction > 0)
            {
                selectedItems.Reverse();
            }
            // Checking selected item
            if (!selectedItems.Any())
                return; // No selected item - nothing to do

            foreach (var item in selectedItems)
            {
                var index = listBox.Items.IndexOf(item);
                // Calculate new index using move direction
                var newIndex = index + direction;

                // Checking bounds of the range
                if (newIndex < 0 || newIndex >= listBox.Items.Count)
                    return; // Index out of range - nothing to do
                
                // Removing removable element
                listBox.Items.RemoveAt(index);
                // Insert it in new position
                listBox.Items.Insert(newIndex, item);
                // Restore selection
                listBox.SetSelected(newIndex, true);
            }


        }
    }
}