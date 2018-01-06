using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayPanel : MonoBehaviour {
	public GameObject ButtonPrototype;

	public Vector2 ButtonSize = new Vector2(200, 200);
	public int ButtonCount = 9;

	public float semiCircleHeight, semiCircleAngleSkip;

	public Vector2[] buttonPositions;
	public PlayPanelButton[] buttons;
	public RectTransform rectTransform;

	public bool Toggle;

	public void OnValidate() {
		rectTransform = GetComponent<RectTransform>();

		if (buttons.Length != ButtonCount) return;

		RelocateButtons();
	}

#if UNITY_EDITOR
	public void OnRectTransformDimensionsChange() {
		RelocateButtons();
	}
#endif

	public void RelocateButtons() {
		Debug.Log("Relocate buttons");

		if (rectTransform.rect.height * 2 > rectTransform.rect.width) {  // Use width / 2
			semiCircleHeight = rectTransform.rect.width / 2;
		} else {  // Use height
			semiCircleHeight = rectTransform.rect.height;
		}

		semiCircleAngleSkip = Mathf.PI / (ButtonCount - 1);

		buttonPositions = new Vector2[ButtonCount];
		for (int i = 0; i < ButtonCount; i++) {
			buttonPositions[i] = semiCircleHeight * new Vector2(Mathf.Cos(Mathf.PI + semiCircleAngleSkip * i), Mathf.Sin(Mathf.PI + semiCircleAngleSkip * i));
			buttons[i].Position = buttonPositions[i];
			buttons[i].Size = ButtonSize;
			buttons[i].particleSystem.transform.rotation = Quaternion.Euler(0, 0, (semiCircleAngleSkip * i - Mathf.PI / 2) * Mathf.Rad2Deg);
		}
	}
}
