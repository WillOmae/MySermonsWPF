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
                string toPass = split;
                if (currentBook != null)
                {
                    if (currentBook.Length > 1)
                    {
                        toPass = ParseString3(currentBook, currentChapter, currentVerse, split, addedVerse);
                    }
                }
                var bcv = ParseString4(ref currentBook, ref currentChapter, ref currentVerse, toPass, out addedVerse);
                var current = bcv.Split('.');
                currentBook = current[0];
                currentChapter = current[1];
                currentVerse = current[2];
                bcvs.Add(bcv);
            }
            return bcvs;
        }
        private string ParseString3(string currentBook, string currentChapter, string currentVerse, string stringToParse, bool addedVerse)
        {
            string newBook, newChapter, newVerse, returnString;

            newBook = currentBook;
            if (stringToParse.Contains(SEPARATOR_CV.ToString()))//only the book is shared; it contains its own chapter and verse
            {
                newChapter = stringToParse.Remove(stringToParse.IndexOf(SEPARATOR_CV));
                newVerse = stringToParse.Remove(0, stringToParse.IndexOf(SEPARATOR_CV) + 1);
                returnString = newBook + newChapter + SEPARATOR_CV + newVerse;
            }
            else
            {
                if (currentVerse == null || currentVerse.Length < 1)//only chapter is recorded, so the string represents a chapter as well
                {
                    newChapter = stringToParse;
                    returnString = newBook + newChapter;
                }
                else
                {
                    if (addedVerse)//only the book is shared e.g. PSA140,141
                    {
                        newChapter = stringToParse;
                        returnString = newBook + newChapter;
                    }
                    else//both book and chapter are shared e.g. HEB11:1,6
                    {
                        newChapter = currentChapter;
                        newVerse = stringToParse;
                        returnString = newBook + newChapter + SEPARATOR_CV + newVerse;
                    }
                }
            }
            return returnString;
        }
        private string ParseString4(ref string currentBook, ref string currentChapter, ref string currentVerse, string stringToParse, out bool addedVerse)
        {
            string startBook = null, startChapter = null, startVerse = null, startBcv = null;
            string endBook = null, endChapter = null, endVerse = null, endBcv = null;
            int index = 0;
            bool foundChapter = false, foundCVSeparator = false;

            foreach (char c in stringToParse)
            {
                if (char.IsDigit(c) == true && index == 0)
                {
                    startBook += c;
                }
                else if (char.IsLetter(c) == true)
                {
                    startBook += c;
                }
                else if (char.IsDigit(c) == true && foundChapter == false)
                {
                    startChapter += c;
                }
                else if (char.IsPunctuation(c) == true && c == SEPARATOR_CV)
                {
                    foundChapter = true;
                    foundCVSeparator = true;
                }
                else if (char.IsDigit(c) == true)
                {
                    startVerse += c;
                }
                else if (char.IsPunctuation(c) == true && c == SEPARATOR_RANGE)//get the end BCVstruct
                {//sample ~Heb13-Jas1~
                    if (foundCVSeparator)
                    {
                        ParseString5(stringToParse.Remove(0, stringToParse.IndexOf(c) + 1), startBook, startChapter, startVerse, ref endBook, ref endChapter, ref endVerse);
                    }
                    else
                    {
                        startVerse = "1";
                        string pass = stringToParse.Remove(0, stringToParse.IndexOf(c) + 1);
                        int firstDigitIndex = 0;
                        for (int i = 0; i < pass.Length; i++)
                        {
                            char x = pass[i];
                            if (char.IsDigit(x))
                            {
                                try
                                {
                                    if (!char.IsLetter(pass[i + 1]))
                                    {
                                        firstDigitIndex = pass.IndexOf(x);
                                        break;
                                    }
                                }
                                catch
                                {
                                    firstDigitIndex = pass.IndexOf(x);
                                }
                            }
                        }
                        string book = pass.Remove(firstDigitIndex), chapter = pass.Remove(0, firstDigitIndex);
                        pass = book + chapter + ":" + VerseCount(book, chapter);

                        ParseString5(pass, startBook, startChapter, startVerse, ref endBook, ref endChapter, ref endVerse);
                    }
                    break;
                }
                index++;
            }

            if (startBook != null)
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
                startBcv = startBook.ToUpper() + "." + startChapter + "." + startVerse;//generate bcv
            }
            else
            {
                startBcv = null;
                addedVerse = false;//***********NOTE
            }
            if (endBook != null)
            {
                endBcv = endBook.ToUpper() + "." + endChapter + "." + endVerse;//generate bcv
                return startBcv + "-" + endBcv;
            }
            else
            {
                return startBcv;
            }
        }
        private void ParseString5(string stringToParse, string startBook, string startChapter, string startVerse, ref string endBook, ref string endChapter, ref string endVerse)
        {
            bool chapterFound = false;

            try
            {
                if (char.IsDigit(stringToParse[0]) && char.IsLetter(stringToParse[1]))//e.g 2John1:1-3John1:1
                {
                    endBook += stringToParse[0];
                }
            }
            catch { }
            foreach (char c in stringToParse)
            {
                if (char.IsLetter(c))//e.g Hebrews11:1-Hebrews12:1
                {
                    endBook += c;
                    stringToParse = stringToParse.Remove(0, stringToParse.IndexOf(c) + 1);
                }
            }

            if (endBook == null)//e.g Hebrews11:1-12:1 End.Book should be Hebrews as well
            {
                endBook = startBook;
            }

            foreach (char c in stringToParse)
            {
                if (char.IsPunctuation(c) && c == ':')//e.g Hebrews11:1-12:1
                {
                    chapterFound = true;
                }
            }
            if (chapterFound)//e.g Hebrews11:1-12:1 chapter is 12
            {
                endChapter = stringToParse.Remove(stringToParse.IndexOf(':'));
                stringToParse = stringToParse.Remove(0, stringToParse.IndexOf(':') + 1);
            }
            else//e.g Hebrews11:1-12 or //e.g Hebrews11-12
            {
                if (startVerse == null)//e.g Hebrews11-12
                {
                    foreach (char c in stringToParse)
                    {
                        endChapter += c;
                        stringToParse = stringToParse.Remove(0, stringToParse.IndexOf(c) + 1);
                    }
                }
                else//e.g Hebrews11:1-12 chapter should be 11 as well i.e Hebrews 11:12 for End
                {
                    endChapter = startChapter;
                }
            }
            if (stringToParse != null && stringToParse.Length > 0)
            {
                foreach (char c in stringToParse)
                {
                    if (startVerse == null)
                    {
                        endChapter += c;
                    }
                    else
                    {
                        endVerse += c;
                    }
                }
            }
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
            if (bcvStart == null || bcvEnd == null)
                return false;
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
