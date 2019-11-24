using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MySermonsWPF.Data;
using MySermonsWPF.Data.Bible;

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
        private void DetectVerses()
        {
            //var allText = GetRichText(DataFormats.Text);
            var allText = sermon.Content;
            var matches = Regex.Matches(allText, @"\w{1,3}\.\d{1,3}\.\d{1,3}\-\w{1,3}\.\d{1,3}\.\d{1,3}|\w{1,3}\.\d{1,3}\.\d{1,3}");
            foreach (Match match in matches)
            {
                var text = match.Value;
                var comb = string.Empty;
                int iPosRange = text.IndexOf('-');
                if (iPosRange != -1)
                {
                    var startString = text.Remove(iPosRange);
                    startString = startString.Replace("-", string.Empty);
                    var startBook = startString.Remove(startString.IndexOf("."));
                    startBook = SentenceCase(startBook);
                    var startChapter = startString.Remove(0, startString.IndexOf(".") + 1);
                    startChapter = startChapter.Remove(startChapter.IndexOf("."));
                    var startVerse = startString.Remove(0, startString.LastIndexOf(".") + 1);

                    startString = text.Remove(0, (iPosRange + 1));
                    var endBook = startString.Remove(startString.IndexOf("."));
                    endBook = SentenceCase(endBook);
                    var endChapter = startString.Remove(0, startString.IndexOf(".") + 1);
                    endChapter = endChapter.Remove(endChapter.IndexOf("."));
                    var endVerse = startString.Remove(0, startString.LastIndexOf(".") + 1);

                    startString = text.Remove(iPosRange);

                    if (startBook == endBook)
                    {
                        if (startChapter == endChapter)
                        {
                            comb = startBook + " " + startChapter + ":" + startVerse + "-" + endVerse;
                        }
                        else
                        {
                            comb = startBook + " " + startChapter + ":" + startVerse + "-" + endChapter + ":" + endVerse;
                        }
                    }
                    else
                    {
                        comb = startBook + " " + startChapter + ":" + startVerse + "-" + endBook + " " + endChapter + ":" + endVerse;
                    }
                }
                else
                {
                    var bcv = text;
                    var startbcv = bcv;
                    var startBook = bcv.Remove(bcv.IndexOf("."));
                    startBook = SentenceCase(startBook);
                    var startChapter = bcv.Remove(0, bcv.IndexOf(".") + 1);
                    startChapter = startChapter.Remove(startChapter.IndexOf("."));
                    var startVerse = bcv.Remove(0, bcv.LastIndexOf(".") + 1);

                    comb = startBook + " " + startChapter + ":" + startVerse;
                }
                if (text != comb) Replace(text, comb);
            }
        }
        string SentenceCase(string original)
        {
            original = original.ToLower();
            var array = original.ToCharArray();
            for (int i = 0; i < original.Length; i++)
            {
                if (char.IsLetter(array[i]))
                {
                    array[i] = char.ToUpper(original[i]);
                    break;
                }
            }
            return new string(array);
        }
        private void Replace(string oldString, string newString)
        {
            TextRange text = new TextRange(BaseViewerContent.Document.ContentStart, BaseViewerContent.Document.ContentEnd);
            TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
            while (current != null)
            {
                string textInRun = current.GetTextInRun(LogicalDirection.Forward);
                if (!string.IsNullOrWhiteSpace(textInRun))
                {
                    int index = textInRun.IndexOf(oldString);
                    if (index != -1)
                    {
                        TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                        TextPointer selectionEnd = selectionStart.GetPositionAtOffset(oldString.Length, LogicalDirection.Forward);
                        TextRange selection = new TextRange(selectionStart, selectionEnd);
                        selection.Text = newString;
                        selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        BaseViewerContent.Selection.Select(selection.Start, selection.End);
                        BaseViewerContent.Focus();
                    }
                }
                current = current.GetNextContextPosition(LogicalDirection.Forward);
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
