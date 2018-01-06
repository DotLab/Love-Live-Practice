using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

public class LiveScrollItem : InfiniteScrollItem {
	public Texture2D Texture {
		get { return (Texture2D)coverUiRawImage.texture; }
	}

	public float Width {
		get { return rectTrans.sizeDelta.x; }
		set { rectTrans.sizeDelta = new Vector2(value, rectTrans.sizeDelta.y); }
	}

	new public ParticleSystem particleSystem;

	public RectTransform coverUiRawImageRectTrans;
	public RawImage coverUiRawImage;

	public Text titleUiText, authorUiText, tagUiText, starsUiText;
	public ColorableSwapable backgroundColorable, textColorable;
	public RectClipHidable coverClipHidable;

	void ClearTexture() {
		coverUiRawImage.texture = null;
		coverUiRawImageRectTrans.sizeDelta = new Vector2(0, rectTrans.sizeDelta.y);
	}

	public void Select(bool forced = false) {
		backgroundColorable.Swap(Color.white, forced);
		textColorable.Swap(Color.black, forced);
		particleSystem.Play();
	}

	public void Deselect(Live live) {
		particleSystem.Stop();
		if (live.colors != null && live.colors.Length != 0) {
			backgroundColorable.Swap(live.color);
			if (live.isDark) textColorable.Swap(Color.white);
			else textColorable.Swap(Color.black);
		}
	}

	public void Init(Live live) {
		ClearTexture();
		particleSystem.Stop();

		titleUiText.text = live.name;
		authorUiText.text = live.artist + " // " + live.uploaderName;
		tagUiText.text = string.Format("{0:N0} [LEVEL{1:N0}]", live.clickCount, live.level);

		string starsText = "";
		for (int i = 0; i < live.level; i++) starsText += "★ ";
		for (int i = live.level; i < 14; i++) starsText += "☆ ";
		starsUiText.text = starsText;

		if (live.texture != null) {
			Init2(live, false);
		} else {
			if (LocalStorage.IsJobPending(live.coverPath)) return;

			LocalStorage.LoadTexture(live.coverPath, job => {
				var texture = job.GetData();
				live.texture = texture;
				Init2(live, true);
			});
		}
	}

	void Init2(Live live, bool ease) {
		if (live.colors == null || live.colors.Length == 0) {
			if (LocalStorage.IsJobPending(live.id)) return;

//			Debug.LogFormat("CT ({0}) start {1:F1}", live.id, Time.time);
			Debug.Log("Calculate colors");
			var colorThiefJob = new ColorThiefDotNet.ColorThief.ColorThiefJob(live.id, live.texture, job => {
				var palette = job.GetData();
				var qColor = ColorThiefDotNet.ColorThief.GetColorFromPalette(palette);
				live.color = qColor.Color.ToUnityColor();
				live.isDark = qColor.IsDark;
				live.colors = ColorThiefDotNet.ColorThief.GetUnityColorsFromPalette(palette);
				Game.SetDirty();
//				Debug.LogFormat("CT ({0}) end {1:F1}", live.id, Time.time);

				Init3(live, true);
			}, 10, 10, false);

			colorThiefJob.Start();
		} else {
			Init3(live, ease);
		}
	}

	void Init3(Live live, bool ease) {
		if (Game.ActiveLive == live) {
			Select(!ease);
		} else {
			backgroundColorable.Swap(live.color, !ease);
			if (live.isDark) textColorable.Swap(Color.white, !ease);
			else textColorable.Swap(Color.black, !ease);
		}

		if (ease) {
			coverUiRawImage.texture = live.texture;
			coverClipHidable.ShowWidth = (float)live.texture.width / live.texture.height * rectTrans.sizeDelta.y;
			coverClipHidable.Show();
		} else {
			coverUiRawImage.texture = live.texture;
			coverUiRawImageRectTrans.sizeDelta = new Vector2((float)live.texture.width / live.texture.height * rectTrans.sizeDelta.y, rectTrans.sizeDelta.y);
		}
	}
}
