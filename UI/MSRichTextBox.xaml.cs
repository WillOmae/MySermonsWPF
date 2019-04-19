using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
        private Sermon sermon = null;
        private MSDocumentManager documentManager = null;

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

        private void SaveCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Save();
        }

        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // you can search for text when there is text, right?
            e.CanExecute = this.documentManager != null ? !this.documentManager.IsEmpty() : false;
        }

        private void SaveAsCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("saving as...");
        }

        private void SaveAsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.documentManager != null ? !this.documentManager.IsEmpty() : false;
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

        private void Save()
        {
            Location location = new Location("Juja", StringType.Name);
            List<Theme> themes = new List<Theme>()
            {
                new Theme("salvation",StringType.Name),
                new Theme("history",StringType.Name),
                new Theme("faith",StringType.Name),
                new Theme("eschatology",StringType.Name),
                new Theme("hermeneutics",StringType.Name),
                new Theme("sanctuary",StringType.Name)
            };
            var content = this.documentManager.GetRichText();
            var title = "New Sermon: " + DateTime.Now.ToShortTimeString();
            var keyText = "Gal 3:24";
            if(this.sermon == null)
            {
                // the sermon does not exist; create
                this.sermon = new Sermon(title, location, themes, keyText, string.Empty, content);
                MessageBox.Show("Creation success: " + this.sermon.Create());
            }
            else
            {
                // the sermon exists; update
                var id = this.sermon.ID;
                var guid = this.sermon.GUID;
                this.sermon = new Sermon(id, guid, title, location.ID, location.Name, location, themes, this.sermon.DateCreated.Ticks, this.sermon.DateLastAccessed.Ticks, keyText, this.sermon.OtherMetaData, content);
                MessageBox.Show("Update successful: " + this.sermon.Update());
            }
        }
    }
}
