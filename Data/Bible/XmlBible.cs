using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MySermonsWPF.Data.Bible
{
    public class XmlBible
    {
        private const char SEPARATOR_BLOCK = ';';
        private const char SEPARATOR_INBLOCK = ',';
        private const char SEPARATOR_CV = ':';
        private const char SEPARATOR_RANGE = '-';
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
            string bcvEnd, bcvStart;
            var bcvs = ParseString1(toParse);
            List<BibleVerse> bibleVerses = new List<BibleVerse>(bcvs.Count);
            foreach (var bcv in bcvs)
            {
                var splits = bcv.Split(new char[] { '-' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length == 1)
                {
                    bcvStart = bcvEnd = splits[0];
                }
                else
                {
                    bcvStart = splits[0];
                    bcvEnd = splits[1];
                }
                if (VerifyRange(bcvStart, bcvEnd))
                {
                    bibleVerses.AddRange(GetVerses(bcvStart, bcvEnd));
                }
            }
            return bibleVerses;
        }
        private List<string> ParseString1(string toParse)
        {
            List<string> bcvs = new List<string>();

            //ensure uniformity in parsing all strings: block separator must exist in all strings
            toParse = toParse.EndsWith(SEPARATOR_BLOCK.ToString()) ? toParse : toParse + SEPARATOR_BLOCK;
            var splits = toParse.Split(new char[] { SEPARATOR_BLOCK }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var split in splits)
            {
                bcvs.AddRange(this.ParseString2(split));
            }
            return bcvs;
        }
        private List<string> ParseString2(string toParse)
        {
            List<string> bcvs = new List<string>();
            string currentBook = null, currentChapter = null, currentVerse = null;
            bool addedVerse = false;

            //ensure uniformity in parsing all strings: inblock separator must exist in all strings
            toParse = toParse.EndsWith(SEPARATOR_INBLOCK.ToString()) ? toParse : toParse + SEPARATOR_INBLOCK;
            var splits = toParse.Split(new char[] { SEPARATOR_INBLOCK }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var split in splits)
            {
                string toPass = currentBook == null ? split : ParseString3(currentBook, currentChapter, currentVerse, split, addedVerse);
                var bcv = ParseString4(ref currentBook, ref currentChapter, ref currentVerse, toPass, addedVerse, out addedVerse);
                var current = BcvExtractor(bcv);
                currentBook = current[0];
                currentChapter = current[1];
                currentVerse = current[2];
                bcvs.Add(bcv);
            }
            return bcvs;
        }
        private string ParseString3(string currentBook, string currentChapter, string currentVerse, string stringToParse, bool addedVerse)
        {
            string newBook, returnString;

            newBook = currentBook;
            if (stringToParse.Contains(SEPARATOR_CV.ToString()))//only the book is shared; it contains its own chapter and verse
            {
                var splits = stringToParse.Split(SEPARATOR_CV);
                returnString = newBook + splits[0] + SEPARATOR_CV + splits[1];
            }
            else
            {
                if (currentVerse == null)//only chapter is recorded, so the string represents a chapter as well
                {
                    returnString = newBook + stringToParse;
                }
                else
                {
                    if (addedVerse)//only the book is shared e.g. PSA140,141
                    {
                        returnString = newBook + stringToParse;
                    }
                    else//both book and chapter are shared e.g. HEB11:1,6
                    {
                        returnString = newBook + currentChapter + SEPARATOR_CV + stringToParse;
                    }
                }
            }
            return returnString;
        }
        private string ParseString4(ref string currentBook, ref string currentChapter, ref string currentVerse, string stringToParse, bool addedStartVerse, out bool addedVerse)
        {
            string startBook = null, startChapter = null, startVerse = null, startBcv = null;
            string endBook = null, endChapter = null, endVerse = null, endBcv = null;
            bool foundChapter = currentChapter == null ? false : stringToParse.Contains(SEPARATOR_CV) ? false : !addedStartVerse;
            bool foundCVSeparator = false;

            addedVerse = false;
            for (int i = 0; i < stringToParse.Length; i++)
            {
                char c = stringToParse[i];
                if (char.IsDigit(c) && i == 0 && stringToParse.Length > 1 && char.IsLetter(stringToParse[i + 1]))
                {
                    startBook += c;
                    foundChapter = false;
                }
                else if (char.IsLetter(c))
                {
                    startBook += c;
                    foundChapter = false;
                }
                else if (char.IsDigit(c) && !foundChapter)
                {
                    startChapter += c;
                }
                else if (c == SEPARATOR_CV)
                {
                    foundChapter = true;
                    foundCVSeparator = true;
                }
                else if (char.IsDigit(c))
                {
                    startVerse += c;
                }
                else if (c == SEPARATOR_RANGE)
                {
                    if (foundCVSeparator)
                    {
                        var end = ParseString4(ref startBook, ref startChapter, ref startVerse, stringToParse.Remove(0, i + 1), addedStartVerse, out addedVerse);
                        if (end.Contains('-'))
                        {
                            end = end.Split(new char[] { '-' }, System.StringSplitOptions.RemoveEmptyEntries)[1];
                        }
                        var splits = BcvExtractor(end);
                        endBook = splits[0];
                        endChapter = splits[1];
                        endVerse = splits[2];
                    }
                    else //sample Heb13-Jas1 or Heb1-2
                    {
                        startVerse = "1";
                        addedStartVerse = true;
                        //addedVerse = true;
                        string pass = stringToParse.Remove(0, i + 1);
                        string end = ParseString4(ref startBook, ref startChapter, ref startVerse, pass, addedStartVerse, out addedVerse);
                        if (end.Contains('-'))
                        {
                            end = end.Split(new char[] { '-' }, System.StringSplitOptions.RemoveEmptyEntries)[1];
                        }
                        var splits = BcvExtractor(end);
                        endBook = splits[0];
                        endChapter = splits[1];
                        endVerse = splits[2];
                    }
                    break;
                }
            }

            if (startBook == null)
            {
                startBook = currentBook;
                if (startChapter == null)
                {
                    startChapter = currentChapter;
                }
                if (startVerse == null)
                {
                    startVerse = currentVerse == null ? "1" : VerseCount(startBook, startChapter).ToString();
                }
                startBcv = BcvBuilder(startBook, startChapter, startVerse);
            }
            else
            {
                //No verse stated, no range given e.g. ~james1~
                if (startVerse == null && endBook == null)
                {
                    startVerse = "1";
                    endBook = startBook;
                    endChapter = startChapter;
                    endVerse = VerseCount(endBook, endChapter).ToString();
                    addedVerse = true;//***********NOTE
                }
                else
                {
                    addedVerse = false;//***********NOTE
                }
                startBcv = BcvBuilder(startBook, startChapter, startVerse);
            }
            if (endBook == null)
            {
                return startBcv;
            }
            else
            {
                endBcv = BcvBuilder(endBook, endChapter, endVerse);
                return startBcv + "-" + endBcv;
            }
        }
        private string BcvBuilder(string book, string chapter, string verse)
        {
            return book.ToUpper() + "." + chapter + "." + verse;
        }
        private string[] BcvExtractor(string bcv)
        {
            return bcv.Split('.');
        }
        private int VerseCount(string book, string chapter)
        {
            for (int i = 0; i < Books.Length; i++)
            {
                if (Books[i].NameAbbr.ToLower().Equals(book.ToLower()))
                {
                    return Books[i].Chapters[(int.Parse(chapter)) - 1].VerseCount;
                }
            }
            return 1;
        }
        /// <summary>
        /// Builds an array of BibleBook's.
        /// </summary>
        /// <returns>BibleBook array.</returns>
        private BibleBook[] ReadBooks()
        {
            BibleBook[] books = new BibleBook[66];
            var booknames = (from XmlElement book in BibleBookNames.ChildNodes
                             select new string[2] { book.Attributes["a"].Value, book.Attributes["s"].Value }).ToArray();
            for (int i = 0; i < KJVBibleNode.ChildNodes.Count; i++)
            {
                BibleChapter[] chapters = new BibleChapter[KJVBibleNode.ChildNodes[i].ChildNodes.Count];
                for (int j = 0; j < KJVBibleNode.ChildNodes[i].ChildNodes.Count; j++)
                {
                    chapters[j] = new BibleChapter(j + 1, KJVBibleNode.ChildNodes[i].ChildNodes[j].ChildNodes.Count);
                }
                books[i] = new BibleBook(booknames[i][0], booknames[i][1], chapters);
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
            if (bcvStart == null || bcvEnd == null)
                return false;
            var partsStart = BcvExtractor(bcvStart);
            var partsEnd = BcvExtractor(bcvEnd);
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
                && iVerseEnd <= this.Books[iBookEnd].Chapters[iChapEnd - 1].VerseCount
                && CompareStartAndFinish(iBookStart, iChapStart, iVerseStart, iBookEnd, iChapEnd, iVerseEnd))
                return true;
            else return false;
        }
        private bool CompareStartAndFinish(int startBook, int startChapter, int startVerse, int endBook, int endChapter, int endVerse)
        {
            return startBook < endBook || (startBook == endBook && (startChapter < endChapter || (startChapter == endChapter && startVerse <= endVerse)));
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
                if (Books[i].NameAbbr.ToLower().Equals(book.ToLower()))
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
            var parts = BcvExtractor(bcvStart);
            bool started = false;

            List<BibleVerse> bibleVerses = new List<BibleVerse>();

            for (int i = 0; i < KJVBibleNode.ChildNodes.Count; i++)
            {
                var bookNode = KJVBibleNode.ChildNodes[i];
                var bookIndex = FindBookIndex(parts[0]);
                if (bookIndex == i || started)
                {
                    for (int j = 0; j < bookNode.ChildNodes.Count; j++)
                    {
                        var chapterNode = bookNode.ChildNodes[j];
                        var chapterIndex = int.Parse(parts[1]) - 1;
                        if (chapterIndex == j || started)
                        {
                            for (int k = 0; k < chapterNode.ChildNodes.Count; k++)
                            {
                                var verseNode = chapterNode.ChildNodes[k];
                                var verseIndex = int.Parse(parts[2]) - 1;
                                if (verseIndex == k || started)
                                {
                                    started = true;
                                    var bcv = Books[i].NameAbbr.ToUpper() + "." + (j + 1) + "." + (k + 1);
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