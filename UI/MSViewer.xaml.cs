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
                DetectVerses();
            }
        }
        private void DetectVerses()
        {
            string regexBC = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3}\b";
            string regexBCV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}\b";
            string regexBCVrV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3} *- *\d{1,3}\b";
            string regexBCrC = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *- *\d{1,3}\b";
            string regexBCVrCV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}- *\d{1,3} *: *\d{1,3}\b";
            string regexBCrBC = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *-(\d *)?[a-zA-Z]{1,} *\d{1,3}\b";
            string regexBCVrBCV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}-(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}\b";
            Regex combinedRegex = new Regex(regexBCVrBCV + "|" + regexBCrBC + "|" + regexBCVrCV + "|" + regexBCrC + "|" + regexBCVrV + "|" + regexBCV + "|" + regexBC, RegexOptions.Compiled);
            string text = GetRichText(DataFormats.Text);
            MatchCollection matches = combinedRegex.Matches(text);
            if (matches == null) return;
            XmlBible xmlBible = new XmlBible();
            foreach (Match match in matches)
            {
                if (match == null) continue;
                List<BibleVerse> list = xmlBible.Parse(match.Value);
                if (list == null && list.Count < 1) continue;
                int start = text.IndexOf(match.Value);
                int length = match.Value.Length;

                TextPointer tp = BaseViewerContent.Document.ContentStart;
                TextPointer tpLeft = GetPositionAtOffset(tp, start, LogicalDirection.Forward);
                TextPointer tpRight = GetPositionAtOffset(tp, start + length, LogicalDirection.Forward);
                TextRange textRange = new TextRange(tpLeft, tpRight);
                Hyperlink inline = new Hyperlink(textRange.Start, textRange.End);
                inline.NavigateUri = new Uri("http://www.google.com");
            }
        }

        private TextPointer GetPositionAtOffset(TextPointer startingPoint, int offset, LogicalDirection direction)
        {
            TextPointer binarySearchPoint1 = null;
            TextPointer binarySearchPoint2 = null;
            // setup arguments appropriately
            if (direction == LogicalDirection.Forward)
            {
                binarySearchPoint2 = BaseViewerContent.Document.ContentEnd;
                if (offset < 0) offset = Math.Abs(offset);
            }
            if (direction == LogicalDirection.Backward)
            {
                binarySearchPoint2 = BaseViewerContent.Document.ContentStart;
                if (offset > 0) offset = -offset;
            }
            // setup for binary search
            bool isFound = false;
            TextPointer resultTextPointer = null;
            int offset2 = Math.Abs(GetOffsetInTextLength(startingPoint, binarySearchPoint2));
            int halfOffset = direction == LogicalDirection.Backward ? -(offset2 / 2) : offset2 / 2;
            binarySearchPoint1 = startingPoint.GetPositionAtOffset(halfOffset, direction);
            int offset1 = Math.Abs(GetOffsetInTextLength(startingPoint, binarySearchPoint1));
            // binary search loop
            while (isFound == false)
            {
                if (Math.Abs(offset1) == Math.Abs(offset))
                {
                    isFound = true;
                    resultTextPointer = binarySearchPoint1;
                }
                else if (Math.Abs(offset2) == Math.Abs(offset))
                {
                    isFound = true;
                    resultTextPointer = binarySearchPoint2;
                }
                else
                {
                    if (Math.Abs(offset) < Math.Abs(offset1))
                    {
                        // this is simple case when we search in the 1st half
                        binarySearchPoint2 = binarySearchPoint1;
                        offset2 = offset1;
                        halfOffset = direction == LogicalDirection.Backward ? -(offset2 / 2) : offset2 / 2;
                        binarySearchPoint1 = startingPoint.GetPositionAtOffset(halfOffset, direction);
                        offset1 = Math.Abs(GetOffsetInTextLength(startingPoint, binarySearchPoint1));
                    }
                    else
                    {
                        // this is more complex case when we search in the 2nd half
                        int rtfOffset1 = startingPoint.GetOffsetToPosition(binarySearchPoint1);
                        int rtfOffset2 = startingPoint.GetOffsetToPosition(binarySearchPoint2);
                        int rtfOffsetMiddle = (Math.Abs(rtfOffset1) + Math.Abs(rtfOffset2)) / 2;
                        if (direction == LogicalDirection.Backward) rtfOffsetMiddle = -rtfOffsetMiddle;
                        TextPointer binarySearchPointMiddle = startingPoint.GetPositionAtOffset(rtfOffsetMiddle, direction);
                        int offsetMiddle = GetOffsetInTextLength(startingPoint, binarySearchPointMiddle);
                        // two cases possible
                        if (Math.Abs(offset) < Math.Abs(offsetMiddle))
                        {
                            // 3rd quarter of search domain
                            binarySearchPoint2 = binarySearchPointMiddle;
                            offset2 = offsetMiddle;
                        }
                        else
                        {
                            // 4th quarter of the search domain
                            binarySearchPoint1 = binarySearchPointMiddle;
                            offset1 = offsetMiddle;
                        }
                    }
                }
            }
            return resultTextPointer;
        }
        int GetOffsetInTextLength(TextPointer pointer1, TextPointer pointer2)
        {
            if (pointer1 == null || pointer2 == null)
                return 0;
            TextRange tr = new TextRange(pointer1, pointer2);
            return tr.Text.Length;
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
