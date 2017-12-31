using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;

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

		public Texture2D Texture;
		public LiveListItem Item;
		public LiveScroll Scroll;

		public Color[] Colors;

		public RectTransform rectTrans, coverUiRawImageRectTrans;
		public RawImage coverUiRawImage;
		public Text titleUiText, authorUiText, tagUiText, starsUiText;
		public ColorableSwapable backgroundColorable, textColorable;
		public RectClipHidable coverClipHidable;

		public void OnValidate() {
			rectTrans = GetComponent<RectTransform>();
			if (coverUiRawImage != null) coverUiRawImageRectTrans = coverUiRawImage.GetComponent<RectTransform>();
		}

		public void Start() {
			coverUiRawImage.texture = Texture = new Texture2D(4, 4);
		}

		public void Init(LiveScroll scroll) {
			Scroll = scroll;
		}

		public void Init(byte[] bytes) {
			Texture.LoadImage(bytes);

			var ct = new ColorThiefDotNet.ColorThief();
			var palette = ct.GetPalette(Texture);
			var qColor = ColorThiefDotNet.ColorThief.GetColorFromPalette(palette);
			Colors = ColorThiefDotNet.ColorThief.GetUnityColorsFromPalette(palette);
			backgroundColorable.Swap(qColor.Color.ToUnityColor());

			if (qColor.IsDark) textColorable.Swap(Color.white);

			coverUiRawImage.texture = Texture;
			coverClipHidable.ShowWidth = (float)Texture.width / Texture.height * rectTrans.sizeDelta.y;
			coverClipHidable.Show();
		}

		public void Init(LiveListItem item) {
			Item = item;

			titleUiText.text = item.live_name;
			authorUiText.text = item.artist + " // " + item.upload_user.username;
			tagUiText.text = string.Format("{0:N0} [LEVEL{1:N0}]", item.click_count, item.level);

			string starsText = "";
			for (int i = 0; i < item.level; i++) starsText += "★ ";
			for (int i = item.level; i < 14; i++) starsText += "☆ ";
			starsUiText.text = starsText;
		}

		public void OnButtonPressed() {
			Scroll.OnItemPressed(this);
		}
	}
}

