using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySermonsWPF.Data;
using MySermonsWPF.UI;

namespace MySermonsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow:Window
    {
        public static List<SortedSermons> Sermons { get; set; }
        public MainWindow()
        {
            if(Database.Initialise())
            {
                Sermons = Sermon.Sort(SermonFilters.Location, Sermon.Read());

                this.InitializeComponent();
                this.BaseTabControl.Items.Add(new TabItem()
                {
                    Header = "New document",
                    Content = new MSRichTextBox(null)
                });
            }
            else
            {
                MessageBox.Show("Database initialisation failed.");
            }
        }

        private void TreeViewEntry_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if((sender is TextBlock textBlock))
            {
                if(e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    this.ViewSermonByGuid(textBlock.Tag.ToString());
                }
            }
        }

        /// <summary>
        /// Get and display sermon based on a GUID.
        /// </summary>
        /// <param name="guid">The string representation of the GUID.</param>
        private void ViewSermonByGuid(string guid)
        {
            var sermon = this.FindChildInSortedListByGuid(guid);
            if(sermon != null)
            {
                TabItem tabItem = new TabItem()
                {
                    Header = sermon.Title,
                    Content = new MSViewer(sermon)
                };
                this.BaseTabControl.Items.Add(tabItem);
                this.BaseTabControl.SetSelectedItem(tabItem);
            }
        }

        /// <summary>
        /// Edit sermon based on a GUID.
        /// </summary>
        /// <param name="guid">The string representation of the GUID.</param>
        private void EditSermonByGuid(string guid)
        {
            var sermon = this.FindChildInSortedListByGuid(guid);
            if(sermon != null)
            {
                TabItem tabItem = new TabItem()
                {
                    Header = sermon.Title,
                    Content = new MSRichTextBox(sermon)
                };
                this.BaseTabControl.Items.Add(tabItem);
                this.BaseTabControl.SetSelectedItem(tabItem);
            }
        }

        /// <summary>
        /// Print a sermon based on a GUID.
        /// </summary>
        /// <param name="guid">The string representation of the GUID.</param>
        private void PrintSermonByGuid(string guid)
        {
            var sermon = this.FindChildInSortedListByGuid(guid);
        }

        /// <summary>
        /// Delete a sermon based on a GUID.
        /// </summary>
        /// <param name="guid">The string representation of the GUID.</param>
        private void DeleteSermonByGuid(string guid)
        {
            var sermon = this.FindChildInSortedListByGuid(guid);
        }

        /// <summary>
        /// Find a sermon in the sorted sermons list by GUID.
        /// </summary>
        /// <param name="guid">The string representation of the GUID.</param>
        /// <returns>A sermon or null.</returns>
        private Sermon FindChildInSortedListByGuid(string guid)
        {
            return (from x in Sermons
                    from y in x.Children
                    where y.GUID.Equals(guid)
                    select y).FirstOrDefault();
        }

        /// <summary>
        /// Find a parent in the sorted sermons list by tag.
        /// </summary>
        /// <param name="guid">The string representation of the GUID.</param>
        /// <returns>SortedSermons or null.</returns>
        private SortedSermons FindParentInSortedList(string tag)
        {
            return (from x in Sermons
                    where x.Parent.Equals(tag)
                    select x).FirstOrDefault();
        }

        /// <summary>
        /// Event handler for treeview child context menu: view clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ChildContextMenuHandler(sender, "view");
        }

        /// <summary>
        /// Event handler for treeview child context menu: edit clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ChildContextMenuHandler(sender, "edit");
        }

        /// <summary>
        /// Event handler for treeview child context menu: print clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ChildContextMenuHandler(sender, "print");
        }

        /// <summary>
        /// Event handler for treeview child context menu: delete clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ChildContextMenuHandler(sender, "delete");
        }

        /// <summary>
        /// Fires appropriate actions based on a specified action.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="action">The action to be fired.</param>
        /// <example>action: view, edit, print, delete</example>
        private void ChildContextMenuHandler(object sender, string action)
        {
            if((sender is MenuItem menuItem) && (menuItem.Parent is ContextMenu contextMenu) && ((contextMenu).PlacementTarget is TextBlock textBlock))
            {
                switch(action)
                {
                    case "view":
                        this.ViewSermonByGuid(textBlock.Tag.ToString());
                        break;
                    case "edit":
                        this.EditSermonByGuid(textBlock.Tag.ToString());
                        break;
                    case "print":
                        this.PrintSermonByGuid(textBlock.Tag.ToString());
                        break;
                    case "delete":
                        this.DeleteSermonByGuid(textBlock.Tag.ToString());
                        break;
                }
            }
        }

        /// <summary>
        /// Event handler for treeview child context menu: view clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewAllTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ParentContextMenuHandler(sender, "view");
        }

        /// <summary>
        /// Event handler for treeview child context menu: edit clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditAllTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ParentContextMenuHandler(sender, "edit");
        }

        /// <summary>
        /// Event handler for treeview child context menu: print clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintAllTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ParentContextMenuHandler(sender, "print");
        }

        /// <summary>
        /// Event handler for treeview child context menu: delete clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteAllTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ParentContextMenuHandler(sender, "delete");
        }

        /// <summary>
        /// Fires appropriate actions based on a specified action.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="action">The action to be fired.</param>
        /// <example>action: view, edit, print, delete</example>
        private void ParentContextMenuHandler(object sender, string action)
        {
            if((sender is MenuItem menuItem) && (menuItem.Parent is ContextMenu contextMenu))
            {
                if(contextMenu.PlacementTarget is TextBlock textBlock)
                {
                    var sortedSermon = this.FindParentInSortedList(textBlock.Tag.ToString());
                    switch(action)
                    {
                        case "view":
                            foreach(var sermon in sortedSermon.Children)
                            {
                                this.ViewSermonByGuid(sermon.GUID);
                            }
                            break;
                        case "edit":
                            foreach(var sermon in sortedSermon.Children)
                            {
                                this.EditSermonByGuid(sermon.GUID);
                            }
                            break;
                        case "print":
                            foreach(var sermon in sortedSermon.Children)
                            {
                                this.PrintSermonByGuid(sermon.GUID);
                            }
                            break;
                        case "delete":
                            foreach(var sermon in sortedSermon.Children)
                            {
                                this.DeleteSermonByGuid(sermon.GUID);
                            }
                            break;
                    }
                }
                else if(contextMenu.PlacementTarget is TreeView treeView)
                {
                    var sortedSermons = treeView.ItemsSource;
                    foreach(SortedSermons sortedSermon in sortedSermons)
                    {
                        switch(action)
                        {
                            case "view":
                                foreach(var sermon in sortedSermon.Children)
                                {
                                    this.ViewSermonByGuid(sermon.GUID);
                                }
                                break;
                            case "edit":
                                foreach(var sermon in sortedSermon.Children)
                                {
                                    this.EditSermonByGuid(sermon.GUID);
                                }
                                break;
                            case "print":
                                foreach(var sermon in sortedSermon.Children)
                                {
                                    this.PrintSermonByGuid(sermon.GUID);
                                }
                                break;
                            case "delete":
                                foreach(var sermon in sortedSermon.Children)
                                {
                                    this.DeleteSermonByGuid(sermon.GUID);
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
