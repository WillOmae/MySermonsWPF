namespace MySermonsWPF.Data
{
    /// <summary>
    /// enum for filters to be used while sorting sermons.
    /// </summary>
    public enum SermonFilter
    {
        /// <summary>
        /// Sort by date.
        /// </summary>
        Date,
        /// <summary>
        /// Sort by location.
        /// </summary>
        Location,
        /// <summary>
        /// Sort by theme.
        /// </summary>
        Theme,
        /// <summary>
        /// Sort by speaker.
        /// </summary>
        Speaker,
        /// <summary>
        /// Sort by title (alphabetically).
        /// </summary>
        Title
    }
}