using System.Collections.Generic;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace MySermonsWPF.Data
{
    /// <summary>
    /// Static class to handle all database functions.
    /// </summary>
    public static class Database
    {
        #region CONSTANTS
        /// <summary>
        /// Connection string, specifying the database name and SQLite version.
        /// </summary>
        private static string CONNECTION_STRING => GenerateConnectionString();
        /// <summary>
        /// Sermons table name.
        /// </summary>
        private const string TABLE_SERMONS = "sermons";
        /// <summary>
        /// Locations table name.
        /// </summary>
        private const string TABLE_LOCATIONS = "locations";
        /// <summary>
        /// Themes table name.
        /// </summary>
        private const string TABLE_THEMES = "themes";
        /// <summary>
        /// Sermon themes table name.
        /// </summary>
        private const string TABLE_SERMON_THEMES = "sermon_themes";
        /// <summary>
        /// Query for creating `sermons` table.
        /// </summary>
        private const string CREATE_TABLE_SERMONS = "CREATE TABLE `sermons` ( `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `guid` TEXT NOT NULL UNIQUE, `title` TEXT NOT NULL, `location` INTEGER NOT NULL, `date_created` INTEGER NOT NULL, `last_access_date` INTEGER NOT NULL, `key_verse` TEXT, `other_metadata` TEXT, `content` TEXT NOT NULL )";
        /// <summary>
        /// Query for creating `locations` table.
        /// </summary>
        private const string CREATE_TABLE_LOCATIONS = "CREATE TABLE `locations` ( `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `guid` TEXT NOT NULL UNIQUE, `name` TEXT NOT NULL )";
        /// <summary>
        /// Query for creating `themes` table.
        /// </summary>
        private const string CREATE_TABLE_THEMES = "CREATE TABLE `themes` ( `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `guid` TEXT NOT NULL UNIQUE, `name` TEXT NOT NULL )";
        /// <summary>
        /// Query for creating `sermon_themes` table.
        /// </summary>
        private const string CREATE_TABLE_SERMON_THEMES = "CREATE TABLE `sermon_themes` ( `sermon_id` INTEGER NOT NULL, `theme_id` INTEGER NOT NULL, PRIMARY KEY(`sermon_id`,`theme_id`) )";
        #endregion

        /// <summary>
        /// Initialises the database.
        /// </summary>
        /// <returns>Initialisation successful?</returns>
        public static bool Initialise()
        {
            //since c# logically short circuits...first check existence, then check creation, then check existence again, then return false
            if (!TableExists(TABLE_SERMONS) && !Create(CREATE_TABLE_SERMONS, TABLE_SERMONS) && !TableExists(TABLE_SERMONS)) return false;
            if (!TableExists(TABLE_LOCATIONS) && !Create(CREATE_TABLE_LOCATIONS, TABLE_LOCATIONS) && !TableExists(TABLE_LOCATIONS)) return false;
            if (!TableExists(TABLE_THEMES) && !Create(CREATE_TABLE_THEMES, TABLE_THEMES) && !TableExists(TABLE_THEMES)) return false;
            if (!TableExists(TABLE_SERMON_THEMES) && !Create(CREATE_TABLE_SERMON_THEMES, TABLE_SERMON_THEMES) && !TableExists(TABLE_SERMON_THEMES)) return false;

            return true;
        }
        /// <summary>
        /// Programmatically creates the connection string.
        /// </summary>
        /// <returns>The connection string.</returns>
        private static string GenerateConnectionString()
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder
            {
                // set the db name
                DataSource = "mysermons.chest",
                // this will create the database if it is non-existent
                FailIfMissing = false,
                // allow foreign keys
                ForeignKeys = true,
                // set the journal mode
                JournalMode = SQLiteJournalModeEnum.Delete,
                // sqlite version to use
                Version = 3
            };
            return builder.ToString();
        }
        /// <summary>
        /// Confirms the existence of a table in the database.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>Does table exist?</returns>
        private static bool TableExists(string tableName)
        {
            /* simply query the sqlite_master table for the table name */
            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            List<Dictionary<string, object>> result = Read(sql, tableName);
            return result != null && result.Count > 0 && (string) result[0]["name"] == tableName ? true : false;
        }
        /// <summary>
        /// Queries the sql statement, getting all instances of parameter names denoted by @.
        /// </summary>
        /// <param name="sql">The sql to be queried.</param>
        /// <returns>String array of parameter names or null for no match.</returns>
        private static string[] ExtractParameterNames(string sql)
        {
            // create regex for parameter names
            Regex regex = new Regex(@"@\w*");
            MatchCollection matches = regex.Matches(sql);
            if (matches.Count > 0)
            {
                string[] parameterNames = new string[matches.Count];
                for (int i = 0; i < parameterNames.Length; i++)
                {
                    parameterNames[i] = matches[i].Value;
                }
                return parameterNames;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Database create method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names when applicable.</param>
        /// <param name="parameter">Single parameter.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Create(string sql, object parameter) => Create(sql, parameter, out long dummy);
        /// <summary>
        /// Database create method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names when applicable.</param>
        /// <param name="parameter">List of parameters.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Create(string sql, List<object> parameters) => Create(sql, parameters, out long dummy);
        /// <summary>
        /// Database create method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names when applicable.</param>
        /// <param name="parameter">Single parameter.</param>
        /// <param name="insertId">(out param) The ID of the last inserted row in the connection if successful; -1 otherwise.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Create(string sql, object parameter, out long insertId) => Create(sql, new List<object>() { parameter }, out insertId);
        /// <summary>
        /// Database create method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names when applicable.</param>
        /// <param name="parameters">List of parameters.</param>
        /// <param name="insertId">(out param) The ID of the last inserted row in the connection if successful; -1 otherwise.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Create(string sql, List<object> parameters, out long insertId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        string[] parameterNames = ExtractParameterNames(sql);
                        if (parameterNames != null && parameterNames.Length == parameters.Count)
                        {
                            for (int i = 0; i < parameterNames.Length; i++)
                            {
                                command.Parameters.AddWithValue(parameterNames[i], parameters[i]);
                            }
                        }
                    }
                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            insertId = connection.LastInsertRowId;
                            return true;
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR DURING SQLITE WRITE");
                        insertId = -1;
                        return false;
                    }
                }
            }
            insertId = -1;
            return false;
        }
        /// <summary>
        /// Database read method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names where applicable.</param>
        /// <returns>SQLiteDataReader if successful; null otherwise</returns>
        public static List<Dictionary<string, object>> Read(string sql) => Read(sql, null);
        /// <summary>
        /// Database read method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names where applicable.</param>
        /// <param name="parameter">Single parameter.</param>
        /// <returns>SQLiteDataReader if successful; null otherwise</returns>
        public static List<Dictionary<string, object>> Read(string sql, object parameter) => Read(sql, new List<object>() { parameter });
        /// <summary>
        /// Database read method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names where applicable.</param>
        /// <param name="parameters">List of parameters.</param>
        /// <returns>SQLiteDataReader if successful; null otherwise</returns>
        public static List<Dictionary<string, object>> Read(string sql, List<object> parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        string[] parameterNames = ExtractParameterNames(sql);
                        if (parameterNames != null && parameterNames.Length == parameters.Count)
                        {
                            for (int i = 0; i < parameterNames.Length; i++)
                            {
                                command.Parameters.AddWithValue(parameterNames[i], parameters[i]);
                            }
                        }
                    }
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            // dictionary is for column cells, list for rows
                            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>(reader.FieldCount);
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(reader.GetName(i), reader.GetValue(i));
                                }
                                rows.Add(row);
                            }
                            if (rows.Count > 0)
                            {
                                return rows;
                            }
                        }
                        return null;
                    }
                }
            }
        }
        /// <summary>
        /// Database update statement.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names where applicable.</param>
        /// <param name="parameter">Single parameter.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Update(string sql, object parameter) => Create(sql, parameter);
        /// <summary>
        /// Database update statement.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names where applicable.</param>
        /// <param name="parameters">List of parameters.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Update(string sql, List<object> parameters) => Create(sql, parameters);
        /// <summary>
        /// Database delete method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names where applicable.</param>
        /// <param name="parameter">Single parameter.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Delete(string sql, object parameter) => Create(sql, parameter);
        /// <summary>
        /// Database delete method.
        /// </summary>
        /// <param name="sql">Sql statement with parameter names where applicable.</param>
        /// <param name="parameters">List of parameters.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public static bool Delete(string sql, List<object> parameters) => Create(sql, parameters);
    }
}
