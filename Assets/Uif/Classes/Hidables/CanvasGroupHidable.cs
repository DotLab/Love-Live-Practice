using UnityEngine;

namespace Uif {
	[AddComponentMenu("Uif/Hidable/CanvasGroup Hidable")]
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupHidable : EasedHidable {
		[Range(0, 1)]
		public float ShowAlpha = 1;
		[Range(0, 1)]
		public float HideAlpha;

		[Space]
		public CanvasGroup group;


		public void OnValidate() {
			group = GetComponent<CanvasGroup>();
		}

		public override bool Shown() {
			return group.alpha == ShowAlpha;
		}

		public override bool Hided() {
			return group.alpha == HideAlpha;
		}

		public override void ForceShow() {
			group.alpha = ShowAlpha;
		}

		public override void ForceHide() {
			group.alpha = HideAlpha;
		}

		float srcAlpha, dstAlpha;

		public override void PrepareShow() {
			srcAlpha = group.alpha;
			dstAlpha = ShowAlpha;
		}

		public override void PrepareHide() {
			srcAlpha = group.alpha;
			dstAlpha = HideAlpha;
		}

		public override void ApplyTransition(float step) {
			group.alpha = srcAlpha + (dstAlpha - srcAlpha) * step;
		}
	}
}