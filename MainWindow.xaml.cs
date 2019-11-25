using MySermonsWPF.Data;
using MySermonsWPF.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MySermonsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Sermon> AllSermons;
        public static ObservableCollection<SortedSermons> SermonsToDisplay { get; set; }
        public static List<string> Filters { get; set; }
        public static List<string> ChildContextMenuItems { get; set; }
        public static List<string> ParentContextMenuItems { get; set; }
        public static FontFamily ControlFont => MyFonts.Find(ff => ff.FamilyNames.Values.Contains("Ubuntu"));
        public SermonFilter CurrentFilter
        {
            get
            {
                return currentFilter;
            }
            set
            {
                currentFilter = value;
                this.SortSermons();
            }
        }
        private SermonFilter currentFilter = SermonFilter.Location;
        private static readonly List<FontFamily> MyFonts = Fonts.GetFontFamilies(new Uri("pack://application:,,,/"), "/UI/Resources/Fonts/").ToList();
        public MainWindow()
        {
            if (Database.Initialise())
            {
                AllSermons = Sermon.Read();
                SermonsToDisplay = new ObservableCollection<SortedSermons>();
                Filters = new List<string>()
                {
                    "Date", "Location", "Speaker", "Theme", "Title"
                };
                ChildContextMenuItems = new List<string>()
                {
                    "View", "Edit", "Print", "Delete"
                };
                this.InitializeComponent();
                this.BaseComboBoxFilters.SelectedIndex = 1;
            }
            else
            {
                MessageBox.Show("Database initialisation failed.");
            }
        }

        /// <summary>
        /// Get and display sermon based on a GUID.
        /// </summary>
        /// <param name="guid">The string representation of the GUID.</param>
        private void ViewSermonByGuid(string guid)
        {
            var sermon = this.FindChildInSortedListByGuid(guid);
            if (sermon != null)
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
            if (sermon != null)
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
            return (from x in AllSermons where x.GUID.Equals(guid) select x).FirstOrDefault();
        }

        /// <summary>
        /// Update the current filter property.
        /// </summary>
        /// <param name="filterString">The string representation of the new filter.</param>
        private void SetCurrentFilter(string filterString)
        {
            switch (filterString)
            {
                case "Date":
                    CurrentFilter = SermonFilter.Date;
                    break;
                case "Location":
                default:
                    CurrentFilter = SermonFilter.Location;
                    break;
                case "Speaker":
                    CurrentFilter = SermonFilter.Speaker;
                    break;
                case "Theme":
                    CurrentFilter = SermonFilter.Theme;
                    break;
                case "Title":
                    CurrentFilter = SermonFilter.Title;
                    break;
            }
        }

        /// <summary>
        /// Sort the sermons based on the current filter.
        /// </summary>
        private void SortSermons()
        {
            List<SortedSermons> list = Sermon.Sort(CurrentFilter, AllSermons);
            SermonsToDisplay.Clear();
            foreach (SortedSermons item in list)
            {
                SermonsToDisplay.Add(item);
            }
        }

        /// <summary>
        /// Event handler for treeview child context menu: view clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                ContextMenu contextMenu = FindVisualParentOfTargetType(typeof(ContextMenu), menuItem) as ContextMenu;
                TextBlock textBlock = contextMenu.PlacementTarget as TextBlock;
                switch (menuItem.Header.ToString().ToLower())
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
        /// Find the visual parent of the specified type of a control.
        /// </summary>
        /// <param name="control">The child control.</param>
        /// <returns>The parent</returns>
        private Control FindVisualParentOfTargetType(Type targetType, DependencyObject control)
        {
            do
            {
                control = VisualTreeHelper.GetParent(control);
            } while (!(control.GetType() == targetType));
            return (Control)control;
        }

        /// <summary>
        /// Event handler for treeview child context menu: view clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentTreeViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender is MenuItem menuItem) && (menuItem.Parent is ContextMenu contextMenu) && contextMenu.PlacementTarget is TreeView)
            {
                string action = menuItem.Header.ToString().ToLower().Replace(" all", "");
                foreach (SortedSermons sortedSermon in SermonsToDisplay)
                {
                    switch (action)
                    {
                        case "view":
                            foreach (Sermon sermon in sortedSermon.Children)
                            {
                                this.ViewSermonByGuid(sermon.GUID);
                            }
                            break;
                        case "edit":
                            foreach (Sermon sermon in sortedSermon.Children)
                            {
                                this.EditSermonByGuid(sermon.GUID);
                            }
                            break;
                        case "print":
                            foreach (Sermon sermon in sortedSermon.Children)
                            {
                                this.PrintSermonByGuid(sermon.GUID);
                            }
                            break;
                        case "delete":
                            foreach (Sermon sermon in sortedSermon.Children)
                            {
                                this.DeleteSermonByGuid(sermon.GUID);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for treeview entry: clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewEntry_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender is TextBlock textBlock))
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    this.ViewSermonByGuid(textBlock.Tag.ToString());
                }
            }
        }

        /// <summary>
        /// Event handler for filter combobox: selection changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                this.SetCurrentFilter(e.AddedItems[0].ToString());
            }
        }

        /// <summary>
        /// Event handler for treeview child context menu: sort by menu item clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByMenuItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TextBlock textBlock)
            {
                this.SetCurrentFilter(textBlock.Text);
            }
        }

        /// <summary>
        /// Event handler for exit command: check if can execute command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Event handler for exit command: what to do once executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event handler for new command: check if can execute command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Event handler for new command: what to do once executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabItem tabItem = new TabItem()
            {
                Header = "New document",
                Content = new MSRichTextBox(null)
            };
            this.BaseTabControl.Items.Add(tabItem);
            this.BaseTabControl.SetSelectedItem(tabItem);
        }
    }
}
