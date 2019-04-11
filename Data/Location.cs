using System;
using System.Collections.Generic;

namespace MySermonsWPF.Data
{
    /// <summary>
    /// Model for location information.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Default location select statement.
        /// </summary>
        private const string SELECT_STATEMENT = "SELECT * FROM locations";
        /// <summary>
        /// Property: Location ID depends on position in database.
        /// </summary>
        public long ID { get { return this.id; } }
        /// <summary>
        /// Property: Unique GUID identifier for the location.
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
        /// Property: Unique name for the location.
        /// </summary>
        public string Name
        {
            get
            {
                if(string.IsNullOrEmpty(this.name))
                {
                    this.name = "LOCATION_NOT_SET";
                }
                return this.name;
            }
        }
        /// <summary>
        /// Field: Location ID depends on position in database.
        /// </summary>
        private long id = -1;
        /// <summary>
        /// Field: Unique GUID identifier for the location.
        /// </summary>
        private string guid = null;
        /// <summary>
        /// Field: Unique name for the location.
        /// </summary>
        private string name = null;

        /// <summary>
        /// Location constructor: used when all properties are known.
        /// </summary>
        /// <param name="id">Location ID.</param>
        /// <param name="guid">Location GUID.</param>
        /// <param name="name">Location name.</param>
        public Location(long id, string guid, string name)
        {
            this.id = id;
            this.guid = guid;
            this.name = name;
        }
        /// <summary>
        /// Location constructor: used when only the name is known.
        /// </summary>
        /// <param name="name">Location name.</param>
        /// <param name="stringType">String type.</param>
        public Location(string name, StringType stringType)
        {
            Location location = Read(name, stringType);
            if(location == null)
            {
                this.guid = Guid.NewGuid().ToString();
                this.name = name;
                this.Create();
            }
            else
            {
                this.id = location.id;
                this.guid = location.guid;
                this.name = location.name;
            }
        }
        /// <summary>
        /// Location constructor: used when only the ID is known.
        /// </summary>
        /// <param name="id"></param>
        public Location(long id)
        {
            Location location = Read(id);
            if(location == null)
            {
                throw new Exception("The location with the set id was not found.");
            }
            else
            {
                this.id = location.id;
                this.guid = location.guid;
                this.name = location.name;
            }
        }
        /// <summary>
        /// Creates a location in the database, setting the id field as the new location index.
        /// </summary>
        /// <returns>Is creation successful?</returns>
        public bool Create()
        {
            string sql = "INSERT INTO `locations`(guid,name) VALUES (@guid,@name);";
            List<object> parameters = new List<object>
            {
                this.GUID,
                this.Name
            };
            return Database.Create(sql, parameters, out this.id);
        }
        /// <summary>
        /// Updates a location in the database.
        /// </summary>
        /// <returns>Is update successful?</returns>
        public bool Update()
        {
            string sql = "UPDATE `locations` SET guid=@guid,name=@name WHERE id=@id;";
            List<object> parameters = new List<object>
            {
                this.GUID,
                this.Name,
                this.ID
            };
            return Database.Update(sql, parameters);
        }
        /// <summary>
        /// Deletes a location in the database.
        /// </summary>
        /// <returns>Is deletion successful?</returns>
        public bool Delete()
        {
            string sql = "DELETE FROM `locations` WHERE id=@id;";
            return Database.Delete(sql, this.ID);
        }
        /// <summary>
        /// Selects a location by ID.
        /// </summary>
        /// <param name="id">The ID to be checked.</param>
        /// <returns>Selected location or null.</returns>
        public static Location Read(long id)
        {
            string sql = SELECT_STATEMENT + " WHERE id=@id";
            List<Dictionary<string, object>> reader = Database.Read(sql, id);
            List<Location> result = BuildFromReader(reader);
            return result != null && result.Count > 0 ? result[0] : null;
        }
        /// <summary>
        /// Selects a location by name.
        /// </summary>
        /// <param name="name">The name to be checked.</param>
        /// <returns>Selected location or null.</returns>
        public static Location Read(string name, StringType stringType)
        {
            string sql = SELECT_STATEMENT;
            switch(stringType)
            {
                case StringType.Name:
                    sql += " WHERE name=@name";
                    break;
                case StringType.Guid:
                    sql += " WHERE guid=@guid";
                    break;
            }
            List<Dictionary<string, object>> reader = Database.Read(sql, name);
            List<Location> result = BuildFromReader(reader);
            return result != null && result.Count > 0 ? result[0] : null;
        }
        /// <summary>
        /// Selects all locations.
        /// </summary>
        /// <returns>All selected locations or null.</returns>
        public static List<Location> Read()
        {
            List<Dictionary<string, object>> reader = Database.Read(SELECT_STATEMENT);
            return BuildFromReader(reader);
        }
        /// <summary>
        /// Builds a location list from read database values.
        /// </summary>
        /// <param name="rows">List of dictionary values to be parsed.</param>
        /// <returns>List of locations or null.</returns>
        private static List<Location> BuildFromReader(List<Dictionary<string, object>> rows)
        {
            if(rows != null)
            {
                List<Location> result = new List<Location>();
                foreach(Dictionary<string, object> row in rows)
                {
                    if(row.Count == 3)
                    {
                        Location location = new Location((long)row["id"], (string)row["guid"], (string)row["name"]);
                        result.Add(location);
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
    }
}