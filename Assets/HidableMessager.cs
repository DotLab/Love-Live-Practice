using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidableMessager : MonoBehaviour {
	public bool Hide, Show;

	public void Update() {
		if (Hide) {
			Hide = false;
			SendMessage("Hide");
		}

		if (Show) {
			Show = false;
			SendMessage("Show");
		}
	}
}
