﻿namespace MySermonsWPF.Data.Bible
{
    public class BibleBook
    {
        /// <summary>
        /// Abbreviated name of a Bible book.
        /// </summary>
        public string NameAbbr { get; }
        /// <summary>
        /// Short name of a Bible book.
        /// </summary>
        public string NameShort { get; }
        /// <summary>
        /// Long name of a Bible book.
        /// </summary>
        public string NameLong { get; }
        /// <summary>
        /// All chapters contained in the Bible book.
        /// </summary>
        public readonly BibleChapter[] Chapters;
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="abbrName">Abbreviated name of a Bible book.</param>
        /// <param name="shortName">Short name of a Bible book.</param>
        /// <param name="longName">Long name of a Bible book.param>
        /// <param name="chapters">All chapters contained in the Bible book.</param>
        public BibleBook(string abbrName, string shortName, string longName, BibleChapter[] chapters)
        {
            NameAbbr = abbrName;
            NameShort = shortName;
            NameLong = longName;
            Chapters = chapters;
        }
        /// <summary>
        /// String representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Book: " + NameShort + ". Chapters: " + Chapters.Length;
        }
    }
}
