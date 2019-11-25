using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DiffPlex;
using DiffPlex.Model;
using MySermonsWPF.Data;
using MySermonsWPF.Data.Bible;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSRichTextBox.xaml
    /// </summary>
    public partial class MSRichTextBox : UserControl
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
        private string lastStringComposition = string.Empty;
        private readonly Differ differ;

        /// <summary>
        /// Constructor accepting a parameter of type sermon.
        /// </summary>
        /// <param name="sermon">The sermon object to be manipulated.</param>
        public MSRichTextBox(Sermon sermon)
        {
            this.InitializeComponent();
            this.sermon = sermon;
            this.xmlBible = new XmlBible();
            this.combinedRegex = new Regex(regexBCVrBCV + "|" + regexBCrBC + "|" + regexBCVrCV + "|" + regexBCrC + "|" + regexBCVrV + "|" + regexBCV + "|" + regexBC, RegexOptions.Compiled);
            this.differ = new Differ();
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
            if (this.sermon != null)
            {
                this.SetUpEditor();
            }
            BaseRichTextBox.KeyUp += this.BaseRichTextBox_KeyUp;
            BaseRichTextBox.FontFamily = new FontFamily("Times New Roman");
        }

        private void DetectVerses()
        {
            string currStringComposition = this.GetRtbText(DataFormats.Text);
            DiffResult diffResults = differ.CreateCharacterDiffs(lastStringComposition, currStringComposition, true);
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
                    List<TextRange> ranges = null;
                    BaseRichTextBox.Dispatcher.Invoke(() =>
                    {
                        TextRange haystack = new TextRange(BaseRichTextBox.Document.ContentStart, BaseRichTextBox.Document.ContentEnd);
                        ranges = FindTextInRange(haystack, match.Value);
                    });
                    foreach (TextRange wordRange in ranges)
                    {
                        this.BaseRichTextBox.Dispatcher.Invoke(() =>
                        {
                            Hyperlink hyperlink = new Hyperlink(wordRange.Start, wordRange.End);
                        });
                    }
                }
            }
        }
        private void BaseRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPeriod)
            {
                e.Handled = true;

                Thread thread = new Thread(new ThreadStart(DetectVerses))
                {
                    IsBackground = true
                };
                thread.Start();
            }
            else
            {
                e.Handled = false;
            }
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

        private void SaveCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Save();
        }

        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsLoaded && !this.IsEmpty();
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
            e.CanExecute = this.IsLoaded && !this.IsEmpty();
        }

        private void FindCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("finding...");
        }

        private void FindCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsLoaded && !this.IsEmpty();
        }

        private void PastePlainCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //this.documentManager.Insert(Clipboard.GetText());
        }

        private void PastePlainCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
        }

        private void RTBFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (double item in e.AddedItems)
            {
                // apply the font size selected
                this.BaseRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, item);
            }
        }

        private void RTBFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (string item in e.AddedItems)
            {
                this.BaseRichTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(item));
            }
        }

        private void BaseRichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.BaseMetadataPanel.Visibility == Visibility.Visible)
            {
                this.ToggleMetadataPanelOpening(MetadataPanelToggle.Close);
            }
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
            StringBuilder builder = new StringBuilder();
            foreach (var theme in this.sermon.Themes.Where(theme => theme.Name != "THEME_NOT_SET").Select(theme => theme))
            {
                builder = builder.Append(theme.Name).Append(", ");
            }
            string themes = builder.ToString().TrimEnd(',', ' ');
            builder.Clear();
            foreach (var speaker in this.sermon.Speakers.Where(speaker => speaker.Name != "SPEAKER_NOT_SET").Select(speaker => speaker))
            {
                builder = builder.Append(speaker.Name).Append(", ");
            }

            string speakers = builder.ToString().TrimEnd(',', ' ');
            this.BaseMetadataPanel.Populate(this.sermon.Title, speakers, this.sermon.KeyVerse, this.sermon.Location.Name, themes, this.sermon.OtherMetaData);
            SetRtbText(this.sermon.Content == "CONTENT_NOT_SET" ? string.Empty : this.sermon.Content, DataFormats.Rtf);

        }

        private void Save()
        {
            if (this.BaseMetadataPanel.Verify())
            {
                char themeDelimiter = ',';
                (string title, string speakers, string keytext, string location, string themes, string otherinfo) metadata = this.BaseMetadataPanel.GetMetadata();

                Location location = string.IsNullOrEmpty(metadata.location) ? null : new Location(metadata.location, StringType.Name);
                List<Theme> themes = string.IsNullOrEmpty(metadata.themes) ? null : Theme.ExtractFromDelimitedString(metadata.themes, themeDelimiter);
                List<Speaker> speakers = string.IsNullOrEmpty(metadata.speakers) ? null : Speaker.ExtractFromDelimitedString(metadata.speakers, themeDelimiter);
                string content = GetRtbText(DataFormats.Rtf);
                string title = string.IsNullOrEmpty(metadata.title) ? null : metadata.title;
                string keyText = string.IsNullOrEmpty(metadata.keytext) ? null : metadata.keytext;
                string otherMetadata = string.IsNullOrEmpty(metadata.otherinfo) ? null : metadata.otherinfo;

                if (this.sermon == null)
                {
                    // the sermon does not exist; create
                    this.sermon = new Sermon(title, location, themes, speakers, keyText, otherMetadata, content);
                    MessageBox.Show("Creation success: " + this.sermon.Create());
                }
                else
                {
                    // the sermon exists; update
                    var id = this.sermon.ID;
                    var guid = this.sermon.GUID;
                    var dateCreated = this.sermon.DateCreated.Ticks;
                    var lastAccessed = DateTime.Now.Ticks;
                    this.sermon = new Sermon(id, guid, title, location.ID, location.Name, location, themes, speakers, dateCreated, lastAccessed, keyText, otherMetadata, content);
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
        /// Convert the RichTextBox text into rich text in a specified format (currently rtf).
        /// </summary>
        /// <returns>A string representation of the rich text.</returns>
        public string GetRtbText(string dataFormat)
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
        public void SetRtbText(string richText, string dataFormat)
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
            return true;
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

        List<TextRange> FindTextInRange(TextRange range, string needle)
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
    }
}
