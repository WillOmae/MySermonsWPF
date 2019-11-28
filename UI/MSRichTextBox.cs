using DiffPlex;
using DiffPlex.Model;
using MySermonsWPF.Data;
using MySermonsWPF.Data.Bible;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSRichTextBox.xaml
    /// </summary>
    public partial class MSRichTextBox : UserControl
    {
        public string Rtf
        {
            get => GetRtbText(DataFormats.Rtf);
            set => SetRtbText(value, DataFormats.Rtf);
        }
        public string Text
        {
            get => GetRtbText(DataFormats.Text);
            set => SetRtbText(value, DataFormats.Text);
        }
        /// <summary>
        /// Sermon that is to be manipulated internally.
        /// </summary>
        public Sermon Sermon { get; private set; }
        private readonly BitmapImage closeIcon = new BitmapImage(new Uri("pack://application:,,,/MySermons;component/UI/Resources/collapse.png"));
        private readonly BitmapImage openIcon = new BitmapImage(new Uri("pack://application:,,,/MySermons;component/UI/Resources/expand.png"));
        private readonly string closePanelTooltip = "Close details panel.";
        private readonly string openPanelTooltip = "Open details panel";
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
        private string lastStringComposition = string.Empty;
        private readonly Differ differ;

        /// <summary>
        /// Constructor accepting a parameter of type sermon.
        /// </summary>
        /// <param name="sermon">The sermon object to be manipulated.</param>
        public MSRichTextBox(Sermon sermon)
        {
            this.InitializeComponent();
            this.Sermon = sermon;
            this.xmlBible = new XmlBible();
            this.combinedRegex = new Regex(regexBCVrBCV + "|" + regexBCrBC + "|" + regexBCVrCV + "|" + regexBCrC + "|" + regexBCVrV + "|" + regexBCV + "|" + regexBC, RegexOptions.Compiled);
            this.differ = new Differ();
        }
        /// <summary>
        /// Set the properties of buttons in bulk.
        /// </summary>
        private void SetButtonsProperties()
        {
            // first, get the buttons.
            foreach (var button in MSFindVisualChildren.FindVisualChildren<Button>(this.BaseFormattingBar))
            {
                // prevent buttons from retaining focus; always pass focus back to the rtb;
                button.Click += (sender, eventArgs) => this.BaseRichTextBox.Focus();
            }
            // first, get the comboboxes
            foreach (var comboBox in MSFindVisualChildren.FindVisualChildren<ComboBox>(this.BaseFormattingBar))
            {
                // prevent comboboxes from retaining focus; always pass focus back to the rtb;
                comboBox.SelectionChanged += (sender, eventArgs) => this.BaseRichTextBox.Focus();
            }
        }
        /// <summary>
        /// Determine verses that have been typed or pasted in.
        /// </summary>
        private void DetectVerses()
        {
            string currStringComposition = this.Text;
            DiffResult diffResults = differ.CreateCharacterDiffs(lastStringComposition, currStringComposition, false);
            lastStringComposition = currStringComposition;
            foreach (DiffBlock diffResult in diffResults.DiffBlocks.Where(diffResult => diffResult.InsertCountB > 0).Select(diffResult => diffResult))
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int diffIndex = diffResult.InsertStartB; diffIndex < diffResult.InsertStartB + diffResult.InsertCountB; diffIndex++)
                {
                    stringBuilder.Append(diffResults.PiecesNew[diffIndex]);
                }

                MatchCollection matches = this.combinedRegex.Matches(stringBuilder.ToString());
                if (matches == null) continue;
                foreach (Match match in matches)
                {
                    if (match == null) continue;
                    List<BibleVerse> list = this.xmlBible.Parse(match.Value);
                    if (list == null || list.Count < 1) continue;
                    TextRange haystack = new TextRange(BaseRichTextBox.Document.ContentStart, BaseRichTextBox.Document.ContentEnd);
                    List<TextRange> ranges = FindTextInRange(haystack, match.Value);
                    foreach (TextRange wordRange in ranges)
                    {
                        StringBuilder verseBuilder = new StringBuilder();
                        foreach (BibleVerse bibleVerse in list)
                        {
                            verseBuilder.Append(bibleVerse.BCV);
                            verseBuilder.Append(" ");
                            verseBuilder.Append(bibleVerse.Content);
                            verseBuilder.Append("\n");
                        }
                        Hyperlink hyperlink = new Hyperlink(wordRange.Start, wordRange.End)
                        {
                            ToolTip = new MSVersePopup()
                            {
                                VerseRef = match.Value,
                                VerseContent = verseBuilder.ToString().TrimEnd('\n')
                            }
                        };
                    }
                }
            }
        }
        /// <summary>
        /// Alter the state of the metadata panel.
        /// </summary>
        /// <param name="toggle"></param>
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
        /// <summary>
        /// Extract data from the sermon and populate necessary fields.
        /// </summary>
        private void SetUpEditor()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var theme in this.Sermon.Themes.Where(theme => theme.Name != "THEME_NOT_SET").Select(theme => theme))
            {
                builder = builder.Append(theme.Name).Append(", ");
            }
            string themes = builder.ToString().TrimEnd(',', ' ');
            builder.Clear();
            foreach (var speaker in this.Sermon.Speakers.Where(speaker => speaker.Name != "SPEAKER_NOT_SET").Select(speaker => speaker))
            {
                builder = builder.Append(speaker.Name).Append(", ");
            }

            string speakers = builder.ToString().TrimEnd(',', ' ');
            this.BaseMetadataPanel.Populate(this.Sermon.Title, speakers, this.Sermon.KeyVerse, this.Sermon.Location.Name, themes, this.Sermon.OtherMetaData);
            this.Rtf = this.Sermon.Content == "CONTENT_NOT_SET" ? string.Empty : this.Sermon.Content;
        }
        /// <summary>
        /// Permanently save changes made to the sermon.
        /// </summary>
        private void Save()
        {
            if (this.BaseMetadataPanel.Verify())
            {
                char themeDelimiter = ',';
                (string title, string speakers, string keytext, string location, string themes, string otherinfo) metadata = this.BaseMetadataPanel.GetMetadata();

                Location location = string.IsNullOrEmpty(metadata.location) ? new Location("LOCATION_NOT_SET", StringType.Name) : new Location(metadata.location, StringType.Name);
                List<Theme> themes = string.IsNullOrEmpty(metadata.themes) ? new List<Theme>() { new Theme("THEME_NOT_SET", StringType.Name) } : Theme.ExtractFromDelimitedString(metadata.themes, themeDelimiter);
                List<Speaker> speakers = string.IsNullOrEmpty(metadata.speakers) ? new List<Speaker>() { new Speaker("SPEAKER_NOT_SET", StringType.Name) } : Speaker.ExtractFromDelimitedString(metadata.speakers, themeDelimiter);
                string content = Rtf;
                string title = metadata.title;
                string keyText = string.IsNullOrEmpty(metadata.keytext) ? null : metadata.keytext;
                string otherMetadata = string.IsNullOrEmpty(metadata.otherinfo) ? null : metadata.otherinfo;

                if (this.Sermon == null)
                {
                    // the sermon does not exist; create
                    this.Sermon = new Sermon(title, location, themes, speakers, keyText, otherMetadata, content);
                    MessageBox.Show("Creation success: " + this.Sermon.Create());
                }
                else
                {
                    // the sermon exists; update
                    var id = this.Sermon.ID;
                    var guid = this.Sermon.GUID;
                    var dateCreated = this.Sermon.DateCreated.Ticks;
                    var lastAccessed = DateTime.Now.Ticks;
                    this.Sermon = new Sermon(id, guid, title, location.ID, location.Name, location, themes, speakers, dateCreated, lastAccessed, keyText, otherMetadata, content);
                    MessageBox.Show("Update successful: " + this.Sermon.Update());
                }
            }
            else
            {
                this.ToggleMetadataPanelOpening(MetadataPanelToggle.Open);
                MessageBox.Show("Specify sermon title", "Title not set", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        /// <summary>
        /// Convert the RichTextBox text into rich text in a specified format (currently rtf).
        /// </summary>
        /// <returns>A string representation of the rich text.</returns>
        private string GetRtbText(string dataFormat)
        {
            TextRange textRange = new TextRange(BaseRichTextBox.Document.ContentStart, BaseRichTextBox.Document.ContentEnd);
            if (textRange.Text.Trim().Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    textRange.Save(stream, dataFormat);
                    this.ResetStream(stream);
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
        private void SetRtbText(string richText, string dataFormat)
        {
            if (!string.IsNullOrEmpty(richText))
            {
                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(richText)))
                {
                    TextRange textRange = new TextRange(BaseRichTextBox.Document.ContentStart, BaseRichTextBox.Document.ContentEnd);
                    textRange.Load(stream, dataFormat);
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
        /// <summary>
        /// Checks whether content is empty.
        /// </summary>
        /// <returns>Is content empty?</returns>
        private bool IsEmpty()
        {
            //if (!BaseRichTextBox.IsLoaded) return false;
            //var content = new TextRange(BaseRichTextBox.Document.ContentStart, BaseRichTextBox.Document.ContentEnd).Text;
            //return string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content);
            return false;
        }
        /// <summary>
        /// Looks for all instances of a string within a range.
        /// </summary>
        /// <param name="range">The haystack</param>
        /// <param name="needle">The needle</param>
        /// <returns></returns>
        private List<TextRange> FindTextInRange(TextRange range, string needle)
        {
            List<TextRange> results = new List<TextRange>();
            TextPointer current = range.Start.GetInsertionPosition(LogicalDirection.Forward);
            while (current != null)
            {
                string haystack = current.GetTextInRun(LogicalDirection.Forward);
                int index;
                if (!string.IsNullOrWhiteSpace(haystack) && (index = haystack.IndexOf(needle)) != -1)
                {
                    TextPointer start = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                    TextPointer stop = start.GetPositionAtOffset(needle.Length, LogicalDirection.Forward);
                    TextRange result = new TextRange(start, stop);
                    results.Add(result);
                }
                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }
            return results;
        }
        /// <summary>
        /// Event handler when all controls have been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MSRichTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.SetButtonsProperties();
            this.RTBFontSize.ItemsSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            this.RTBFontSize.SelectedItem = 12D;
            this.RTBFont.ItemsSource = Fonts.SystemFontFamilies.Select(ff => ff.FamilyNames.Values.First()).OrderBy(ff => ff).ToList();
            this.RTBFont.SelectedItem = "Times New Roman";
            if (this.Sermon != null) this.SetUpEditor();
            BaseRichTextBox.KeyUp += this.BaseRichTextBox_KeyUp;
            BaseRichTextBox.FontFamily = new FontFamily("Times New Roman");
            BaseRichTextBox.IsDocumentEnabled = true;
        }
        /// <summary>
        /// Event triggered when a key is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPeriod)
            {
                e.Handled = true;
                DetectVerses();
            }
            else
            {
                e.Handled = false;
            }
        }
        /// <summary>
        /// Event triggered when the save command is executed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        private void SaveCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Save();
        }
        /// <summary>
        /// Event triggered to check if the save command can be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsLoaded && !this.IsEmpty();
        }
        /// <summary>
        /// Event triggered when the open command is executed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Event triggered to check if the open command can be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        /// <summary>
        /// Event triggered when the print command is executed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        private void PrintCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("printing...");
        }
        /// <summary>
        /// Event triggered to check if the print command can be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsLoaded && !this.IsEmpty();
        }
        /// <summary>
        /// Event triggered when the find command is executed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        private void FindCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("finding...");
        }
        /// <summary>
        /// Event triggered to check if the find command can be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsLoaded && !this.IsEmpty();
        }
        /// <summary>
        /// Event triggered when the paste plain command is executed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        private void PastePlainCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //this.documentManager.Insert(Clipboard.GetText());
        }
        /// <summary>
        /// Event triggered to check if the paste plain command can be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PastePlainCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
        }
        /// <summary>
        /// Event triggered if the font size combobox selection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RTBFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (double item in e.AddedItems)
            {
                // apply the font size selected
                this.BaseRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, item);
            }
        }
        /// <summary>
        /// Event triggered if the font family combobox selection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RTBFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (string item in e.AddedItems)
            {
                this.BaseRichTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(item));
            }
        }
        /// <summary>
        /// Event triggered when the richtextbox receives focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseRichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.BaseMetadataPanel.Visibility == Visibility.Visible)
            {
                this.ToggleMetadataPanelOpening(MetadataPanelToggle.Close);
            }
        }
        /// <summary>
        /// Event triggered when the richtextbox content is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //foreach (var arg in e.Changes)
            //{
            //    if (arg.AddedLength > 0)
            //    {
            //        //string rtbText = documentManager.GetRichText(DataFormats.Text);
            //        rtbText.ToString();
            //    }
            //}
        }
        /// <summary>
        /// Event triggered when the expand metadata panel button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Enum holding values for the different states of the metadata panel.
        /// </summary>
        private enum MetadataPanelToggle
        {
            Open, Close
        }
    }
}
