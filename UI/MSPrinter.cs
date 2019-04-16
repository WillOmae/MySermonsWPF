using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MySermonsWPF.Data;

namespace MySermonsWPF.UI
{
    public class MSPrinter
    {
        private Sermon sermon;
        public MSPrinter(Sermon sermon)
        {
            this.sermon = sermon;
        }
        public void Print()
        {
            if(this.sermon != null && !string.IsNullOrEmpty(this.sermon.Content))
            {
                FlowDocument flowDocument = new FlowDocument();
                using(Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(this.sermon.Content)))
                {
                    TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    textRange.Load(stream, DataFormats.Rtf);
                }
                PrintDialog printDialog = new PrintDialog();
                if(printDialog.ShowDialog() == true)
                {
                    printDialog.PrintDocument(((IDocumentPaginatorSource)(flowDocument)).DocumentPaginator, "Trial");
                }
            }
        }
    }
}