using System;
using System.Collections.Generic;
using System.Text;

namespace MySermonsWPF.Data
{
    public class Theme
    {
        /// <summary>
        /// Default theme select statement.
        /// </summary>
        private const string SELECT_STATEMENT = "SELECT * FROM `themes`";
        /// <summary>
        /// Property: Theme ID depends on position in database.
        /// </summary>
        public long ID { get { return this.id; } }
        /// <summary>
        /// Property: Unique GUID identifier for the theme.
        /// </summary>
        public string GUID
        {
            get
            {
                if(string.IsNullOrEmpty(this.guid))
                {
                    this.guid = Guid.NewGuid().ToString();
                }
                return this.guid;
            }
        }
        /// <summary>
        /// Property: Unique name for the theme.
        /// </summary>
        public string Name
        {
            get
            {
                if(string.IsNullOrEmpty(this.name))
                {
                    this.name = "THEME_NOT_SET";
                }
                return this.name;
            }
        }
        /// <summary>
        /// Field: Theme ID depends on position in database.
        /// </summary>
        private long id = -1;
        /// <summary>
        /// Field: Unique GUID identifier for the theme.
        /// </summary>
        private string guid = null;
        /// <summary>
        /// Field: Unique name for the theme.
        /// </summary>
        private string name = null;
        /// <summary>
        /// Theme constructor: used when all properties are known.
        /// </summary>
        /// <param name="id">Theme ID.</param>
        /// <param name="guid">Theme GUID.</param>
        /// <param name="name">Theme name.</param>
        public Theme(long id, string guid, string name)
        {
            this.id = id;
            this.guid = guid;
            this.name = name;
        }
        /// <summary>
        /// Theme constructor: used when either name or GUID is know.
        /// </summary>
        /// <param name="name">Name or GUID.</param>
        /// <param name="stringType">String type.</param>
        public Theme(string name, StringType stringType)
        {
            Theme theme = Read(name, stringType);
            if(theme == null)
            {
                this.guid = Guid.NewGuid().ToString();
                this.name = name;
                this.Create();
            }
            else
            {
                this.id = theme.id;
                this.guid = theme.guid;
                this.name = theme.name;
            }
        }
        /// <summary>
        /// Theme constructor: used when only ID is known.
        /// </summary>
        /// <param name="id"></param>
        public Theme(long id)
        {
            Theme theme = Read(id);
            if(theme == null)
            {
                throw new Exception("The theme with the set id was not found.");
            }
            else
            {
                this.id = theme.id;
                this.guid = theme.guid;
                this.name = theme.name;
            }
        }
        /// <summary>
        /// Creates a theme in the database, setting the ID field as the new index.
        /// </summary>
        /// <returns>Is creation successful?</returns>
        public bool Create()
        {
            string sql = "INSERT INTO `themes`(guid,name) VALUES (@guid,@name);";
            List<object> parameters = new List<object>
            {
                this.GUID,
                this.Name
            };
            return Database.Create(sql, parameters, out this.id);
        }
        /// <summary>
        /// Updates a theme in the database.
        /// </summary>
        /// <returns>Is update successful?</returns>
        public bool Update()
        {
            string sql = "UPDATE `themes` SET guid=@guid,name=@name WHERE id=@id;";
            List<object> parameters = new List<object>
            {
                this.GUID,
                this.Name,
                this.ID
            };
            return Database.Update(sql, parameters);
        }
        /// <summary>
        /// Deletes a theme in the database.
        /// </summary>
        /// <returns>Is deletion successful?</returns>
        public bool Delete()
        {
            string sql = "DELETE FROM `themes` WHERE id=@id;";
            return Database.Delete(sql, this.ID);
        }
        /// <summary>
        /// Selects a theme by ID.
        /// </summary>
        /// <param name="id">The ID to be checked.</param>
        /// <returns>Selected theme or null.</returns>
        public static Theme Read(long id)
        {
            string sql = SELECT_STATEMENT + " WHERE id=@id";
            List<Dictionary<string, object>> reader = Database.Read(sql, id);
            List<Theme> result = BuildFromReader(reader);
            return result != null && result.Count > 0 ? result[0] : null;
        }
        /// <summary>
        /// Selects a theme by name or GUID.
        /// </summary>
        /// <param name="name">Name or GUID.</param>
        /// <param name="stringType">String type.</param>
        /// <returns>Selected theme or null.</returns>
        public static Theme Read(string name, StringType stringType)
        {
            string sql = SELECT_STATEMENT + " WHERE name=@name";
            List<Dictionary<string, object>> reader = Database.Read(sql, name);
            List<Theme> result = BuildFromReader(reader);
            return result != null && result.Count > 0 ? result[0] : null;
        }
        /// <summary>
        /// Selects all themes.
        /// </summary>
        /// <returns>List of themes or null.</returns>
        public static List<Theme> Read()
        {
            List<Dictionary<string, object>> reader = Database.Read(SELECT_STATEMENT);
            return BuildFromReader(reader);
        }
        /// <summary>
        /// Builds a theme list from read database values.
        /// </summary>
        /// <param name="rows">Read database values.</param>
        /// <returns>List of themes or null.</returns>
        private static List<Theme> BuildFromReader(List<Dictionary<string, object>> rows)
        {
            if(rows != null)
            {
                List<Theme> result = new List<Theme>();
                foreach(Dictionary<string, object> row in rows)
                {
                    if(row.Count == 3)
                    {
                        result.Add(new Theme((long)row["id"], (string)row["guid"], (string)row["name"]));
                    }
                    else
                    {
                        continue;
                    }
                }
                return result.Count < 1 ? null : result;
            }
            else return null;
        }
        /// <summary>
        /// Gets all themes associated with a sermon.
        /// </summary>
        /// <param name="sermonId">The ID of the sermon to be checked.</param>
        /// <returns>List of themes associated with a sermon.</returns>
        public static List<Theme> GetSermonThemes(long sermonId)
        {
            List<Dictionary<string, object>> rows = Database.Read("SELECT theme_id FROM `sermon_themes` WHERE sermon_id=@sermonId;", sermonId);
            if(rows != null)
            {
                List<Theme> result = new List<Theme>();
                foreach(Dictionary<string, object> row in rows)
                {
                    Theme theme = new Theme((long)row["theme_id"]);
                    result.Add(theme);
                }
                return result.Count < 1 ? null : result;
            }
            return null;
        }
        /// <summary>
        /// Records themes associated with a sermon in the database.
        /// </summary>
        /// <param name="themes">List of themes associated with a sermon.</param>
        /// <param name="sermonId">The ID of the sermon.</param>
        public static void SetSermonThemes(List<Theme> themes, long sermonId)
        {
            if(themes != null)
            {
                StringBuilder sqlBuilder = new StringBuilder("INSERT INTO `sermon_themes` (sermon_id, theme_id) VALUES ");
                foreach(Theme theme in themes)
                {
                    sqlBuilder.Append(string.Format("({0}, {1}), ", sermonId, theme.ID));
                }
                string sql = sqlBuilder.ToString();
                sql = sql.TrimEnd(' ', ',');
                Database.Create(sql, null);
            }
        }
        /// <summary>
        /// Updates the record of themes associated with a sermon.
        /// </summary>
        /// <param name="themes">List of themes associated with a sermon.</param>
        /// <param name="sermonId">The ID of the sermon.</param>
        public static void UpdateSermonThemes(List<Theme> themes, long sermonId)
        {
            DeleteSermonThemes(sermonId);
            SetSermonThemes(themes, sermonId);
        }
        /// <summary>
        /// Deletes the record of themes associated with a sermon.
        /// </summary>
        /// <param name="sermonId">The ID of the sermon.</param>
        public static void DeleteSermonThemes(long sermonId)
        {
            string sql = "DELETE FROM `sermon_themes` WHERE sermon_id=@sermonId";
            Database.Delete(sql, sermonId);
        }
        /// <summary>
        /// Extract themes from a delimited string.
        /// </summary>
        /// <param name="delimitedString">The delimited string to be parsed.</param>
        /// <param name="delimiter">The delimiter to be used.</param>
        /// <returns>List of themes.</returns>
        public static List<Theme> ExtractFromDelimitedString(string delimitedString, char delimiter)
        {
            List<Theme> themes = new List<Theme>();

            var splits = delimitedString.Split(delimiter);
            foreach(var split in splits)
            {
                themes.Add(new Theme(split.Trim(' '), StringType.Name));
            }
            return themes;
        }
    }
}