using System.Windows.Controls;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MetadataPanel.xaml
    /// </summary>
    public partial class MetadataPanel : UserControl
    {
        public MetadataPanel()
        {
            InitializeComponent();
        }
        public void Populate(string title, string speaker, string keytext, string location, string themes, string otherinfo)
        {
            this.MetaTitle.Text = title ?? string.Empty;
            this.MetaSpeaker.Text = speaker ?? string.Empty;
            this.MetaKeyText.Text = keytext ?? string.Empty;
            this.MetaLocation.Text = location ?? string.Empty;
            this.MetaThemes.Text = themes ?? string.Empty;
            this.MetaOtherInfo.Text = otherinfo ?? string.Empty;
        }
        public (string title, string speaker, string keytext, string location, string themes, string otherinfo) GetMetadata()
        {
            return (this.MetaTitle.Text,
                    this.MetaSpeaker.Text,
                    this.MetaKeyText.Text,
                    this.MetaLocation.Text,
                    this.MetaThemes.Text,
                    this.MetaOtherInfo.Text);
        }
        public bool Verify()
        {
            if (string.IsNullOrEmpty(this.MetaTitle.Text))
            {
                this.MetaTitle.Focus();
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
