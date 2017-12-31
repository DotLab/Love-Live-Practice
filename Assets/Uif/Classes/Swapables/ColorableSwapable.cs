using UnityEngine;
using System.Collections;

namespace Uif {
	[AddComponentMenu("Uif/Swapable/Colorable Swapable")]
	[RequireComponent(typeof(Colorable))]
	public class ColorableSwapable : ColorSwapable {
		public Colorable colorable;

		Color lastColor;

		public void OnValidate() {
			colorable = GetComponent<Colorable>();
		}

		public override void Swap(Color newColor) {
			if (colorable.GetColor() == newColor && lastColor == newColor) return;
			lastColor = newColor;

			StopAllCoroutines();
			StartCoroutine(SwapHandler(colorable.GetColor(), newColor));
		}

		public override void ForceSwap(Color newColor) {
			colorable.SetColor(newColor);
		}

		IEnumerator SwapHandler(Color srcColor, Color dstColor) {
			float time = 0;

			while (time < TransitionDuration) {
				var easedStep = Easing.Ease(TransitionEasingType, TransitionEasingPhase, time, TransitionDuration);

				colorable.SetColor(Color.Lerp(srcColor, dstColor, easedStep));

				time += Time.deltaTime;
				yield return null;
			}

			colorable.SetColor(dstColor);
		}
	}
}