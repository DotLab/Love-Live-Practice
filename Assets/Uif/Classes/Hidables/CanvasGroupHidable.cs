using UnityEngine;

namespace Uif {
	[AddComponentMenu("Uif/Hidable/CanvasGroup Hidable")]
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupHidable : EasedHidable {
		public CanvasGroup Group;

		[Range(0, 1)]
		public float ShowAlpha = 1;
		[Range(0, 1)]
		public float HideAlpha;


		void OnValidate () {
			Group = GetComponent<CanvasGroup>();
		}

		public override bool Shown () {
			return Group.alpha == ShowAlpha;
		}

		public override bool Hided () {
			return Group.alpha == HideAlpha;
		}

		public override void ForceShow () {
			Group.alpha = ShowAlpha;
		}

		public override void ForceHide () {
			Group.alpha = HideAlpha;
		}

		float srcAlpha, dstAlpha;

		public override void PrepareShow () {
			srcAlpha = Group.alpha;
			dstAlpha = ShowAlpha;
		}

		public override void PrepareHide () {
			srcAlpha = Group.alpha;
			dstAlpha = HideAlpha;
		}

		public override void ApplyTransition (float step) {
			Group.alpha = srcAlpha + (dstAlpha - srcAlpha) * step;
		}
	}
}