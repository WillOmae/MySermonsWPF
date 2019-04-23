using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private Sermon sermon = null;
        private MSDocumentManager documentManager = null;
        private BitmapImage closeIcon = new BitmapImage(new Uri("pack://application:,,,/MySermons;component/UI/Resources/Formatbar/close2.png"));
        private BitmapImage openIcon = new BitmapImage(new Uri("pack://application:,,,/MySermons;component/UI/Resources/Formatbar/open.png"));

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
            this.documentManager = new MSDocumentManager(this.BaseRichTextBox);
            this.SetButtonsProperties();
            this.RTBFontSize.ItemsSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            this.RTBFontSize.SelectedItem = 12D;
            this.RTBFont.SelectedItem = new FontFamily("Arial");
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
            // first, get the comboboxes
            foreach(var comboBox in MSFindVisualChildren.FindVisualChildren<ComboBox>(this.BaseFormattingBar))
            {
                // prevent comboboxes from retaining focus; always pass focus back to the rtb;
                comboBox.SelectionChanged += (sender, eventArgs) => this.BaseRichTextBox.Focus();
            }
        }

        private void SaveCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Save();
        }

        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // you can save text when there is text, right?
            e.CanExecute = this.documentManager != null ? !this.documentManager.IsEmpty() : false;
        }

        private void OpenCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            switch(this.MetadataPanel.Visibility)
            {
                case Visibility.Collapsed:
                    this.MetadataPanel.Visibility = Visibility.Visible;
                    this.RTBMetadataIcon.Source = this.closeIcon;
                    break;
                case Visibility.Visible:
                    this.MetadataPanel.Visibility = Visibility.Collapsed;
                    this.RTBMetadataIcon.Source = this.openIcon;
                    break;
            }
        }

        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PrintCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("printing...");
        }

        private void PrintCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.documentManager != null ? !this.documentManager.IsEmpty() : false;
        }

        private void FindCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("finding...");
        }

        private void FindCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // you can search for text when there is text, right?
            e.CanExecute = this.documentManager != null ? !this.documentManager.IsEmpty() : false;
        }

        private void PastePlainCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.documentManager.Insert(Clipboard.GetText());
        }

        private void PastePlainCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.documentManager != null && Clipboard.ContainsText();
        }

        private void RTBFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(double item in e.AddedItems)
            {
                // apply the font size selected
                this.BaseRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, item);
            }
        }

        private void RTBFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(FontFamily item in e.AddedItems)
            {
                // apply the font selected
                this.BaseRichTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, item.Source);
            }
        }

        private void Save()
        {
            if(string.IsNullOrEmpty(this.MetaTitle.Text))
            {
                MessageBox.Show("Specify sermon title", "Title not set", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                this.MetadataPanel.Visibility = Visibility.Visible;
                this.RTBMetadataIcon.Source = this.closeIcon;
            }
            else
            {
                Location location = string.IsNullOrEmpty(this.MetaLocation.Text) ? null : new Location(this.MetaLocation.Text, StringType.Name);
                List<Theme> themes = string.IsNullOrEmpty(this.MetaThemes.Text) ? null : new List<Theme>() { new Theme(this.MetaThemes.Text, StringType.Name) };
                string content = this.documentManager.IsEmpty() ? null : this.documentManager.GetRichText();
                string title = string.IsNullOrEmpty(this.MetaTitle.Text) ? null : this.MetaTitle.Text;
                string keyText = string.IsNullOrEmpty(this.MetaKeyText.Text) ? null : this.MetaKeyText.Text;
                string otherMetadata = string.IsNullOrEmpty(this.MetaOtherInfo.Text) ? null : this.MetaOtherInfo.Text;
                if(this.sermon == null)
                {
                    // the sermon does not exist; create
                    this.sermon = new Sermon(title, location, themes, keyText, otherMetadata, content);
                    MessageBox.Show("Creation success: " + this.sermon.Create());
                }
                else
                {
                    // the sermon exists; update
                    var id = this.sermon.ID;
                    var guid = this.sermon.GUID;
                    var dateCreated = this.sermon.DateCreated.Ticks;
                    var lastAccessed = DateTime.Now.Ticks;
                    this.sermon = new Sermon(id, guid, title, location.ID, location.Name, location, themes, dateCreated, lastAccessed, keyText, otherMetadata, content);
                    MessageBox.Show("Update successful: " + this.sermon.Update());
                }
            }
        }
    }
}
