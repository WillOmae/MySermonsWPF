namespace MySermonsWPF.Data.Bible
{
    /// <summary>
    /// Class defining a Bible Chapter.
    /// </summary>
    public class BibleChapter
    {
        /// <summary>
        /// Chapter identifier.
        /// </summary>
        public int ChapterId { get; }
        /// <summary>
        /// Number of verses in the chapter.
        /// </summary>
        public int VerseCount { get; }
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="chapter">Chapter identifier.</param>
        /// <param name="verseCount">Number of verses in chapter.</param>
        public BibleChapter(int chapter, int verseCount)
        {
            ChapterId = chapter;
            VerseCount = verseCount;
        }
    }
}
