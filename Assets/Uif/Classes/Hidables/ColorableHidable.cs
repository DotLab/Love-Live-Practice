using UnityEngine;

namespace Uif {
	[AddComponentMenu("Uif/Hidable/Colorable Hidable")]
	[RequireComponent(typeof(Colorable))]
	public class ColorableHidable : EasedHidable {
		public Colorable Colorable;

		[Space]
		[Range(0, 1)]
		public float ShowAlpha = 1;
		[Range(0, 1)]
		public float HideAlpha;


		void OnValidate () {
			Colorable = GetComponent<Colorable>();
		}

		public override bool Shown () {
			return Colorable.GetColor().a == ShowAlpha;
		}

		public override bool Hided () {
			return Colorable.GetColor().a == HideAlpha;
		}

		public override void ForceShow () {
			srcColor = Colorable.GetColor();
			Colorable.SetColor(new Color(srcColor.r, srcColor.g, srcColor.b, ShowAlpha));
		}

		public override void ForceHide () {
			srcColor = Colorable.GetColor();
			Colorable.SetColor(new Color(srcColor.r, srcColor.g, srcColor.b, HideAlpha));
		}

		Color srcColor, dstColor;

		public override void PrepareShow () {
			srcColor = Colorable.GetColor();
			dstColor = new Color(srcColor.r, srcColor.g, srcColor.b, ShowAlpha);
		}

		public override void PrepareHide () {
			srcColor = Colorable.GetColor();
			dstColor = new Color(srcColor.r, srcColor.g, srcColor.b, HideAlpha);
		}

		public override void ApplyTransition (float step) {
			Colorable.SetColor(Color.Lerp(srcColor, dstColor, step));
		}
	}
}