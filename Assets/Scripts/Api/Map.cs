using System.Text.RegularExpressions;

namespace LoveLivePractice.Api {
	[System.Serializable]
	public class Map {
		public string audiofile;
		public int speed;
		public Note[] lane;

		public static string Transform(string json) {
			string result = Regex.Replace(json, @"\[\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\]\]", @"[$1,$2,$3,$4,$5,$6,$7,$8,$9]");
			// Fix empty lanes
			result = Regex.Replace(result, @",+", @",");
			result = Regex.Replace(result, @"\[,", @"[");
			result = Regex.Replace(result, @"\,]", @"]");
			return result;
		}
	}

	[System.Serializable]
	public class Note {
		public int lane;
		public double starttime, endtime;
		public bool longnote, parallel, hold;
	}
}