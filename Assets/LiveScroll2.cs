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

		item.Init(Game.FocusLives[index]);
	}

	public override void OnItemClick(LiveScroll2Item item) {
		Game.FocusLive = Game.FocusLives[item.Index];
		MenuScheduler.ChangeLive();
	}
}
