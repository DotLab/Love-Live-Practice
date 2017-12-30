using UnityEngine;
using UnityEngine.UI;

namespace Uif {
	[AddComponentMenu("Uif/Colorable/Graphic Colorable")]
	[RequireComponent(typeof(Graphic))]
	public class GraphicColorable : Colorable {
		public Graphic Graphic;

		void OnValidate () {
			Graphic = GetComponent<Graphic>();
		}

		public override Color GetColor () {
			return Graphic.color;
		}

		public override void SetColor (Color newColor) {
			Graphic.color = newColor;
		}
	}
}