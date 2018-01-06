using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
	public float Period = 0.6f;

	// Update is called once per frame
	void Update() {
		transform.Rotate(0, 0, 360f / Period * Time.deltaTime);
	}
}
