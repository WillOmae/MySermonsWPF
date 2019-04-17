using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MySermonsWPF.Data;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSRichTextBox.xaml
    /// </summary>
    public partial class MSRichTextBox:UserControl
    {
        private SolidColorBrush white = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        private SolidColorBrush grey = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        private Sermon sermon;
        public MSRichTextBox(Sermon sermon)
        {
            this.InitializeComponent();
            this.sermon = sermon;
        }

        private void MSRichTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.SetButtonsProperties();
            MessageBox.Show(this.RTBPastePlain.Background.ToString());
        }
        /// <summary>
        /// Set the properties of buttons in bulk.
        /// </summary>
        private void SetButtonsProperties()
        {
            // first, get the buttons.
            foreach(var button in MSFindVisualChildren.FindVisualChildren<Button>(this.BaseFormattingBar))
            {
                button.IsEnabledChanged += this.Button_IsEnabledChanged;
                button.Click += delegate
                {
                    this.BaseRichTextBox.Focus();
                };
            }
        }

        private void Button_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((Button)sender).Background = (bool)e.NewValue ? this.white : this.grey;
        }

        private void FindCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("finding...");
        }
        private void FindCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void PastePlainCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("pasting plain...");
        }
        private void PastePlainCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
