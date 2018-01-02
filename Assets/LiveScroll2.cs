using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Uif;

public class LiveScroll2 : InfiniteScroll<LiveScroll2Item> {
	public float ItemMinWidth = 360, ItemMaxWidth = 480;
	public EasingType EasingType = EasingType.Cubic;
	public EasingPhase EasingPhase = EasingPhase.Out;

	public bool Dirty;

	public LiveInfoPanel liveInfoPanel;
	public EasedHidable hidable;

	public void Start() {
		uiScroll.onValueChanged.AddListener(value => Dirty = true);

		ScrollTop = rectTrans.rect.yMax - (ItemHeight / 2);
		ScrollBottom = rectTrans.rect.yMin - (ItemHeight / 2);

		RebuildItems();
	}

	public override void RebuildContent() {
		ItemCount = Game.FocusLives.Count;

		base.RebuildContent();
	
		hidable.Show();
	}

	public void Update() {
		if (Dirty) {
			Dirty = false;

			UpdateScroll();

			foreach (var item in items) {
				float step = 1 - (Mathf.Abs(item.Index - contentTop / itemSkip) / visibleItemCount * 2);
				step = Easing.Ease(EasingType, EasingPhase, step);
				item.Width = ItemMinWidth + step * (ItemMaxWidth - ItemMinWidth);
			}
		}
	}

	public override void InitItem(LiveScroll2Item item, int index) {
		base.InitItem(item, index);

		var live = Game.FocusLives[index];

		item.TitleUiText.text = live.name;
		item.AuthorUiText.text = live.artist + " // " + live.uploaderName;
		item.TagUiText.text = string.Format("{0:N0} [LEVEL{1:N0}]", live.clickCount, live.level);

		string starsText = "";
		for (int i = 0; i < live.level; i++) starsText += "★ ";
		for (int i = live.level; i < 14; i++) starsText += "☆ ";
		item.StarsUiText.text = starsText;

		ResourceStore.LoadTexture(live.coverPath, job => {
			var texture = job.GetData();
			live.texture = texture;

			if (live.colors == null || live.colors.Length == 0) {
				var colorThief = new ColorThiefDotNet.ColorThief();
				var palette = colorThief.GetPalette(texture, 10);
				var qColor = ColorThiefDotNet.ColorThief.GetColorFromPalette(palette);
				live.color = qColor.Color.ToUnityColor();
				live.isDark = qColor.IsDark;
				live.colors = ColorThiefDotNet.ColorThief.GetUnityColorsFromPalette(palette);
				Game.SetDirty();
			}
				
			if (item.CoverUiRawImage.texture == null) {
				item.BackgroundColorable.Swap(live.color);
				if (live.isDark) item.TextColorable.Swap(Color.white);
				
				item.CoverUiRawImage.texture = texture;
				item.CoverClipHidable.ShowWidth = (float)texture.width / texture.height * item.rectTrans.sizeDelta.y;
				item.CoverClipHidable.Show();
			} else {
				item.BackgroundColorable.ForceSwap(live.color);
				if (live.isDark) item.TextColorable.ForceSwap(Color.white);

				item.CoverUiRawImage.texture = texture;
				item.CoverUiRawImageRectTrans.sizeDelta = new Vector2((float)texture.width / texture.height * item.rectTrans.sizeDelta.y, item.rectTrans.sizeDelta.y);
			}
		});
	}

	public override void OnItemClick(LiveScroll2Item item) {
		Game.FocusLive = Game.FocusLives[item.Index];
		MenuScheduler.ChangeLive();
	}
}
