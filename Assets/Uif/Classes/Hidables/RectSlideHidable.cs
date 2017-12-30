using UnityEngine;

namespace Uif {
	[AddComponentMenu("Uif/Hidable/Rect Slide Hidable")]
	public class RectSlideHidable : EasedHidable {
		public RectTransform Rect;

		[Space]
		public bool LockX;
		public float ShowX;
		public float HideX;

		[Space]
		public bool LockY;
		public float ShowY;
		public float HideY;

		void OnValidate () {
			Rect = GetComponent<RectTransform>();
		}

		public override bool Shown () {
			return Rect.anchoredPosition == new Vector2(ShowX, ShowY);
		}

		public override bool Hided () {
			return Rect.anchoredPosition == new Vector2(HideX, HideY);
		}

		public override void ForceShow () {
			Rect.anchoredPosition = new Vector2(
				LockX ? Rect.anchoredPosition.x : ShowX,
				LockY ? Rect.anchoredPosition.y : ShowY);
		}

		public override void ForceHide () {
			Rect.anchoredPosition = new Vector2(
				LockX ? Rect.anchoredPosition.x : HideX, 
				LockY ? Rect.anchoredPosition.y : HideY);
		}

		Vector2 startPosition, endPosition;

		public override void PrepareShow () {
			startPosition = Rect.anchoredPosition;
			endPosition = new Vector2(
				LockX ? Rect.anchoredPosition.x : ShowX,
				LockY ? Rect.anchoredPosition.y : ShowY);
		}

		public override void PrepareHide () {
			startPosition = Rect.anchoredPosition;
			endPosition = new Vector2(
				LockX ? Rect.anchoredPosition.x : HideX, 
				LockY ? Rect.anchoredPosition.y : HideY);
		}

		public override void ApplyTransition (float step) {
			Rect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, step);
		}
	}
}