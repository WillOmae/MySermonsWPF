using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using MySermonsWPF.Data;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSRichTextBoxAdv.xaml
    /// </summary>
    public partial class MSRichTextBoxAdv : UserControl
    {
        /// <summary>
        /// Sermon that is to be manipulated internally.
        /// </summary>
        private Sermon sermon = null;
        private BitmapImage closeIcon = new BitmapImage(new Uri("pack://application:,,,/MySermons;component/UI/Resources/collapse.png"));
        private BitmapImage openIcon = new BitmapImage(new Uri("pack://application:,,,/MySermons;component/UI/Resources/expand.png"));
        private string closePanelTooltip = "Close details panel.";
        private string openPanelTooltip = "Open details panel";
        /// <summary>
        /// Single chapter e.g. Hebrews 1
        /// </summary>
        private const string regexBC = @"\d?\s*[a-zA-Z]{1,}\s*\d{1,3}";
        /// <summary>
        /// Single verse e.g. Hebrews 11:1
        /// </summary>
        private const string regexBCV = @"\d?\s*[a-zA-Z]{1,}\s*\d{1,3}\s*:\s*\d{1,3}";
        /// <summary>
        /// Range of verses within a chapter e.g. Psalms 119:105 - 150
        /// </summary>
        private const string regexBCVrV = @"\d?\s*[a-zA-Z]{1,}\s*\d{1,3}\s*:\s*\d{1,3}\s*-\s*\d{1,3}";
        /// <summary>
        /// Range of chapters within the same book e.g. 1John 1 - 3
        /// </summary>
        private const string regexBCrC = @"\d?\s*[a-zA-Z]{1,}\s*\d{1,3}\s*-\s*\d{1,3}";
        /// <summary>
        /// Range of verses across chapters of the same book e.g. Revelation 20:10 - 21:10
        /// </summary>
        private const string regexBCVrCV = @"\d?\s*[a-zA-Z]{1,}\s*\d{1,3}\s*:\s*\d{1,3}-\s*\d{1,3}\s*:\s*\d{1,3}";
        /// <summary>
        /// Range of chapters across books e.g. 2John 1 - 3John 1
        /// </summary>
        private const string regexBCrBC = @"\d?\s*[a-zA-Z]{1,}\s*\d{1,3}\s*-\d?\s*[a-zA-Z]{1,}\s*\d{1,3}";
        /// <summary>
        /// Range of verses across chapters across books e.g. 2John 1:1 - 3John 1:3
        /// </summary>
        private const string regexBCVrBCV = @"\d?\s*[a-zA-Z]{1,}\s*\d{1,3}\s*:\s*\d{1,3}-\d?\s*[a-zA-Z]{1,}\s*\d{1,3}\s*:\s*\d{1,3}";

        /// <summary>
        /// Constructor accepting a parameter of type sermon.
        /// </summary>
        /// <param name="sermon">The sermon object to be manipulated.</param>
        public MSRichTextBoxAdv(Sermon sermon)
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
            if (this.sermon != null)
            {
                this.SetUpEditor();
            }
        }
        private string GetRTBContents(FormatType dataFormat)
        {
            using (var stream = new MemoryStream())
            {
                BaseRichTextBox.Save(stream, dataFormat);
                stream.Position = 0;
                byte[] textBytes = new byte[stream.Length];
                stream.Read(textBytes, 0, textBytes.Length);
                return Encoding.UTF8.GetString(textBytes, 0, textBytes.Length);
            }
        }
        private void SetRTBContents(FormatType format, string message)
        {
            using (var stream = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                BaseRichTextBox.Load(stream, format);
            }
        }
        private void SaveCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Save();
        }
        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // you can save text when there is text, right?
            e.CanExecute = true;
        }
        private void OpenCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            switch (this.BaseMetadataPanel.Visibility)
            {
                case Visibility.Collapsed:
                    this.ToggleMetadataPanelOpening(MetadataPanelToggle.Open);
                    break;
                case Visibility.Visible:
                    this.ToggleMetadataPanelOpening(MetadataPanelToggle.Close);
                    break;
            }
        }
        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void BaseRichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.ToggleMetadataPanelOpening(MetadataPanelToggle.Close);
        }
        private void ToggleMetadataPanelOpening(MetadataPanelToggle toggle)
        {
            switch (toggle)
            {
                case MetadataPanelToggle.Open:
                    this.BaseMetadataPanel.Visibility = Visibility.Visible;
                    this.ExpandMetadataPanel.ToolTip = this.closePanelTooltip;
                    this.ExpandMetadataPanelImage.Source = this.closeIcon;
                    break;
                case MetadataPanelToggle.Close:
                    this.BaseMetadataPanel.Visibility = Visibility.Collapsed;
                    this.ExpandMetadataPanel.ToolTip = this.openPanelTooltip;
                    this.ExpandMetadataPanelImage.Source = this.openIcon;
                    break;
            }
        }
        public Sermon GetSermon()
        {
            return this.sermon;
        }
        private void SetUpEditor()
        {
            StringBuilder themesBuilder = new StringBuilder();
            foreach (var theme in this.sermon.Themes)
            {
                if (theme.Name != "THEME_NOT_SET")
                {
                    themesBuilder = themesBuilder.Append(theme.Name).Append(", ");
                }
            }
            string themes = themesBuilder.ToString().TrimEnd(',', ' ');
            this.BaseMetadataPanel.Populate(this.sermon.Title, null, this.sermon.KeyVerse, this.sermon.Location.Name, themes, this.sermon.OtherMetaData);
            SetRTBContents(FormatType.Rtf, this.sermon.Content == "CONTENT_NOT_SET" ? string.Empty : this.sermon.Content);
        }
        private void Save()
        {
            if (this.BaseMetadataPanel.Verify())
            {
                char themeDelimiter = ',';
                (string title, string speaker, string keytext, string location, string themes, string otherinfo) metadata = this.BaseMetadataPanel.GetMetadata();

                Location location = string.IsNullOrEmpty(metadata.location) ? null : new Location(metadata.location, StringType.Name);
                List<Theme> themes = string.IsNullOrEmpty(metadata.themes) ? null : Theme.ExtractFromDelimitedString(metadata.themes, themeDelimiter);
                string content = GetRTBContents(FormatType.Rtf);
                string title = string.IsNullOrEmpty(metadata.title) ? null : metadata.title;
                string keyText = string.IsNullOrEmpty(metadata.keytext) ? null : metadata.keytext;
                string otherMetadata = string.IsNullOrEmpty(metadata.otherinfo) ? null : metadata.otherinfo;

                if (this.sermon == null)
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
            else
            {
                this.ToggleMetadataPanelOpening(MetadataPanelToggle.Open);
                MessageBox.Show("Specify sermon title", "Title not set", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        private enum MetadataPanelToggle
        {
            Open, Close
        }

        private void ExpandMetadataPanel_Click(object sender, RoutedEventArgs e)
        {
            switch (this.BaseMetadataPanel.Visibility)
            {
                case Visibility.Collapsed:
                    this.ToggleMetadataPanelOpening(MetadataPanelToggle.Open);
                    break;
                case Visibility.Visible:
                    this.ToggleMetadataPanelOpening(MetadataPanelToggle.Close);
                    break;
            }
        }
    }
}
