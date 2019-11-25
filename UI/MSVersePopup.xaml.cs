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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSVersePopup.xaml
    /// </summary>
    public partial class MSVersePopup : UserControl
    {
        public string VerseRef
        {
            set => BaseVerseRef.Text = value;
        }
        public string VerseContent
        {
            set => BaseVerseContent.Text = value;
        }
        public MSVersePopup()
        {
            InitializeComponent();
        }
    }
}
