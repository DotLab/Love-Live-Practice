using UnityEngine;

namespace Uif {
	[AddComponentMenu("Uif/Hidable/Rect Panel Slide Hidable")]
	public class RectPanelSlideHidable : EasedHidable {
		public RectTransform Rect;

		[Header("Lower Left : Upper Right")]
		public Vector2 ShowMin;
		public Vector2 ShowMax;

		[Header("Lower Left : Upper Right")]
		public Vector2 HideMin;
		public Vector2 HideMax;

		[ContextMenu("RecordShowOffset")]
		public void RecordShowOffset () {
			if (Rect == null) Rect = GetComponent<RectTransform>();
			ShowMin = Rect.offsetMin;
			ShowMax = Rect.offsetMax;
		}

		[ContextMenu("RecordHideOffset")]
		public void RecordHideOffset () {
			if (Rect == null) Rect = GetComponent<RectTransform>();
			HideMin = Rect.offsetMin;
			HideMax = Rect.offsetMax;
		}

		void OnValidate () {
			Rect = GetComponent<RectTransform>();
		}

		public override bool Shown () {
			return Rect.offsetMin == ShowMin && Rect.offsetMax != ShowMax;
		}

		public override bool Hided () {
			return Rect.offsetMin == HideMin && Rect.offsetMax != HideMax;
		}

		public override void ForceShow () {
			Rect.offsetMin = ShowMin;
			Rect.offsetMax = ShowMax;
		}

		public override void ForceHide () {
			Rect.offsetMin = HideMin;
			Rect.offsetMax = HideMax;
		}

		Vector2 startMin, startMax, endMin, endMax;

		public override void PrepareShow () {
			startMin = Rect.offsetMin;
			startMax = Rect.offsetMax;
			endMin = ShowMin;
			endMax = ShowMax;
		}

		public override void PrepareHide () {
			startMin = Rect.offsetMin;
			startMax = Rect.offsetMax;
			endMin = HideMin;
			endMax = HideMax;
		}

		public override void ApplyTransition (float step) {
			Rect.offsetMin = Vector2.Lerp(startMin, endMin, step);
			Rect.offsetMax = Vector2.Lerp(startMax, endMax, step);
		}
	}
}