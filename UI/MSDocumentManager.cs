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
        public MSDocumentManager(RichTextBox richTextBox) => this.rtb = richTextBox;

        /// <summary>
        /// Convert the RichTextBox text into rich text in a specified format (currently rtf).
        /// </summary>
        /// <returns>A string representation of the rich text.</returns>
        public string GetRichText()
        {
            TextRange textRange = new TextRange(this.rtb.Document.ContentStart, this.rtb.Document.ContentEnd);
            if(textRange.Text.Trim().Length > 0)
            {
                using(var stream = new MemoryStream())
                {
                    textRange.Save(stream, DataFormats.Rtf);
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
        public void SetRichText(string richText)
        {
            if(!string.IsNullOrEmpty(richText))
            {
                using(Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(richText)))
                {
                    TextRange textRange = new TextRange(this.rtb.Document.ContentStart, this.rtb.Document.ContentEnd);
                    textRange.Load(stream, DataFormats.Rtf);
                }
            }
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
