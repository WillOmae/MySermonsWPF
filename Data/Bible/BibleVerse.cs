namespace MySermonsWPF.Data.Bible
{
    /// <summary>
    /// Class defining a Bible verse.
    /// </summary>
    public class BibleVerse
    {
        /// <summary>
        /// Shorthand book-chapter-verse reference.
        /// </summary>
        public string BCV { get; }
        /// <summary>
        /// Human-readable verse reference.
        /// </summary>
        public string Reference { get; }
        /// <summary>
        /// Verse content.
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="bcv">Shorthand book-chapter-verse reference.</param>
        /// <param name="reference">Human-readable verse reference.</param>
        /// <param name="content">Verse content.</param>
        public BibleVerse(string bcv, string reference, string content)
        {
            BCV = bcv;
            Reference = reference;
            Content = content;
        }
        /// <summary>
        /// Return a string representation of the object.
        /// </summary>
        /// <returns>The string representation of the object.</returns>
        public override string ToString()
        {
            return Reference;
        }
    }
}
