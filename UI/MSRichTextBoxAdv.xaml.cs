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
            BaseRichTextBox.PreviewMouseMove += this.BaseRichTextBox_PreviewMouseMove;
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
    }
}
