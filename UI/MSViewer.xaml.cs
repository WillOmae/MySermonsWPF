using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySermonsWPF.Data;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class MSViewer:UserControl
    {
        private Sermon sermon;
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
                return this.BaseViewerTitle.Text;
            }
            private set
            {
                this.BaseViewerTitle.Text = value;
            }
        }
        /// <summary>
        /// Rich text content string.
        /// </summary>
        public string ViewerContent
        {
            get
            {
                return this.documentManager.GetRichText(DataFormats.Rtf);
            }
            private set
            {
                this.documentManager.SetRichText(value, DataFormats.Rtf);
            }
        }
        /// <summary>
        /// Viewer constructor.
        /// </summary>
        /// <param name="sermon">Sermon to be displayed.</param>
        public MSViewer(Sermon sermon) : base()
        {
            this.InitializeComponent();
            this.documentManager = new MSDocumentManager(this.BaseViewerContent);
            this.sermon = sermon;
            if(sermon != null)
            {
                this.ViewerTitle = this.sermon.Title;
                this.ViewerContent = this.sermon.Content;
            }
        }
        public Sermon GetSermon()
        {
            return sermon;
        }
        private void PrintCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Printing...");
        }
        private void PrintCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.sermon != null ? true : false;
        }
        private void DeleteCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if(this.sermon.Delete())
            {
                MessageBox.Show("Deleting...");
            }
        }
        private void DeleteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.sermon != null ? true : false;
        }
        private void SaveAsCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Saving as...");
        }
        private void SaveAsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.sermon != null ? true : false;
        }
    }
}
