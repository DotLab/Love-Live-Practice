using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeControl : MonoBehaviour {
	//	public static event System.Action<SwipeDirection> OnSwipe;
	public static SwipeDirection Swipe;

	public enum SwipeDirection {
		None,
		Up,
		Down,
		Left,
		Right,
	}

	//First touch position
	public Vector3 beginPosition, endPosition;
	//minimum distance for a swipe to be registered
	public float dragTimeout = 0.2f, dragDistance;

	public float dragStartTime;

	void Start() {
		dragDistance = Screen.height * 0.2f; //dragDistance is 15% height of the screen
	}

	void Update() {
		Swipe = SwipeDirection.None;

#if UNITY_STANDALONE || UNITY_EDITOR
		if (Input.GetMouseButtonDown(0)) StartDrag(Input.mousePosition);
		else if (Input.GetMouseButtonUp(0)) EndDrag(Input.mousePosition);
#endif

		if (Input.touchCount != 1) return; // user is touching the screen with a single touch
		Touch touch = Input.GetTouch(0); // get the touch

		if (touch.phase == TouchPhase.Began) StartDrag(touch.position);
		else if (touch.phase == TouchPhase.Ended) EndDrag(touch.position);
		
	}

	void StartDrag(Vector3 position) {
		beginPosition = position;

		dragStartTime = Time.time;
	}

	void EndDrag(Vector3 position) {
		endPosition = position;  //last touch position. Ommitted if you use list

		if (Time.time - dragStartTime > dragTimeout) return;

		//Check if drag distance is greater than 20% of the screen height
		if (Mathf.Abs(endPosition.x - beginPosition.x) > dragDistance || Mathf.Abs(endPosition.y - beginPosition.y) > dragDistance) {//It's a drag
			//check if the drag is vertical or horizontal
			if (Mathf.Abs(endPosition.x - beginPosition.x) > Mathf.Abs(endPosition.y - beginPosition.y)) {   //If the horizontal movement is greater than the vertical movement...
				if ((endPosition.x > beginPosition.x)) {  //If the movement was to the right)//Right swipe
//					Debug.Log("Right Swipe");
					Swipe = SwipeDirection.Right;
				} else {   //Left swipe
//					Debug.Log("Left Swipe");
					Swipe = SwipeDirection.Left;
				}
			} else {   //the vertical movement is greater than the horizontal movement
				if (endPosition.y > beginPosition.y) {  //If the movement was up//Up swipe
//					Debug.Log("Up Swipe");
					Swipe = SwipeDirection.Up;
				} else {   //Down swipe
//					Debug.Log("Down Swipe");
					Swipe = SwipeDirection.Down;
				}
			}
		}
	}
}
