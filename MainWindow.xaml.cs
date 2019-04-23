using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private static List<Sermon> _sermons { get; set; }
        public MainWindow()
        {
            if(Database.Initialise())
            {
                _sermons = Sermon.Read();
                Sermons = Sermon.Sort(SermonFilters.Location, _sermons);

                this.InitializeComponent();
                this.MSTabControl.Items.Add(new TabItem()
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

        private void TreeViewEntry_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2 && (sender is TextBlock textBlock))
            {
                string guid = textBlock.Tag.ToString();
                var sermon = (from x in Sermons
                              from y in x.Children
                              where y.GUID.Equals(guid)
                              select y).FirstOrDefault();
                if(sermon != null)
                {
                    TabItem tabItem = new TabItem()
                    {
                        Header = sermon.Title,
                        Content = new MSViewer(sermon)
                    };
                    this.MSTabControl.Items.Add(tabItem);
                    this.MSTabControl.SetSelectedItem(tabItem);
                }
            }
        }
    }
}
