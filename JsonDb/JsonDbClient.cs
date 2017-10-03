using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace JsonDb
{
    public class JsonDbClient
    {

        public JsonDbClient()
        {

            // setup serializer settings
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            serializerSettings.Formatting = Formatting.None;
            serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            _jsonSerializerSettings = serializerSettings;

        }


        private JsonSerializerSettings _jsonSerializerSettings;
        /// <summary>
        /// Json Serializer settings for serializing/deserializing the json
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return _jsonSerializerSettings;
            }
            set
            {
                _jsonSerializerSettings = value;
            }
        }


        /// <summary>
        /// Get all objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public List<T> GetAll<T>()
        {
            var json = File.ReadAllText(GetDataFile<T>().ToString());
            var db = JsonConvert.DeserializeObject<List<T>>(json, JsonSerializerSettings);
            return db;
        }


        /// <summary>
        /// Get object by Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">Object Id</param>
        public T Get<T>(int id)
        {
            var json = File.ReadAllText(GetDataFile<T>().ToString());
            var db = JsonConvert.DeserializeObject<List<T>>(json, JsonSerializerSettings);

            // cast it to IEntity so we can get by id
            var data = db.Cast<IJsonDbEntity>().ToList();

            // get the object by id
            var obj = data.Where(x => x.Id == id).FirstOrDefault();

            return (T)obj;
        }


        /// <summary>
        /// Saves object to json file db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">Object to save</param>
        public void Save<T>(IJsonDbEntity model)
        {

            if (model == null)
                throw new Exception("model cannot be of null value");

            string newJson = String.Empty;
            var fi = GetDataFile<T>();
            var json = File.ReadAllText(fi.ToString());

            // if the file is empty write out the data
            if (String.IsNullOrEmpty(json))
            {
                // serialize
                var list = new List<IJsonDbEntity>();
                list.Add(model);
                newJson = JsonConvert.SerializeObject(list, JsonSerializerSettings);
            }
            else
            {
                // deserialze json into our collection
                var db = JsonConvert.DeserializeObject<List<T>>(json, JsonSerializerSettings);

                // cast it to IEntity so we can get by id
                var data = db.Cast<IJsonDbEntity>().ToList();

                // we can just delete the old ones
                var obj = data.RemoveAll(x => x.Id == model.Id);

                // add the new one
                data.Add(model);

                // serialize it back
                newJson = JsonConvert.SerializeObject(data);

            }

            // save
            SaveData(fi, newJson);

        }


        /// <summary>
        /// Saves range of objects to json file db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">Objects to save</param>
        public void SaveRange<T>(IEnumerable<IJsonDbEntity> models)
        {

            if (models == null)
                throw new Exception("models cannot be of null value");

            string newJson = String.Empty;
            var fi = GetDataFile<T>();
            var json = File.ReadAllText(fi.ToString());

            // if the file is empty write out the data
            if (String.IsNullOrEmpty(json))
            {
                // serialize
                newJson = JsonConvert.SerializeObject(models);
            }
            else
            {
                // deserialze json into our collection
                var db = JsonConvert.DeserializeObject<List<T>>(json, JsonSerializerSettings);

                // cast it to IEntity so we can get by id
                var data = db.Cast<IJsonDbEntity>().ToList();

                foreach (var model in models)
                {

                    // we can just delete the old ones
                    var obj = data.RemoveAll(x => x.Id == model.Id);

                    // add the new one
                    data.Add(model);

                }

                // serialize it back
                newJson = JsonConvert.SerializeObject(data);

            }

            // save
            SaveData(fi, newJson);

        }


        /// <summary>
        /// Deletes an object from the json file db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">Object to delete</param>
        public void Delete<T>(IJsonDbEntity model)
        {

            if (model == null)
                throw new Exception("model cannot be of null value");

            string newJson = String.Empty;
            var fi = GetDataFile<T>();
            var json = File.ReadAllText(fi.ToString());

            // if the file is empty write out the data
            if (!String.IsNullOrEmpty(json))
            {
                // deserialze json into our collection
                var db = JsonConvert.DeserializeObject<List<T>>(json, JsonSerializerSettings);

                // cast it to IEntity so we can get by id
                var data = db.Cast<IJsonDbEntity>().ToList();

                // we delete it
                var obj = data.RemoveAll(x => x.Id == model.Id);

                // serialize it back
                newJson = JsonConvert.SerializeObject(data);

            }

            // save
            SaveData(fi, newJson);

        }


        /// <summary>
        /// Deletes a range of objects from the json db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">Objects to delete</param>
        public void DeleteRange<T>(IEnumerable<IJsonDbEntity> models)
        {

            if (models == null)
                throw new Exception("model cannot be of null value");

            string newJson = String.Empty;
            var fi = GetDataFile<T>();
            var json = File.ReadAllText(fi.ToString());

            // if the file is empty write out the data
            if (!String.IsNullOrEmpty(json))
            {
                // deserialze json into our collection
                var db = JsonConvert.DeserializeObject<List<T>>(json, JsonSerializerSettings);

                // cast it to IEntity so we can get by id
                var data = db.Cast<IJsonDbEntity>().ToList();

                // we delete it
                foreach (var del in models)
                {
                    data.RemoveAll(x => x.Id == del.Id);
                }

                // serialize it back
                newJson = JsonConvert.SerializeObject(data);

            }

            // save
            SaveData(fi, newJson);

        }


        /// <summary>
        /// Last time the objects file db was updated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public DateTime GetLastModifiedUtc<T>()
        {
            string dataFilePath = DataFilePath<T>();

            // if the file does not exist yet, return MinValue
            if (!File.Exists(dataFilePath))
                return DateTime.MinValue;

            var fi = new FileInfo(dataFilePath);
            return fi.LastWriteTimeUtc;
        }


        private void SaveData(FileInfo fi, string jsonData)
        {
            // clear data
            File.WriteAllText(fi.ToString(), string.Empty);

            // write out the new json
            using (FileStream fs = fi.OpenWrite())
            {
                Byte[] txt = new UTF8Encoding(true).GetBytes(jsonData);
                fs.Write(txt, 0, txt.Length);
            }
        }


        private DirectoryInfo GetDataDirectory()
        {

            DirectoryInfo tempDirectory = new DirectoryInfo(Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data\\JsonDb"));

            if (!tempDirectory.Exists)
                tempDirectory.Create();

            return tempDirectory;

        }


        private FileInfo GetDataFile<T>()
        {

            string dataFilePath = DataFilePath<T>();

            if (!File.Exists(dataFilePath))
                CreateDataFile(dataFilePath);

            var fi = new FileInfo(dataFilePath);

            return fi;
        }


        private void CreateDataFile(string file)
        {
            FileInfo fi = new FileInfo(file);
            using (FileStream fs = fi.Create())
            {
                Byte[] txt = new UTF8Encoding(true).GetBytes("");
                fs.Write(txt, 0, txt.Length);
            }
        }


        private string DataFilePath<T>()
        {
            return GetDataDirectory().ToString() + "\\" + typeof(T).Name + ".json";
        }

    }
}
