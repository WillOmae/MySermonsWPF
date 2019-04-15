using System.Windows.Controls;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class MSViewer:UserControl
    {
        /// <summary>
        /// Rich text document manager.
        /// </summary>
        private MSDocumentManager documentManager;
        /// <summary>
        /// Viewer title string.
        /// </summary>
        public string ViewerTitle
        {
            get
            {
                return this.MSViewerTitle.Text;
            }
            set
            {
                this.MSViewerTitle.Text = value;
            }
        }
        /// <summary>
        /// Rich text content string.
        /// </summary>
        public string ViewerContent
        {
            get
            {
                return this.documentManager.GetRichText();
            }
            set
            {
                this.documentManager.SetRichText(value);
            }
        }
        /// <summary>
        /// Viewer constructor.
        /// </summary>
        public MSViewer()
        {
            this.InitializeComponent();
            this.documentManager = new MSDocumentManager(this.MSViewerContent);
        }
    }
}
