using MySermonsWPF.Data;
using MySermonsWPF.Data.Bible;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using Syncfusion.Windows.Controls.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        private const string regexBC = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3}\b";
        /// <summary>
        /// Single verse e.g. Hebrews 11:1
        /// </summary>
        private const string regexBCV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}\b";
        /// <summary>
        /// Range of verses within a chapter e.g. Psalms 119:105 - 150
        /// </summary>
        private const string regexBCVrV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3} *- *\d{1,3}\b";
        /// <summary>
        /// Range of chapters within the same book e.g. 1John 1 - 3
        /// </summary>
        private const string regexBCrC = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *- *\d{1,3}\b";
        /// <summary>
        /// Range of verses across chapters of the same book e.g. Revelation 20:10 - 21:10
        /// </summary>
        private const string regexBCVrCV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}- *\d{1,3} *: *\d{1,3}\b";
        /// <summary>
        /// Range of chapters across books e.g. 2John 1 - 3John 1
        /// </summary>
        private const string regexBCrBC = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *-(\d *)?[a-zA-Z]{1,} *\d{1,3}\b";
        /// <summary>
        /// Range of verses across chapters across books e.g. 2John 1:1 - 3John 1:3
        /// </summary>
        private const string regexBCVrBCV = @"\b(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}-(\d *)?[a-zA-Z]{1,} *\d{1,3} *: *\d{1,3}\b";

        private readonly Regex combinedRegex;
        private readonly XmlBible xmlBible;

        /// <summary>
        /// Constructor accepting a parameter of type sermon.
        /// </summary>
        /// <param name="sermon">The sermon object to be manipulated.</param>
        public MSRichTextBoxAdv(Sermon sermon)
        {
            this.InitializeComponent();
            this.sermon = sermon;
            this.xmlBible = new XmlBible();
            this.combinedRegex = new Regex(regexBCVrBCV + "|" + regexBCrBC + "|" + regexBCVrCV + "|" + regexBCrC + "|" + regexBCVrV + "|" + regexBCV + "|" + regexBC, RegexOptions.Compiled);
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
            BaseRichTextBox.KeyUp += this.BaseRichTextBox_KeyUp;
            BaseRichTextBox.PreviewMouseLeftButtonUp += this.BaseRichTextBox_PreviewMouseLeftButtonUp;
        }

        private void BaseRichTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(BaseRichTextBox);
        }
        private void DetectVerses()
        {
            string text = GetRTBContents(FormatType.Txt);
            var matches = combinedRegex.Matches(text);

            foreach (Match match in matches)
            {
                var verses = xmlBible.Parse(match.Value);
                if (verses != null)
                {
                    var positions = BaseRichTextBox.FindAll(match.Value, FindOptions.None);
                    if (positions != null)
                    {
                        for (int i = 0; i < positions.Count; i++)
                        {
                            var position = positions[i];
                            var hierarchicalIndex = position.Start.HierarchicalIndex;
                            var splits = hierarchicalIndex.Split(';');
                            var sectionIndex = int.Parse(splits[0]);
                            var blockIndex = int.Parse(splits[1]);
                            var inlineIndex = int.Parse(splits[2]);
                            var section = BaseRichTextBox.Document.Sections[sectionIndex];

                            if (section.Blocks[blockIndex] is ParagraphAdv paragraphAdv)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (Inline inline in paragraphAdv.Inlines)
                                {
                                    if (inline is SpanAdv inlineSpan)
                                    {
                                        stringBuilder.Append(inlineSpan.Text);
                                    }
                                }
                                string posText = position.Text;
                                int index = stringBuilder.ToString().IndexOf(posText);
                                if (index != -1)
                                {
                                    int foundLength = 0;
                                    string[] parts = posText.Split('-', ',', ' ', ';', ':');
                                    int countTotal = 2 * parts.Length - 1;
                                    for (int j = 0; j < paragraphAdv.Inlines.Count; j++)
                                    {
                                        if (paragraphAdv.Inlines[j] is SpanAdv inlineSpan)
                                        {
                                            foundLength += inlineSpan.Text.Length;
                                            if (foundLength > index && foundLength <= (index + posText.Length))
                                            {
                                                paragraphAdv.Inlines.Insert(j++, new FieldBeginAdv());
                                                paragraphAdv.Inlines.Insert(j++, new SpanAdv
                                                {
                                                    Text = " HYPERLINK \"" + posText + "\" "
                                                });
                                                paragraphAdv.Inlines.Insert(j++, new FieldSeparatorAdv());
                                                SpanAdv fieldResult = new SpanAdv
                                                {
                                                    Text = posText
                                                };
                                                fieldResult.CharacterFormat.Underline = Underline.Single;
                                                fieldResult.CharacterFormat.FontColor = Color.FromArgb(0xff, 0x05, 0x63, 0xc1);
                                                paragraphAdv.Inlines.Insert(j++, fieldResult);
                                                paragraphAdv.Inlines.Insert(j++, new FieldEndAdv());
                                                for (int k = countTotal - 1; k >= 0 && (j + k) >= 0 && (j + k) < paragraphAdv.Inlines.Count; k--)
                                                {
                                                    paragraphAdv.Inlines.RemoveAt(j + k);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void BaseRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPeriod)
            {
                var currentStart = BaseRichTextBox.Selection.Start;
                var currentEnd = BaseRichTextBox.Selection.End;

                DetectVerses();

                BaseRichTextBox.Selection.Select(currentStart, currentEnd);
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
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
        private void PrintCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("printing...");
        }
        private void PrintCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void PastePlainCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //this.documentManager.Insert(Clipboard.GetText());
        }
        private void PastePlainCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
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
