using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;

namespace LoveLivePractice.Unity {
	public class LiveScroll : MonoBehaviour {
		public LiveScrollItem[] Items;
		public GameObject ItemPrototype;

		public float ItemHeight = 70, ItemSpacing = 2, ItemMinWidth = 360, ItemMaxWidth = 480;
		public int Limit = 24, CurrentPage = 0;

		public EasingType EasingType = EasingType.Cubic;
		public EasingPhase EasingPhase = EasingPhase.Out;

		public RectTransform contentRectTrans;
		public ScrollRect uiScrollRect;
		public Hidable hidable;

		public LiveInfoPanel liveInfoPanel;

		int visibleItemCount;
		bool dirty = true;

		public void OnValidate() {
			uiScrollRect = GetComponent<ScrollRect>();
			hidable = GetComponent<Hidable>();
		}

		public void Start() {
			visibleItemCount = (int)(CanvasSizer.GetCanvasHeight() * 0.5f / (ItemHeight + ItemSpacing) + 1);

			ChangePage();
		}

		public void FixedUpdate() {
			if (dirty) {
				float position = contentRectTrans.anchoredPosition.y / (ItemHeight + ItemSpacing);
				int index = (int)position + 1;

				for (int i = Mathf.Max(0, index - visibleItemCount); i < Mathf.Min(Limit, index + visibleItemCount); i++) {
					float step = 1 - (Mathf.Abs(i - position) / visibleItemCount);
					step = Easing.Ease(EasingType, EasingPhase, step);
					Items[i].Width = ItemMinWidth + step * (ItemMaxWidth - ItemMinWidth);
				}
			}
		}

		[ContextMenu("BuildItems")]
		public void BuildItems() {
			if (contentRectTrans.childCount != 0) return;

			Items = new LiveScrollItem[Limit];
			for (int i = 0; i < Limit; i++) {
				var item = Instantiate(ItemPrototype, contentRectTrans).GetComponent<LiveScrollItem>();
				Items[i] = item;

				item.Width = ItemMinWidth;
				item.Y = -i * (ItemHeight + ItemSpacing);

				item.Init(this);
			}

			contentRectTrans.sizeDelta = new Vector2(contentRectTrans.sizeDelta.x, (ItemHeight + ItemSpacing) * Limit - ItemSpacing);
		}

		[ContextMenu("ChangePage")]
		public void ChangePage() {
			string liveListUrl = UrlBuilder.GetLiveListUrl(CurrentPage * Limit, Limit);
			hidable.Hide();

			StartCoroutine(LoadLiveList(liveListUrl));
		}

		System.Collections.IEnumerator LoadLiveList(string url) {
			var www = new WWW(url);

			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);

			var response = JsonUtility.FromJson<LiveListResponse>(www.text);
			var items = response.content.items;

			for (int i = 0; i < response.content.items.Length; i++) {
				Items[i].Init(items[i]);
			}

			hidable.Show();

			for (int i = 0; i < response.content.items.Length; i++) {
				var item = Items[i];
				DataStore.LoadTexture(items[i].cover_path, job => item.Init(job.GetData()));
			}
		}

		public void OnValueChanged(Vector2 value) {
			dirty = true;
		}

		public void OnItemPressed(LiveScrollItem item) {
			liveInfoPanel.ChangeLive(item.Texture, item.Item);
			liveInfoPanel.Colors = item.Colors;
		}
	}
}

