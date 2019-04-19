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
        public MainWindow()
        {
            if(Database.Initialise())
            {
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
    }
}
