using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MySermonsWPF.Data.Bible
{
    public class XmlBible
    {
        /// <summary>
        /// Array of BibleBooks.
        /// </summary>
        public BibleBook[] Books { get; }
        /// <summary>
        /// The working XmlDocument.
        /// </summary>
        private XmlDocument Bible { get; }
        /// <summary>
        /// KJV Bible node.
        /// </summary>
        private XmlNode KJVBibleNode => Bible.DocumentElement.ChildNodes[1];
        /// <summary>
        /// Book names node.
        /// </summary>
        private XmlNode BibleBookNames => Bible.DocumentElement.ChildNodes[0];
        /// <summary>
        /// Default constructor.
        /// </summary>
        public XmlBible()
        {
            Bible = new XmlDocument();
            Bible.Load("BibleXml.xml");
            Books = ReadBooks();
        }
        public List<BibleVerse> Parse(string toParse)
        {
            string bcvStart, bcvEnd;
            if (VerifyRange(bcvStart, bcvEnd))
            {
                return (GetVerses(bcvStart, bcvEnd));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Builds an array of BibleBook's.
        /// </summary>
        /// <returns>BibleBook array.</returns>
        private BibleBook[] ReadBooks()
        {
            BibleBook[] books = new BibleBook[66];
            var booknames = (from XmlElement book in BibleBookNames.ChildNodes
                             select new string[3] { book.Attributes["c"].Value, book.Attributes["abbr"].Value, book.Attributes["short"].Value }).ToArray();
            for (int i = 0; i < KJVBibleNode.ChildNodes.Count; i++)
            {
                BibleChapter[] chapters = new BibleChapter[KJVBibleNode.ChildNodes[i].ChildNodes.Count];
                for (int j = 0; j < KJVBibleNode.ChildNodes[i].ChildNodes.Count; j++)
                {
                    chapters[j] = new BibleChapter(int.Parse(KJVBibleNode.ChildNodes[i].ChildNodes[j].Attributes["ID"].Value),
                                                   KJVBibleNode.ChildNodes[i].ChildNodes[j].ChildNodes.Count);
                }
                books[i] = new BibleBook(booknames[i][0], booknames[i][1], booknames[i][2], chapters);
            }
            return books;
        }
        /// <summary>
        /// Checks a range of bcv's for validity.
        /// </summary>
        /// <param name="bcvStart">Starting bcv.</param>
        /// <param name="bcvEnd">Ending bcv.</param>
        /// <returns>Is range valid?</returns>
        private bool VerifyRange(string bcvStart, string bcvEnd)
        {
            var partsStart = bcvStart.Split('.');
            var partsEnd = bcvEnd.Split('.');
            var iBookStart = FindBookIndex(partsStart[0]);
            var iBookEnd = FindBookIndex(partsEnd[0]);

            if (iBookStart != -1
                && iBookEnd != -1
                && iBookStart <= iBookEnd
                && int.TryParse(partsStart[1], out int iChapStart)
                && int.TryParse(partsEnd[1], out int iChapEnd)
                && iChapStart > 0
                && iChapEnd > 0
                && iChapStart <= this.Books[iBookStart].Chapters.Length
                && iChapEnd <= this.Books[iBookEnd].Chapters.Length
                && int.TryParse(partsStart[2], out int iVerseStart)
                && int.TryParse(partsEnd[2], out int iVerseEnd)
                && iVerseStart > 0
                && iVerseEnd > 0
                && iVerseStart <= this.Books[iBookStart].Chapters[iChapStart - 1].VerseCount
                && iVerseEnd <= this.Books[iBookEnd].Chapters[iChapEnd - 1].VerseCount)
                return true;
            else return false;
        }
        /// <summary>
        /// Returns the index of a book from the BibleBook array.
        /// </summary>
        /// <param name="book">Book to find.</param>
        /// <returns>Book index. -1 if not found.</returns>
        private int FindBookIndex(string book)
        {
            for (int i = 0; i < Books.Length; i++)
            {
                if (Books[i].NameAbbr.Equals(book))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Builds BibleVerses from a range of bcv's.
        /// </summary>
        /// <param name="bcvStart">Range start.</param>
        /// <param name="bcvEnd">Range end.</param>
        /// <returns>List of BibleVerses.</returns>
        private List<BibleVerse> GetVerses(string bcvStart, string bcvEnd)
        {
            var parts = bcvStart.Split('.');
            bool started = false;

            List<BibleVerse> bibleVerses = new List<BibleVerse>();

            for (int i = 0; i < KJVBibleNode.ChildNodes.Count; i++)
            {
                var bookNode = KJVBibleNode.ChildNodes[i];
                if (bookNode.Attributes["NAME"].Value.Equals(parts[0]) || started)
                {
                    for (int j = 0; j < bookNode.ChildNodes.Count; j++)
                    {
                        var chapterNode = bookNode.ChildNodes[j];
                        if (chapterNode.Attributes["ID"].Value.Equals(parts[1]) || started)
                        {
                            for (int k = 0; k < chapterNode.ChildNodes.Count; k++)
                            {
                                var verseNode = chapterNode.ChildNodes[k];
                                var bcv = verseNode.Attributes["BCV"].Value;
                                if (bcv.Equals(bcvStart) || started)
                                {
                                    started = true;
                                    bibleVerses.Add(new BibleVerse(bcv, this.Books[i].NameShort + " " + (j + 1) + ":" + (k + 1), verseNode.InnerText));
                                    if (bcv.Equals(bcvEnd))
                                    {
                                        started = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return bibleVerses;
        }
    }
}
