using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCFilter
{
	public class FilterStorage
	{
		[Description("List of NPCs to prevent from spawning.")]
		public List<int> FilteredNPCs = new List<int>();


		public static FilterStorage Read(string path)
		{
			if (!File.Exists(path))
				return new FilterStorage();
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return Read(fs);
			}
		}

		public static FilterStorage Read(Stream stream)
		{
			using (var sr = new StreamReader(stream))
			{
				var cf = JsonConvert.DeserializeObject<FilterStorage>(sr.ReadToEnd());
				if (FilterRead != null)
					FilterRead(cf);
				return cf;
			}
		}

		public void Write(string path)
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				Write(fs);
			}
		}

		public void Write(Stream stream)
		{
			var str = JsonConvert.SerializeObject(this, Formatting.Indented);
			using (var sw = new StreamWriter(stream))
			{
				sw.Write(str);
			}
		}

		public static Action<FilterStorage> FilterRead;
	}

	
}
