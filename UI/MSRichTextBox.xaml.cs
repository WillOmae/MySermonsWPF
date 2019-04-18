using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MySermonsWPF.Data;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSRichTextBox.xaml
    /// </summary>
    public partial class MSRichTextBox:UserControl
    {
        /// <summary>
        /// Sermon that is to be manipulated internally.
        /// </summary>
        private Sermon sermon;
        /// <summary>
        /// Constructor accepting a parameter of type sermon.
        /// </summary>
        /// <param name="sermon">The sermon object to be manipulated.</param>
        public MSRichTextBox(Sermon sermon)
        {
            this.InitializeComponent();
            this.sermon = sermon;
        }
        /// <summary>
        /// Event handler when all controls have been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MSRichTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.SetButtonsProperties();
        }
        /// <summary>
        /// Set the properties of buttons in bulk.
        /// </summary>
        private void SetButtonsProperties()
        {
            // first, get the buttons.
            foreach(var button in MSFindVisualChildren.FindVisualChildren<Button>(this.BaseFormattingBar))
            {
                // prevent buttons from retaining focus; always pass focus back to the rtb;
                button.Click += (sender, eventArgs) => this.BaseRichTextBox.Focus();
            }
        }

        private void FindCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("finding...");
        }
        private void FindCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // you can search for text when there is text, right?
            e.CanExecute = !this.IsRTBEmpty();
        }
        private void PastePlainCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            // insert plain text from the clipboard to the caret position
            this.BaseRichTextBox.CaretPosition.InsertTextInRun(Clipboard.GetText());
        }
        private void PastePlainCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
        }
        /// <summary>
        /// Checks whether rtb is empty.
        /// </summary>
        /// <returns>Is rtb empty?</returns>
        private bool IsRTBEmpty()
        {
            // check whether content is null, empty or whitespace
            return this.BaseRichTextBox != null
                ? string.IsNullOrEmpty(new TextRange(this.BaseRichTextBox.Document.ContentStart, this.BaseRichTextBox.Document.ContentEnd).Text) || string.IsNullOrWhiteSpace(new TextRange(this.BaseRichTextBox.Document.ContentStart, this.BaseRichTextBox.Document.ContentEnd).Text)
                : true;
        }
    }
}
