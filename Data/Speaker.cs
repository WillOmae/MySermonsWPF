﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MySermonsWPF.Data
{
    public class Speaker
    {
        /// <summary>
        /// Default speaker select statement.
        /// </summary>
        private const string SELECT_STATEMENT = "SELECT * FROM `speakers`";
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
                    this.name = "THEME_NOT_SET";
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
        /// Speaker constructor: used when either name or GUID is know.
        /// </summary>
        /// <param name="name">Name or GUID.</param>
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
        /// Speaker constructor: used when only ID is known.
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
        /// Creates a speaker in the database, setting the ID field as the new index.
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
        /// Selects a speaker by name or GUID.
        /// </summary>
        /// <param name="name">Name or GUID.</param>
        /// <param name="stringType">String type.</param>
        /// <returns>Selected speaker or null.</returns>
        public static Speaker Read(string name, StringType stringType)
        {
            string sql = SELECT_STATEMENT + " WHERE name=@name";
            List<Dictionary<string, object>> reader = Database.Read(sql, name);
            List<Speaker> result = BuildFromReader(reader);
            return result != null && result.Count > 0 ? result[0] : null;
        }
        /// <summary>
        /// Selects all speakers.
        /// </summary>
        /// <returns>List of speakers or null.</returns>
        public static List<Speaker> Read()
        {
            List<Dictionary<string, object>> reader = Database.Read(SELECT_STATEMENT);
            return BuildFromReader(reader);
        }
        /// <summary>
        /// Builds a speaker list from read database values.
        /// </summary>
        /// <param name="rows">Read database values.</param>
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
                        result.Add(new Speaker((long)row["id"], (string)row["guid"], (string)row["name"]));
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
        /// Gets all speakers associated with a sermon.
        /// </summary>
        /// <param name="sermonId">The ID of the sermon to be checked.</param>
        /// <returns>List of speakers associated with a sermon.</returns>
        public static List<Speaker> GetSermonSpeakers(long sermonId)
        {
            List<Dictionary<string, object>> rows = Database.Read("SELECT speaker_id FROM `sermon_speakers` WHERE sermon_id=@sermonId;", sermonId);
            if(rows != null)
            {
                List<Speaker> result = new List<Speaker>();
                foreach(Dictionary<string, object> row in rows)
                {
                    Speaker speaker = new Speaker((long)row["speaker_id"]);
                    result.Add(speaker);
                }
                return result.Count < 1 ? null : result;
            }
            return null;
        }
        /// <summary>
        /// Records speakers associated with a sermon in the database.
        /// </summary>
        /// <param name="speakers">List of speakers associated with a sermon.</param>
        /// <param name="sermonId">The ID of the sermon.</param>
        public static void SetSermonSpeakers(List<Speaker> speakers, long sermonId)
        {
            if(speakers != null)
            {
                StringBuilder sqlBuilder = new StringBuilder("INSERT INTO `sermon_speakers` (sermon_id, speaker_id) VALUES ");
                foreach(Speaker speaker in speakers)
                {
                    sqlBuilder.Append(string.Format("({0}, {1}), ", sermonId, speaker.ID));
                }
                string sql = sqlBuilder.ToString();
                sql = sql.TrimEnd(' ', ',');
                Database.Create(sql, null);
            }
        }
        /// <summary>
        /// Updates the record of speakers associated with a sermon.
        /// </summary>
        /// <param name="speakers">List of speakers associated with a sermon.</param>
        /// <param name="sermonId">The ID of the sermon.</param>
        public static void UpdateSermonSpeakers(List<Speaker> speakers, long sermonId)
        {
            DeleteSermonSpeakers(sermonId);
            SetSermonSpeakers(speakers, sermonId);
        }
        /// <summary>
        /// Deletes the record of speakers associated with a sermon.
        /// </summary>
        /// <param name="sermonId">The ID of the sermon.</param>
        public static void DeleteSermonSpeakers(long sermonId)
        {
            string sql = "DELETE FROM `sermon_speakers` WHERE sermon_id=@sermonId";
            Database.Delete(sql, sermonId);
        }
        /// <summary>
        /// Extract speakers from a delimited string.
        /// </summary>
        /// <param name="delimitedString">The delimited string to be parsed.</param>
        /// <param name="delimiter">The delimiter to be used.</param>
        /// <returns>List of speakers.</returns>
        public static List<Speaker> ExtractFromDelimitedString(string delimitedString, char delimiter)
        {
            List<Speaker> speakers = new List<Speaker>();

            var splits = delimitedString.Split(delimiter);
            foreach(var split in splits)
            {
                speakers.Add(new Speaker(split.Trim(' '), StringType.Name));
            }
            return speakers;
        }
    }
}