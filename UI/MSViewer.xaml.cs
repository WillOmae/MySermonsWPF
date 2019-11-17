using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MySermonsWPF.Data;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class MSViewer : UserControl
    {
        private Sermon sermon;
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
                return GetRichText(DataFormats.Rtf);
            }
            private set
            {
                SetRichText(value, DataFormats.Rtf);
            }
        }
        /// <summary>
        /// Viewer constructor.
        /// </summary>
        /// <param name="sermon">Sermon to be displayed.</param>
        public MSViewer(Sermon sermon) : base()
        {
            this.InitializeComponent();
            this.sermon = sermon;
            if (sermon != null)
            {
                this.ViewerTitle = this.sermon.Title;
                this.ViewerContent = this.sermon.Content;
            }
        }
        public Sermon GetSermon()
        {
            return sermon;
        }
        /// <summary>
        /// Convert the RichTextBox text into rich text in a specified format (currently rtf).
        /// </summary>
        /// <returns>A string representation of the rich text.</returns>
        public string GetRichText(string dataFormat)
        {
            TextRange textRange = new TextRange(this.BaseViewerContent.Document.ContentStart, this.BaseViewerContent.Document.ContentEnd);
            if (textRange.Text.Trim().Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    textRange.Save(stream, dataFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    StringBuilder stringBuilder = new StringBuilder();
                    int b;
                    while ((b = stream.ReadByte()) != -1)
                    {
                        stringBuilder.Append(Convert.ToChar(b));
                    }
                    return stringBuilder.ToString();
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets the RichTextBox text from rich text in a specified format (currently rtf).
        /// </summary>
        /// <param name="richText">The string representation of the rich text.</param>
        public void SetRichText(string richText, string dataFormat)
        {
            if (!string.IsNullOrEmpty(richText))
            {
                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(richText)))
                {
                    TextRange textRange = new TextRange(this.BaseViewerContent.Document.ContentStart, this.BaseViewerContent.Document.ContentEnd);
                    textRange.Load(stream, dataFormat);
                }
            }
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
            if (this.sermon.Delete())
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
