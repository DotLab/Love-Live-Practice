using UnityEngine;
using UnityEngine.UI;

using Uif;

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
		public ColorableSwapable backgroundColorable, textColorable;
		public RectClipHidable coverClipHidable;

		public void OnValidate() {
			rectTrans = GetComponent<RectTransform>();
			if (coverUiRawImage != null) coverUiRawImageRectTrans = coverUiRawImage.GetComponent<RectTransform>();
		}

		public void Init(Texture2D texture) {
			var ct = new ColorThiefDotNet.ColorThief();
			var qColor = ct.GetColor(texture);
			backgroundColorable.Swap(qColor.Color.ToUnityColor());

			if (qColor.IsDark) textColorable.Swap(Color.white);

			coverUiRawImage.texture = texture;
			coverClipHidable.ShowWidth = (float)texture.width / texture.height * rectTrans.sizeDelta.y;
			coverClipHidable.Show();
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

