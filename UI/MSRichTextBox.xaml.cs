using System.Windows.Controls;
using MySermonsWPF.Data;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSRichTextBox.xaml
    /// </summary>
    public partial class MSRichTextBox:UserControl
    {
        private Sermon sermon;
        public MSRichTextBox(Sermon sermon)
        {
            this.InitializeComponent();
            this.sermon = sermon;
        }
    }
}
