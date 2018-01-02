using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;

namespace LoveLivePractice.Unity {
	public class LiveScrollItem : InfiniteScrollItem {
		public float Width {
			get { return rectTrans.sizeDelta.x; }
			set { rectTrans.sizeDelta = new Vector2(value, rectTrans.sizeDelta.y); }
		}

		public Texture2D Texture {
			get { return (Texture2D)coverUiRawImage.texture; }
		}

		public ApiLiveListItem Item;

		public Color[] Colors;

		public RectTransform coverUiRawImageRectTrans;
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
			var palette = ct.GetPalette(texture, 10);
			var qColor = ColorThiefDotNet.ColorThief.GetColorFromPalette(palette);
			Colors = ColorThiefDotNet.ColorThief.GetUnityColorsFromPalette(palette);
			backgroundColorable.Swap(qColor.Color.ToUnityColor());

			if (qColor.IsDark) textColorable.Swap(Color.white);

			coverUiRawImage.texture = texture;
			coverClipHidable.ShowWidth = (float)texture.width / texture.height * rectTrans.sizeDelta.y;
			coverClipHidable.Show();
		}

		public void Init(ApiLiveListItem item) {
			Item = item;

			titleUiText.text = item.live_name;
			authorUiText.text = item.artist + " // " + item.upload_user.username;
			tagUiText.text = string.Format("{0:N0} [LEVEL{1:N0}]", item.click_count, item.level);

			string starsText = "";
			for (int i = 0; i < item.level; i++) starsText += "★ ";
			for (int i = item.level; i < 14; i++) starsText += "☆ ";
			starsUiText.text = starsText;
		}
	}
}

