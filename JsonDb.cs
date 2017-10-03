using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace UmbracoLibrary.Data.JsonDb
{
	public class JsonDbClient
	{

		public JsonDbClient()
		{
		}


		public List<T> GetAll<T>()
		{
			var json = File.ReadAllText(GetDataFile<T>().ToString());
			var db = JsonConvert.DeserializeObject<List<T>>(json, AppSettings.App.JsonSerializerSettings);
			return db;
		}


		public T Get<T>(int id)
		{
			var json = File.ReadAllText(GetDataFile<T>().ToString());
			var db = JsonConvert.DeserializeObject<List<T>>(json, AppSettings.App.JsonSerializerSettings);

			// cast it to IEntity so we can get by id
			var data = db.Cast<IEntity>().ToList();

			// get the object by id
			var obj = data.Where(x => x.Id == id).FirstOrDefault();

			return (T)obj;
		}


		public void Save<T>(IEntity model)
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
				var list = new List<IEntity>();
				list.Add(model);
				newJson = JsonConvert.SerializeObject(list, AppSettings.App.JsonSerializerSettings);
			}
			else
			{
				// deserialze json into our collection
				var db = JsonConvert.DeserializeObject<List<T>>(json, AppSettings.App.JsonSerializerSettings);

				// cast it to IEntity so we can get by id
				var data = db.Cast<IEntity>().ToList();

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


		public void SaveRange<T>(IEnumerable<IEntity> models)
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
				var db = JsonConvert.DeserializeObject<List<T>>(json, AppSettings.App.JsonSerializerSettings);

				// cast it to IEntity so we can get by id
				var data = db.Cast<IEntity>().ToList();

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


		public void Delete<T>(IEntity model)
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
				var db = JsonConvert.DeserializeObject<List<T>>(json, AppSettings.App.JsonSerializerSettings);

				// cast it to IEntity so we can get by id
				var data = db.Cast<IEntity>().ToList();

				// we delete it
				var obj = data.RemoveAll(x => x.Id == model.Id);

				// serialize it back
				newJson = JsonConvert.SerializeObject(data);

			}

			// save
			SaveData(fi, newJson);

		}


		public void DeleteRange<T>(IEnumerable<IEntity> models)
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
				var db = JsonConvert.DeserializeObject<List<T>>(json, AppSettings.App.JsonSerializerSettings);

				// cast it to IEntity so we can get by id
				var data = db.Cast<IEntity>().ToList();

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

			string path = HttpContext.Current.Server.MapPath("/App_Data/JsonDb");

			DirectoryInfo tempDirectory = new DirectoryInfo(path);

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
