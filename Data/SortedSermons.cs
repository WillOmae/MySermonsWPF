using System.Collections.Generic;

namespace MySermonsWPF.Data
{
    /// <summary>
    /// Class used while sorting sermons. It defines the parent (based on the filter) and its children (that match the filter).
    /// </summary>
    public class SortedSermons
    {
        /// <summary>
        /// The common value of a specified attribute of sermons based on the filter.
        /// </summary>
        public string Parent { get; set; }
        /// <summary>
        /// A list of sermons that share a parent based on the filter.
        /// </summary>
        public List<Sermon> Children { get; set; }
    }
}
