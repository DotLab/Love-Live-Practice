using UnityEngine;
using UnityEngine.UI;

namespace LoveLivePractice.Unity {
	public class LiveScrollItem : MonoBehaviour {
		public float Width {
			get { return rectTrans.sizeDelta.x; }
			set { rectTrans.sizeDelta = new Vector2(value, rectTrans.sizeDelta.y); }
		}

		public float Y {
			get { return rectTrans.anchoredPosition.y; }
			set { rectTrans.anchoredPosition = new Vector2(rectTrans.anchoredPosition.x, value); }
		}

		public RectTransform rectTrans, coverUiRawImageRectTrans;
		public RawImage coverUiRawImage;
		public Text titleUiText, authorUiText, tagUiText, starsUiText;

		public void OnValidate() {
			rectTrans = GetComponent<RectTransform>();
			if (coverUiRawImage != null) coverUiRawImageRectTrans = coverUiRawImage.GetComponent<RectTransform>();
		}

		public void Init(Texture texture) {
			coverUiRawImage.texture = texture;
			coverUiRawImageRectTrans.sizeDelta = new Vector2(
				(float)texture.width / texture.height * rectTrans.sizeDelta.y, 
				rectTrans.sizeDelta.y);
		}

		public void Init(string title, string author, string tag, int stars) {
			titleUiText.text = title;
			authorUiText.text = author;
			tagUiText.text = tag;

			string starsText = "";
			for (int i = 0; i < stars; i++) starsText += "★ ";
			for (int i = stars; i < 14; i++) starsText += "☆ ";
			starsUiText.text = starsText;
		}
	}
}

