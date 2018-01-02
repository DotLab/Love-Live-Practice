using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour {
	public Text uiText;

	public void OnValidate() {
		if (uiText == null) uiText = GetComponent<Text>();
	}

	public void Update() {
		uiText.text = (1.0f / Time.deltaTime).ToString("N1");
	}
}
