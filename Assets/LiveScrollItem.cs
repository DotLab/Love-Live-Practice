using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

public class LiveScrollItem : InfiniteScrollItem {
	public Texture2D Texture {
		get { return (Texture2D)CoverUiRawImage.texture; }
	}

	public float Width {
		get { return rectTrans.sizeDelta.x; }
		set { rectTrans.sizeDelta = new Vector2(value, rectTrans.sizeDelta.y); }
	}

	public RectTransform CoverUiRawImageRectTrans;
	public RawImage CoverUiRawImage;

	public Text TitleUiText, AuthorUiText, TagUiText, StarsUiText;
	public ColorableSwapable BackgroundColorable, TextColorable;
	public RectClipHidable CoverClipHidable;

	public void Init(Live live) {
		TitleUiText.text = live.name;
		AuthorUiText.text = live.artist + " // " + live.uploaderName;
		TagUiText.text = string.Format("{0:N0} [LEVEL{1:N0}]", live.clickCount, live.level);

		string starsText = "";
		for (int i = 0; i < live.level; i++) starsText += "★ ";
		for (int i = live.level; i < 14; i++) starsText += "☆ ";
		StarsUiText.text = starsText;

		if (live.texture != null) {
			Init2(live);
		} else {
			ResourceStore.LoadTexture(live.coverPath, job => {
				var texture = job.GetData();
				live.texture = texture;
				Init2(live);
			});
		}
	}

	void Init2(Live live) {
		if (live.colors == null || live.colors.Length == 0) {
//			Debug.Log("Calculate color");
			var colorThiefJob = new ColorThiefDotNet.ColorThief.ColorThiefJob(live.texture, job => {
				var palette = job.GetData();
				var qColor = ColorThiefDotNet.ColorThief.GetColorFromPalette(palette);
				live.color = qColor.Color.ToUnityColor();
				live.isDark = qColor.IsDark;
				live.colors = ColorThiefDotNet.ColorThief.GetUnityColorsFromPalette(palette);
				Game.SetDirty();

				Init3(live);
			});

			colorThiefJob.Start();
		} else {
			Init3(live);
		}
	}

	void Init3(Live live) {
//		if (CoverUiRawImage.texture == null) {
//			BackgroundColorable.Swap(live.color);
//			if (live.isDark) TextColorable.Swap(Color.white);
//
//			CoverUiRawImage.texture = live.texture;
//			CoverClipHidable.ShowWidth = (float)live.texture.width / live.texture.height * rectTrans.sizeDelta.y;
//			CoverClipHidable.Show();
//		} else {
		BackgroundColorable.ForceSwap(live.color);
		if (live.isDark) TextColorable.ForceSwap(Color.white);

		CoverUiRawImage.texture = live.texture;
		CoverUiRawImageRectTrans.sizeDelta = new Vector2((float)live.texture.width / live.texture.height * rectTrans.sizeDelta.y, rectTrans.sizeDelta.y);
//		}
	}
}
