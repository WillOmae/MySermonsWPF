﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MySermonsWPF.Data
{
    /// <summary>
    /// Class describing a sermon/Bible study entry
    /// </summary>
    public class Sermon
    {
        /// <summary>
        /// The autoincremented integer ID.
        /// </summary>
        public long ID
        {
            get
            {
                return this.id;
            }
        }

        /// <summary>
        /// The uniquely generated Guid string.
        /// </summary>
        public string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(this.guid))
                {
                    this.guid = Guid.NewGuid().ToString();
                }
                return this.guid;
            }
        }
        /// <summary>
        /// The sermon title.
        /// </summary>
        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(this.title))
                {
                    this.title = "TITLE_NOT_SET";
                }
                return this.title;
            }
        }
        /// <summary>
        /// The sermon location.
        /// </summary>
        public Location Location
        {
            get
            {
                if (this.location == null)
                {
                    if (this.locationId != -1)
                        this.location = new Location(this.locationId);
                    else if (!string.IsNullOrEmpty(this.locationName))
                        this.location = new Location(this.locationName, StringType.Name);

                    this.locationId = this.location.ID;
                    this.locationName = this.location.Name;
                }
                return this.location;
            }
        }
        /// <summary>
        /// Date the sermon was created.
        /// </summary>
        public DateTime DateCreated
        {
            get
            {
                if (this.dateCreated == -1)
                    this.dateCreated = DateTime.Now.Ticks;
                return new DateTime(this.dateCreated);
            }
        }
        /// <summary>
        /// Date the sermon was last accessed.
        /// </summary>
        public DateTime DateLastAccessed
        {
            get
            {
                if (this.lastAccessed == -1)
                    this.lastAccessed = DateTime.Now.Ticks;
                return new DateTime(this.lastAccessed);
            }
            set
            {
                this.lastAccessed = value.Ticks;
            }
        }
        /// <summary>
        /// Key verse of the sermon.
        /// </summary>
        public string KeyVerse
        {
            get
            {
                if (string.IsNullOrEmpty(this.keyVerse))
                {
                    this.keyVerse = "KEY_VERSE_NOT_SET";
                }
                return this.keyVerse;
            }
        }
        /// <summary>
        /// Other metadata to describe the sermon.
        /// </summary>
        public string OtherMetaData
        {
            get
            {
                if (string.IsNullOrEmpty(this.otherMetadata))
                {
                    this.otherMetadata = "OTHER_METADATA_NOT_SET";
                }
                return this.otherMetadata;
            }
        }
        /// <summary>
        /// The rich text content string.
        /// </summary>
        public string Content
        {
            get
            {
                if (string.IsNullOrEmpty(this.content))
                {
                    this.content = "CONTENT_NOT_SET";
                }
                return this.content;
            }
        }
        /// <summary>
        /// The themes.
        /// </summary>
        public List<Theme> Themes
        {
            get
            {
                if (this.themes == null)
                {
                    this.themes = Theme.GetSermonThemes(this.ID);
                }
                return this.themes;
            }
            set
            {
                this.themes = value;
            }
        }
        /// <summary>
        /// The speakers.
        /// </summary>
        public List<Speaker> Speakers
        {
            get
            {
                if (this.speakers == null)
                {
                    this.speakers = Speaker.GetSermonSpeakers(this.ID);
                }
                return this.speakers;
            }
            set
            {
                this.speakers = value;
            }
        }

        /// <summary>
        /// The autoincremented integer ID. -1 means the sermon has not been created in the database.
        /// </summary>
        private long id = -1;
        /// <summary>
        /// The uniquely generated Guid string. null means the Guid has not been set.
        /// </summary>
        private string guid = null;
        /// <summary>
        /// The title of the sermon. null means it has not been set.
        /// </summary>
        private string title = null;
        /// <summary>
        /// The location ID. -1 means it has not been set.
        /// </summary>
        private long locationId = -1;
        /// <summary>
        /// The location name. null means it has not been set.
        /// </summary>
        private string locationName = null;
        /// <summary>
        /// The location object. null means it has not been set.
        /// </summary>
        private Location location = null;
        /// <summary>
        /// Ticks since the epoch. -1 means it has not been set.
        /// </summary>
        private long dateCreated = -1;
        /// <summary>
        /// Ticks since the epoch. -1 means it has not been set.
        /// </summary>
        private long lastAccessed = -1;
        /// <summary>
        /// The key verse. null means it has not been set.
        /// </summary>
        private string keyVerse = null;
        /// <summary>
        /// Other metadata. null means it has not been set.
        /// </summary>
        private string otherMetadata = null;
        /// <summary>
        /// The content. null means it has not been set.
        /// </summary>
        private string content = null;
        /// <summary>
        /// The themes. null means it has not been set.
        /// </summary>
        private List<Theme> themes = null;
        /// <summary>
        /// The speakers. null means it has not been set.
        /// </summary>
        private List<Speaker> speakers = null;

        /// <summary>
        /// The default select statement.
        /// </summary>
        private const string SELECT_STATEMENT = "SELECT " +
            "sermons.id AS sermonId, " +
            "sermons.guid AS sermonGuid, " +
            "sermons.title AS sermonTitle, " +
            "locations.id AS locationId, " +
            "locations.Name AS locationName, " +
            "sermons.date_created AS sermonDateCreated, " +
            "sermons.last_access_date AS sermonLastAccessed, " +
            "sermons.key_verse AS sermonKeyVerse, " +
            "sermons.other_metadata AS sermonOtherMetadata, " +
            "sermons.content AS sermonContent " +
            "FROM sermons " +
            "INNER JOIN locations " +
            "ON locations.id=sermons.location";

        /// <summary>
        /// Internal constructor without parameters.
        /// </summary>
        private Sermon()
        {

        }

        /// <summary>
        /// Build sermon from ID.
        /// </summary>
        /// <param name="id">The ID of the sermon to search.</param>
        public Sermon(long id)
        {
            Sermon sermon = Read(id);
            this.CommonInit(sermon.ID, sermon.GUID, sermon.Title, sermon.Location.ID, sermon.Location.Name, sermon.Location, sermon.Themes, sermon.Speakers, sermon.DateCreated.Ticks, sermon.DateLastAccessed.Ticks, sermon.KeyVerse, sermon.OtherMetaData, sermon.Content);
        }

        /// <summary>
        /// Build sermon from GUID.
        /// </summary>
        /// <param name="guid">The GUID of the sermon to search.</param>
        public Sermon(string guid)
        {
            Sermon sermon = Read(guid);
            this.CommonInit(sermon.ID, sermon.GUID, sermon.Title, sermon.Location.ID, sermon.Location.Name, sermon.Location, sermon.Themes, sermon.Speakers, sermon.DateCreated.Ticks, sermon.DateLastAccessed.Ticks, sermon.KeyVerse, sermon.OtherMetaData, sermon.Content);
        }

        /// <summary>
        /// Build sermon from minimal parameters.
        /// </summary>
        /// <param name="title">The title of the sermon.</param>
        /// <param name="location">The location of the sermon.</param>
        /// <param name="keyVerse">The key verse of the sermon.</param>
        /// <param name="otherMetadata">Other metadata.</param>
        /// <param name="content">The content of the sermon.</param>
        public Sermon(string title, Location location, List<Theme> themes, List<Speaker> speakers, string keyVerse, string otherMetadata, string content)
        {
            this.CommonInit(-1, Guid.NewGuid().ToString(), title, location.ID, location.Name, location, themes, speakers, DateTime.Now.Ticks, DateTime.Now.Ticks, keyVerse, otherMetadata, content);
        }

        /// <summary>
        /// Build sermon from all parameters.
        /// </summary>
        /// <param name="id">The ID of the sermon.</param>
        /// <param name="guid">The GUID of the sermon.</param>
        /// <param name="title">The title of the sermon.</param>
        /// <param name="locationId">The location ID.</param>
        /// <param name="locationName">The location name.</param>
        /// <param name="location">The location of the sermon.</param>
        /// <param name="dateCreated">The date the sermon was created.</param>
        /// <param name="lastAccessed">The date the sermon was last accessed.</param>
        /// <param name="keyVerse">The key verse of the sermon.</param>
        /// <param name="otherMetadata">Other metadata.</param>
        /// <param name="content">The content of the sermon.</param>
        public Sermon(long id, string guid, string title, long locationId, string locationName, Location location, List<Theme> themes, List<Speaker> speakers, long dateCreated, long lastAccessed, string keyVerse, string otherMetadata, string content)
        {
            this.CommonInit(id, guid, title, locationId, locationName, location, themes, speakers, dateCreated, lastAccessed, keyVerse, otherMetadata, content);
        }

        /// <summary>
        /// Called by all constructors to prevent duplication of code.
        /// </summary>
        /// <param name="id">The ID of the sermon.</param>
        /// <param name="guid">The GUID of the sermon.</param>
        /// <param name="title">The title of the sermon.</param>
        /// <param name="locationId">The location ID.</param>
        /// <param name="locationName">The location name.</param>
        /// <param name="location">The location of the sermon.</param>
        /// <param name="dateCreated">The date the sermon was created.</param>
        /// <param name="lastAccessed">The date the sermon was last accessed.</param>
        /// <param name="keyVerse">The key verse of the sermon.</param>
        /// <param name="otherMetadata">Other metadata.</param>
        /// <param name="content">The content of the sermon.</param>
        private void CommonInit(long id, string guid, string title, long locationId, string locationName, Location location, List<Theme> themes, List<Speaker> speakers, long dateCreated, long lastAccessed, string keyVerse, string otherMetadata, string content)
        {
            this.id = id;
            this.guid = guid;
            this.title = title;
            this.locationId = locationId;
            this.locationName = locationName;
            this.location = location;
            this.dateCreated = dateCreated;
            this.lastAccessed = lastAccessed;
            this.keyVerse = keyVerse;
            this.otherMetadata = otherMetadata;
            this.content = content;
            if (themes == null)
            {
                this.themes = new List<Theme>
                {
                    new Theme("none", StringType.Name)
                };
            }
            else
            {
                this.themes = themes;
            }
            if (speakers == null)
            {
                this.speakers = new List<Speaker>
                {
                    new Speaker("none", StringType.Name)
                };
            }
            else
            {
                this.speakers = speakers;
            }
        }

        /// <summary>
        /// Create the sermon in the database.
        /// </summary>
        /// <returns>Creation successful?</returns>
        public bool Create()
        {
            string sql = "INSERT INTO `sermons`(guid,title,location,date_created,last_access_date,key_verse,other_metadata,content) VALUES (@guid,@title,@location,@date_created,@last_access_date,@key_verse,@other_metadata,@content);";
            List<object> parameters = new List<object>() { this.guid, this.Title, this.locationId, this.dateCreated, this.lastAccessed, this.KeyVerse, this.OtherMetaData, this.Content };
            if (Database.Create(sql, parameters, out this.id))
            {
                Theme.SetSermonThemes(this.Themes, this.ID);
                Speaker.SetSermonSpeakers(this.Speakers, this.ID);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Update the sermon in the database.
        /// </summary>
        /// <returns>Update successful?</returns>
        public bool Update()
        {
            string sql = "UPDATE `sermons` SET guid=@guid,title=@title,location=@location,date_created=@date_created,last_access_date=@last_access_date,key_verse=@key_verse,other_metadata=@other_metadata,content=@content WHERE id=@id;";
            List<object> parameters = new List<object>() { this.guid, this.Title, this.locationId, this.dateCreated, this.lastAccessed, this.KeyVerse, this.OtherMetaData, this.Content, this.ID };
            if (Database.Update(sql, parameters))
            {
                Theme.UpdateSermonThemes(this.Themes, this.ID);
                Speaker.UpdateSermonSpeakers(this.Speakers, this.ID);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete the sermon from the database.
        /// </summary>
        /// <returns>Deletion successful?</returns>
        public bool Delete()
        {
            string sql = "DELETE FROM `sermons` WHERE id=@id;";
            if (Database.Delete(sql, this.ID))
            {
                Theme.DeleteSermonThemes(this.ID);
                Speaker.DeleteSermonSpeakers(this.ID);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Select by ID.
        /// </summary>
        /// <param name="ID">The ID to search for.</param>
        /// <returns>Sermon if found; null otherwise.</returns>
        public static Sermon Read(long ID)
        {
            string sql = SELECT_STATEMENT + " WHERE sermons.id=@id;";
            List<Dictionary<string, object>> reader = Database.Read(sql, ID);
            List<Sermon> result = BuildFromReader(reader);
            if (result != null && result.Count > 0)
            {
                result[0].Themes = Theme.GetSermonThemes(result[0].ID);
                result[0].Speakers = Speaker.GetSermonSpeakers(result[0].ID);
                return result[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Select by GUID.
        /// </summary>
        /// <param name="guid">The GUID to search for.</param>
        /// <returns>Sermon if true; null otherwise.</returns>
        public static Sermon Read(string guid)
        {
            string sql = SELECT_STATEMENT + " WHERE sermons.guid=@guid;";
            List<Dictionary<string, object>> reader = Database.Read(sql, guid);
            List<Sermon> result = BuildFromReader(reader);
            if (result != null && result.Count > 0)
            {
                result[0].Themes = Theme.GetSermonThemes(result[0].ID);
                result[0].Speakers = Speaker.GetSermonSpeakers(result[0].ID);
                return result[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Selects all sermons.
        /// </summary>
        /// <returns>List of sermons if they exist; null otherwise.</returns>
        public static List<Sermon> Read()
        {
            List<Dictionary<string, object>> reader = Database.Read(SELECT_STATEMENT);
            return BuildFromReader(reader);
        }

        /// <summary>
        /// Builds a list of sermons from a more generic list returned by the database class.
        /// </summary>
        /// <param name="rows">List of data rows.</param>
        /// <returns>List of sermons if the parameter has data; null otherwise.</returns>
        private static List<Sermon> BuildFromReader(List<Dictionary<string, object>> rows)
        {
            if (rows != null)
            {
                List<Sermon> result = new List<Sermon>();
                foreach (Dictionary<string, object> row in rows)
                {
                    if (row.Count == 10)
                    {
                        Sermon sermon = new Sermon
                        {
                            id = (long)row["sermonId"],
                            guid = (string)row["sermonGuid"],
                            title = (string)row["sermonTitle"],
                            locationId = (long)row["locationId"],
                            locationName = (string)row["locationName"],
                            dateCreated = (long)row["sermonDateCreated"],
                            lastAccessed = (long)row["sermonLastAccessed"],
                            keyVerse = (string)row["sermonKeyVerse"],
                            otherMetadata = (string)row["sermonOtherMetadata"],
                            content = (string)row["sermonContent"]
                        };
                        sermon.Themes = Theme.GetSermonThemes(sermon.ID);
                        sermon.Speakers = Speaker.GetSermonSpeakers(sermon.ID);
                        result.Add(sermon);
                    }
                    else
                    {
                        continue;
                    }
                }
                if (result.Count < 1) return null;
                else return result;
            }
            else return null;
        }

        /// <summary>
        /// Sort sermons based on the filter passed.
        /// </summary>
        /// <param name="filter">The filter to be used.</param>
        /// <param name="sermons">List of sermons to be sorted.</param>
        /// <returns>List of sorted sermons or null.</returns>
        public static List<SortedSermons> Sort(SermonFilter filter, List<Sermon> sermons)
        {
            return sermons == null ? null : SortActuator(filter, sermons);
        }

        /// <summary>
        /// Does the actual sorting.
        /// </summary>
        /// <param name="filter">The filter to be used to sort.</param>
        /// <param name="sermons">The non-null list of sermons to be sorted.</param>
        /// <returns>List of sorted sermons.</returns>
        private static List<SortedSermons> SortActuator(SermonFilter filter, List<Sermon> sermons)
        {
            List<SortedSermons> sortedSermons = new List<SortedSermons>();
            switch (filter)
            {
                case SermonFilter.Date:
                    HashSet<int> years = new HashSet<int>();
                    foreach (Sermon sermon in sermons)
                    {
                        years.Add(sermon.DateCreated.Year);
                    }
                    foreach (int year in years)
                    {
                        AddToSortedList(ref sortedSermons, year.ToString(), from sermon in sermons
                                                                            where sermon.DateCreated.Year == year
                                                                            select sermon);
                    }
                    break;
                case SermonFilter.Location:
                    var locations = Location.Read();
                    if (locations == null) break;
                    foreach (Location location in locations)
                    {
                        AddToSortedList(ref sortedSermons, location.Name, from sermon in sermons
                                                                          where sermon.Location.Name == location.Name
                                                                          select sermon);
                    }
                    break;
                case SermonFilter.Title:
                    HashSet<char> chars = new HashSet<char>() { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
                    foreach (char c in chars)
                    {
                        AddToSortedList(ref sortedSermons, c.ToString(), from sermon in sermons
                                                                         where char.ToLower(sermon.Title[0]) == c
                                                                         select sermon);
                    }
                    break;
                case SermonFilter.Theme:
                    var themes = Theme.Read();
                    if (themes == null) break;
                    foreach (Theme theme in themes)
                    {
                        AddToSortedList(ref sortedSermons,
                                        theme.Name,
                                        sermons.Where(sermon => sermon.Themes != null && sermon.Themes.Find(x => x.Name == theme.Name) != null).Select(sermon => sermon).ToList());
                    }
                    break;
                case SermonFilter.Speaker:
                    var speakers = Speaker.Read();
                    if (speakers == null) break;
                    foreach (Speaker speaker in speakers)
                    {
                        AddToSortedList(ref sortedSermons,
                                        speaker.Name,
                                        sermons.Where(sermon => sermon.Speakers != null && sermon.Speakers.Find(x => x.Name == speaker.Name) != null).Select(sermon => sermon).ToList());
                    }
                    break;
            }
            return sortedSermons;
        }

        /// <summary>
        /// Creates a <see cref="SortedSermons"/> object and adds it to the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list of <see cref="SortedSermons"/> passed by reference.</param>
        /// <param name="parent">The string to be set as the parent.</param>
        /// <param name="children">The collection of <see cref="Sermon"/> objects.</param>
        private static void AddToSortedList(ref List<SortedSermons> list, string parent, IEnumerable<Sermon> children)
        {
            if (children == null || !children.Any()) return;
            SortedSermons sortedSermons = new SortedSermons
            {
                Parent = parent,
                Children = children.ToList()
            };
            list.Add(sortedSermons);
        }

        public bool Equals(Sermon test)
        {
            if (test == null) return false;
            return this.Content == test.Content && this.Title == test.Title && this.DateCreated == test.DateCreated;
        }
    }
}