using System;
using System.Collections.Generic;

namespace MySermonsWPF.Data
{
    /// <summary>
    /// Model for speaker information.
    /// </summary>
    public class Speaker
    {
        /// <summary>
        /// Default speaker select statement.
        /// </summary>
        private const string SELECT_STATEMENT = "SELECT * FROM speakers";
        /// <summary>
        /// Property: Speaker ID depends on position in database.
        /// </summary>
        public long ID { get { return this.id; } }
        /// <summary>
        /// Property: Unique GUID identifier for the speaker.
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
        /// Property: Unique name for the speaker.
        /// </summary>
        public string Name
        {
            get
            {
                if(string.IsNullOrEmpty(this.name))
                {
                    this.name = "SPEAKER_NOT_SET";
                }
                return this.name;
            }
        }
        /// <summary>
        /// Field: Speaker ID depends on position in database.
        /// </summary>
        private long id = -1;
        /// <summary>
        /// Field: Unique GUID identifier for the speaker.
        /// </summary>
        private string guid = null;
        /// <summary>
        /// Field: Unique name for the speaker.
        /// </summary>
        private string name = null;

        /// <summary>
        /// Speaker constructor: used when all properties are known.
        /// </summary>
        /// <param name="id">Speaker ID.</param>
        /// <param name="guid">Speaker GUID.</param>
        /// <param name="name">Speaker name.</param>
        public Speaker(long id, string guid, string name)
        {
            this.id = id;
            this.guid = guid;
            this.name = name;
        }
        /// <summary>
        /// Speaker constructor: used when only the name is known.
        /// </summary>
        /// <param name="name">Speaker name.</param>
        /// <param name="stringType">String type.</param>
        public Speaker(string name, StringType stringType)
        {
            Speaker speaker = Read(name, stringType);
            if(speaker == null)
            {
                this.guid = Guid.NewGuid().ToString();
                this.name = name;
                this.Create();
            }
            else
            {
                this.id = speaker.id;
                this.guid = speaker.guid;
                this.name = speaker.name;
            }
        }
        /// <summary>
        /// Speaker constructor: used when only the ID is known.
        /// </summary>
        /// <param name="id"></param>
        public Speaker(long id)
        {
            Speaker speaker = Read(id);
            if(speaker == null)
            {
                throw new Exception("The speaker with the set id was not found.");
            }
            else
            {
                this.id = speaker.id;
                this.guid = speaker.guid;
                this.name = speaker.name;
            }
        }
        /// <summary>
        /// Creates a speaker in the database, setting the id field as the new speaker index.
        /// </summary>
        /// <returns>Is creation successful?</returns>
        public bool Create()
        {
            string sql = "INSERT INTO `speakers`(guid,name) VALUES (@guid,@name);";
            List<object> parameters = new List<object>
            {
                this.GUID,
                this.Name
            };
            return Database.Create(sql, parameters, out this.id);
        }
        /// <summary>
        /// Updates a speaker in the database.
        /// </summary>
        /// <returns>Is update successful?</returns>
        public bool Update()
        {
            string sql = "UPDATE `speakers` SET guid=@guid,name=@name WHERE id=@id;";
            List<object> parameters = new List<object>
            {
                this.GUID,
                this.Name,
                this.ID
            };
            return Database.Update(sql, parameters);
        }
        /// <summary>
        /// Deletes a speaker in the database.
        /// </summary>
        /// <returns>Is deletion successful?</returns>
        public bool Delete()
        {
            string sql = "DELETE FROM `speakers` WHERE id=@id;";
            return Database.Delete(sql, this.ID);
        }
        /// <summary>
        /// Selects a speaker by ID.
        /// </summary>
        /// <param name="id">The ID to be checked.</param>
        /// <returns>Selected speaker or null.</returns>
        public static Speaker Read(long id)
        {
            string sql = SELECT_STATEMENT + " WHERE id=@id";
            List<Dictionary<string, object>> reader = Database.Read(sql, id);
            List<Speaker> result = BuildFromReader(reader);
            return result != null && result.Count > 0 ? result[0] : null;
        }
        /// <summary>
        /// Selects a speaker by name.
        /// </summary>
        /// <param name="name">The name to be checked.</param>
        /// <returns>Selected speaker or null.</returns>
        public static Speaker Read(string name, StringType stringType)
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
            List<Speaker> result = BuildFromReader(reader);
            return result != null && result.Count > 0 ? result[0] : null;
        }
        /// <summary>
        /// Selects all speakers.
        /// </summary>
        /// <returns>All selected speakers or null.</returns>
        public static List<Speaker> Read()
        {
            List<Dictionary<string, object>> reader = Database.Read(SELECT_STATEMENT);
            return BuildFromReader(reader);
        }
        /// <summary>
        /// Builds a speaker list from read database values.
        /// </summary>
        /// <param name="rows">List of dictionary values to be parsed.</param>
        /// <returns>List of speakers or null.</returns>
        private static List<Speaker> BuildFromReader(List<Dictionary<string, object>> rows)
        {
            if(rows != null)
            {
                List<Speaker> result = new List<Speaker>();
                foreach(Dictionary<string, object> row in rows)
                {
                    if(row.Count == 3)
                    {
                        Speaker speaker = new Speaker((long)row["id"], (string)row["guid"], (string)row["name"]);
                        result.Add(speaker);
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