using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayPanelButton : MonoBehaviour {
	public Vector2 Size {
		get { return rectTrans.sizeDelta; }
		set { rectTrans.sizeDelta = value; }
	}

	public Vector2 Position {
		get { return rectTrans.anchoredPosition; }
		set { rectTrans.anchoredPosition = value; }
	}

	public RectTransform rectTrans;
	public Image circleUiImage;
	new public ParticleSystem particleSystem;
}
