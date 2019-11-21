using DiffPlex;
using DiffPlex.Model;
using MySermonsWPF.Data;
using MySermonsWPF.Data.Bible;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        public static readonly List<string> InstalledFonts = Fonts.SystemFontFamilies.Select(ff => ff.FamilyNames.Values.First()).OrderBy(ff => ff).ToList();
        public static readonly List<double> FontSizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        private static readonly SfRichTextBoxAdv sfRichTextBoxAdv = new SfRichTextBoxAdv();
        /// <summary>
        /// Sermon that is to be manipulated internally.
        /// </summary>
        private Sermon sermon = null;
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

        private readonly bool? verseBold = true;
        private readonly Underline? verseUnderline = Underline.Single;
        private readonly Color? verseColor = Color.FromArgb(0xff, 0x05, 0x63, 0xc1);

        private readonly Regex combinedRegex;
        private readonly XmlBible xmlBible;
        private readonly Differ differ;

        private Point lastMousePosition = new Point(-1, -1);
        private string lastStringComposition = string.Empty;

        /// <summary>
        /// Constructor accepting a parameter of type sermon.
        /// </summary>
        /// <param name="sermon">The sermon object to be manipulated.</param>
        public MSRichTextBoxAdv(Sermon sermon)
        {
            this.InitializeComponent();
            this.sermon = sermon;
            this.xmlBible = new XmlBible();
            this.differ = new Differ();
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
            //BaseRichTextBox.PreviewMouseMove += this.BaseRichTextBox_PreviewMouseMove;
            fontFamilyComboBox.SelectedItem = InstalledFonts.Find(ff => ff.Equals("Times New Roman"));
            fontSizeComboBox.SelectedIndex = 2;
        }

        private void BaseRichTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            string current = BaseRichTextBox.Selection.Start.HierarchicalIndex;
            Point mousePoint = Mouse.GetPosition(BaseRichTextBox);
            if (lastMousePosition.Equals(mousePoint))
                return;
            lastMousePosition = mousePoint;
            BaseRichTextBox.CaptureMouse();

            // Raises mouse event to perform selection at mouse point.
            this.BaseRichTextBox.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent
            });
            this.BaseRichTextBox.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseUpEvent
            });
            // Moves the start and end pointer to Word start and end respectively.
            BaseRichTextBox.Selection.Start.MoveToWordStart();
            BaseRichTextBox.Selection.End.MoveToWordEnd();
            // Gets the word start location
            Rect startRect = BaseRichTextBox.Selection.Start.GetRect();
            // Gets the word end location.
            Rect endRect = BaseRichTextBox.Selection.End.GetRect();
            //Calculates the width.
            double width = 0;
            if (endRect.X - startRect.X > 0)
                width = endRect.X - startRect.X;
            // Calculates teh word bounds.
            Rect wordBounds = new Rect(startRect.X, startRect.Y, width, startRect.Height);
            //Check whether teh mouse point within the word bounds.
            if (wordBounds.Contains(mousePoint))
            {
                //Gets the word under the mouse pointer.
                var charFormat = BaseRichTextBox.Selection.CharacterFormat;
                if (charFormat.Bold == verseBold && charFormat.Underline == verseUnderline && charFormat.FontColor == verseColor)
                {
                    Mouse.SetCursor(Cursors.Hand);
                }
                else
                {
                    Mouse.SetCursor(Cursors.IBeam);
                }
            }
            else
            {
                Mouse.SetCursor(Cursors.IBeam);
            }

            // Reset the cursor position to last user editing position.
            TextPosition truePos = BaseRichTextBox.Document.GetTextPosition(current);
            BaseRichTextBox.Selection.Select(truePos, truePos);
            BaseRichTextBox.ReleaseMouseCapture();
            BaseRichTextBox.Focus();
        }

        private void BaseRichTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(BaseRichTextBox);
        }
        private void DetectVerses()
        {
            string currStringComposition = this.GetRTBContents(FormatType.Txt);
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
                    if (list == null && list.Count < 1) continue;
                    string currentStart = string.Empty;
                    TextSearchResults positions = null;
                    BaseRichTextBox.Dispatcher.Invoke(() =>
                    {
                        currentStart = BaseRichTextBox.Selection.Start.HierarchicalIndex;
                        positions = BaseRichTextBox.FindAll(match.Value, FindOptions.CaseSensitiveWholeWord);
                    });
                    if (positions == null) continue;
                    for (int posCount = 0; posCount < positions.Count; posCount++)
                    {
                        TextSearchResult position = positions[posCount];
                        string start = position.Start.HierarchicalIndex;
                        string end = position.End.HierarchicalIndex;
                        this.BaseRichTextBox.Dispatcher.BeginInvoke((FormatAsVerseDelegate)this.FormatAsVerse, start, end, currentStart);
                    }
                }
            }
        }
        delegate void FormatAsVerseDelegate(string start, string end, string cursorPos);
        private void FormatAsVerse(string startPos, string endPos, string cursorPos)
        {
            TextPosition start = BaseRichTextBox.Document.GetTextPosition(startPos);
            TextPosition end = BaseRichTextBox.Document.GetTextPosition(endPos);
            BaseRichTextBox.Selection.Select(start, end);
            BaseRichTextBox.Selection.CharacterFormat.Bold = verseBold;
            BaseRichTextBox.Selection.CharacterFormat.Underline = verseUnderline;
            BaseRichTextBox.Selection.CharacterFormat.FontColor = verseColor;

            TextPosition textPosition = BaseRichTextBox.Document.GetTextPosition(cursorPos);
            BaseRichTextBox.Selection.Select(textPosition, textPosition);
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
            if (this.BaseMetadataPanel.Verify())
            {
                char themeDelimiter = ',';
                (string title, string speakers, string keytext, string location, string themes, string otherinfo) metadata = this.BaseMetadataPanel.GetMetadata();

                List<Theme> themes = string.IsNullOrEmpty(metadata.themes) ? null : Theme.ExtractFromDelimitedString(metadata.themes, themeDelimiter);
                List<Speaker> speakers = string.IsNullOrEmpty(metadata.speakers) ? null : Speaker.ExtractFromDelimitedString(metadata.speakers, themeDelimiter);
                string title = string.IsNullOrEmpty(metadata.title) ? null : metadata.title;
                string keyText = string.IsNullOrEmpty(metadata.keytext) ? null : metadata.keytext;

                string line1 = null;
                if (!string.IsNullOrEmpty(title))
                {
                    line1 = title;
                }
                StringBuilder line2 = new StringBuilder();
                if (speakers != null)
                {
                    line2.Append("by ");
                    foreach (var speaker in speakers.Where(speaker => speaker.Name != "SPEAKER_NOT_SET").Select(speaker => speaker))
                    {
                        line2.Append(speaker.Name).Append(", ");
                    }
                    int len = line2.Length;
                    if (line2[len - 2] == ',' && line2[len - 1] == ' ')
                    {
                        line2.Remove(len - 2, 2);
                    }
                }
                if (!string.IsNullOrEmpty(keyText))
                {
                    line2.Append(" based on ");
                    line2.Append(keyText);
                }
                string date = "18/11/2019";
                string place = "Eldoret";
                StringBuilder line3 = new StringBuilder();
                if (!string.IsNullOrEmpty(place))
                {
                    line3.Append("at ");
                    line3.Append(place);
                }
                if (!string.IsNullOrEmpty(date))
                {
                    line3.Append(" on ");
                    line3.Append(date);
                }
                string topics = "salvation, eschatology, christology";
                StringBuilder line4 = new StringBuilder();
                if (!string.IsNullOrEmpty(topics))
                {
                    line4.Append(topics);
                }

                SpanAdv span1 = new SpanAdv
                {
                    Text = line1
                };
                span1.CharacterFormat.Bold = true;
                span1.CharacterFormat.Italic = true;
                span1.CharacterFormat.FontSize = 20;
                SpanAdv span2 = new SpanAdv
                {
                    Text = line2.ToString()
                };
                span2.CharacterFormat.Italic = true;
                span2.CharacterFormat.FontSize = 18;
                SpanAdv span3 = new SpanAdv
                {
                    Text = line3.ToString()
                };
                span3.CharacterFormat.Italic = true;
                span3.CharacterFormat.FontSize = 18;
                SpanAdv span4 = new SpanAdv
                {
                    Text = line4.ToString()
                };
                span4.CharacterFormat.Italic = true;
                span4.CharacterFormat.FontSize = 18;


                ParagraphAdv para1 = new ParagraphAdv();
                para1.Inlines.Add(span1);
                para1.ParagraphFormat.AfterSpacing = 10;
                para1.ParagraphFormat.TextAlignment = TextAlignment.Center;
                ParagraphAdv para2 = new ParagraphAdv();
                para2.Inlines.Add(span2);
                para2.ParagraphFormat.TextAlignment = TextAlignment.Center;
                ParagraphAdv para3 = new ParagraphAdv();
                para3.Inlines.Add(span3);
                para3.ParagraphFormat.TextAlignment = TextAlignment.Center;
                ParagraphAdv para4 = new ParagraphAdv();
                para4.Inlines.Add(span4);
                para4.ParagraphFormat.TextAlignment = TextAlignment.Center;

                // prevents rtb blackening when assigning documents
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BaseRichTextBox.Save(memoryStream, FormatType.Rtf);
                    sfRichTextBoxAdv.Load(memoryStream, FormatType.Rtf);
                }
                sfRichTextBoxAdv.Document.Sections[0].Blocks.Insert(0, para1);
                sfRichTextBoxAdv.Document.Sections[0].Blocks.Insert(1, para2);
                sfRichTextBoxAdv.Document.Sections[0].Blocks.Insert(2, para3);
                sfRichTextBoxAdv.Document.Sections[0].Blocks.Insert(3, para4);
                sfRichTextBoxAdv.PrintDocument();
            }
            else
            {
                this.ToggleMetadataPanelOpening(MetadataPanelToggle.Open);
                MessageBox.Show("Specify sermon title", "Title not set", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
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
            SetRTBContents(FormatType.Rtf, this.sermon.Content == "CONTENT_NOT_SET" ? string.Empty : this.sermon.Content);
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
                string content = GetRTBContents(FormatType.Rtf);
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

        private void DummyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void DummyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }
}
