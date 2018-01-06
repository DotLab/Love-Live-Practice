using UnityEngine;

public class PlayPanelLongNote : PlayPanelNote {
	public bool Holding;
	public double HoldTime;

	public RectTransform trailRectTrans, endCapRectTrans;
	public UnityEngine.UI.LineImage lineImage;

	public void Init(LiveMapNote note, float angle) {
		base.Init(note);
	
		Holding = false;
		trailRectTrans.rotation = Quaternion.Euler(0, 0, angle);
	}

	public void UpdateNote(Vector2 startPosition, Vector2 endPosition, float startSize, float endSize) {
		rectTrans.anchoredPosition = startPosition;
		rectTrans.sizeDelta = new Vector2(startSize, startSize);

		trailRectTrans.sizeDelta = new Vector2(trailRectTrans.sizeDelta.x, Vector2.Distance(startPosition, endPosition));

		endCapRectTrans.sizeDelta = new Vector2(endSize, endSize);

		lineImage.StartOffset = (startSize - endSize) / 2;
	}
}