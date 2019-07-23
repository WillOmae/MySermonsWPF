namespace MySermonsWPF.Data.Bible
{
    /// <summary>
    /// Class defining a Bible chapter.
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
        /// <summary>
        /// Return a string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Chapter: " + ChapterId + ". Verses: " + VerseCount + ".";
        }
    }
}
