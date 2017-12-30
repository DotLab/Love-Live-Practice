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

		public RectTransform contentRectTrans;
		public ScrollRect uiScrollRect;

		public int visibleItemCount;

		public void OnValidate() {
			uiScrollRect = GetComponent<ScrollRect>();
		}

		public void Start() {
			ChangePage();

			visibleItemCount = (int)(CanvasSizer.GetCanvasHeight() * 0.5f / (ItemHeight + ItemSpacing) + 1);
		}

		public void OnEnable() {
			uiScrollRect.onValueChanged.AddListener(OnValueChanged);
		}

		public void OnDisable() {
			uiScrollRect.onValueChanged.RemoveListener(OnValueChanged);
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
			}

			contentRectTrans.sizeDelta = new Vector2(contentRectTrans.sizeDelta.x, (ItemHeight + ItemSpacing) * Limit - ItemSpacing);
		}

		[ContextMenu("ChangePage")]
		public void ChangePage() {
			string liveListUrl = UrlBuilder.GetLiveListUrl(CurrentPage * Limit, Limit);

			StartCoroutine(LoadLiveList(liveListUrl));
		}

		System.Collections.IEnumerator LoadLiveList(string url) {
			var www = new WWW(url);

			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);

			var response = JsonUtility.FromJson<LiveListResponse>(www.text);
			for (int i = 0; i < response.content.items.Length; i++) {
				var item = response.content.items[i];

				Items[i].Init(item.live_name, item.artist + " // " + item.upload_user.username, item.category.name, item.level);
			}
		}

		public void OnValueChanged(Vector2 value) {
			float position = contentRectTrans.anchoredPosition.y / (ItemHeight + ItemSpacing);
			int index = (int)position + 1;

			for (int i = Mathf.Max(0, index - visibleItemCount); i < Mathf.Min(Limit, index + visibleItemCount); i++) {
				float itemPosition = i;
				float step = 1 - (Mathf.Abs(itemPosition - position) / visibleItemCount);
				step = Easing.EaseOut(step, EasingType);
				Items[i].Width = ItemMinWidth + step * (ItemMaxWidth - ItemMinWidth);
				//Items[i].Init(itemPosition.ToString(), (position - itemPosition).ToString(), step.ToString(), 0);
			}
		}
	}
}

