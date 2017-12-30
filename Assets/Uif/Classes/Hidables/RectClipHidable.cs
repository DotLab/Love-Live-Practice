using UnityEngine;

namespace Uif {
	[AddComponentMenu("Uif/Hidable/Rect Clip Hidable")]
	public class RectClipHidable : EasedHidable {
		public RectTransform Rect;

		public bool LockWidth;
		public float ShowWidth;
		public float HideHeight;

		public bool LockHeight;
		public float ShowHeight;
		public float HideWidth;


		void OnValidate () {
			Rect = GetComponent<RectTransform>();
		}

		public override bool Shown () {
			return Rect.sizeDelta == new Vector2(ShowWidth, ShowHeight);
		}

		public override bool Hided () {
			return Rect.sizeDelta == new Vector2(HideHeight, HideWidth);
		}

		public override void ForceShow () {
			Rect.sizeDelta = new Vector2(
				LockWidth ? Rect.sizeDelta.x : ShowWidth,
				LockHeight ? Rect.sizeDelta.y : ShowHeight);
		}

		public override void ForceHide () {
			Rect.sizeDelta = new Vector2(
				LockWidth ? Rect.sizeDelta.x : HideWidth, 
				LockHeight ? Rect.sizeDelta.y : HideHeight);
		}

		Vector2 startSize, endSize;

		public override void PrepareShow () {
			startSize = Rect.sizeDelta;
			endSize = new Vector2(
				LockWidth ? Rect.sizeDelta.x : ShowWidth,
				LockHeight ? Rect.sizeDelta.y : ShowHeight);
		}

		public override void PrepareHide () {
			startSize = Rect.sizeDelta;
			endSize = new Vector2(
				LockWidth ? Rect.sizeDelta.x : HideWidth, 
				LockHeight ? Rect.sizeDelta.y : HideHeight);
		}

		public override void ApplyTransition (float step) {
			Rect.sizeDelta = Vector2.Lerp(startSize, endSize, step);
		}
	}
}