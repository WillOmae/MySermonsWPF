using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Rich text document manager.
    /// </summary>
    public class MSDocumentManager
    {
        /// <summary>
        /// The RichTextBox control to be managed.
        /// </summary>
        private readonly RichTextBox rtb;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox control to be managed.</param>
        public MSDocumentManager(RichTextBox richTextBox)
        {
            this.rtb = richTextBox;
        }

        /// <summary>
        /// Convert the RichTextBox text into rich text in a specified format (currently rtf).
        /// </summary>
        /// <returns>A string representation of the rich text.</returns>
        public string GetRichText(string dataFormat)
        {
            TextRange textRange = new TextRange(this.rtb.Document.ContentStart, this.rtb.Document.ContentEnd);
            if(textRange.Text.Trim().Length > 0)
            {
                using(var stream = new MemoryStream())
                {
                    textRange.Save(stream, dataFormat);
                    this.ResetStream(stream);
                    StringBuilder stringBuilder = new StringBuilder();
                    int b;
                    while((b = stream.ReadByte()) != -1)
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
            if(!string.IsNullOrEmpty(richText))
            {
                using(Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(richText)))
                {
                    TextRange textRange = new TextRange(this.rtb.Document.ContentStart, this.rtb.Document.ContentEnd);
                    textRange.Load(stream, dataFormat);
                }
            }
        }

        /// <summary>
        /// Checks whether content is empty.
        /// </summary>
        /// <returns>Is content empty?</returns>
        public bool IsEmpty()
        {
            // check whether content is null, empty or whitespace
            if(this.rtb != null)
            {
                var content = new TextRange(this.rtb.Document.ContentStart, this.rtb.Document.ContentEnd).Text;
                return string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Insert text at the caret position.
        /// </summary>
        /// <param name="text">Text to insert.</param>
        public void Insert(string text)
        {
            // insert plain text from the clipboard to the caret position
            this.rtb.CaretPosition.InsertTextInRun(text);
        }

        /// <summary>
        /// Set the position of the stream to the beginning.
        /// </summary>
        /// <param name="stream">The stream to be reset.</param>
        private void ResetStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
