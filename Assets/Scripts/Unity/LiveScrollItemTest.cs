using UnityEngine;

namespace LoveLivePractice.Unity {
	public class LiveScrollItemTest : MonoBehaviour {
		public Texture2D Texture;
		public string Title = "[創作譜面]近未来ハッピーエンド", Author = "Aqours // gytjhkj", Tag = "TECHNICAL";
		public int Stars = 11;

		public void OnValidate() {
			GetComponent<LiveScrollItem>().Init(Title, Author, Tag, Stars);
			if (Texture != null) GetComponent<LiveScrollItem>().Init(Texture.GetRawTextureData());

			GetComponent<LiveScrollItem>().Width = 360;
		}
	}
}

