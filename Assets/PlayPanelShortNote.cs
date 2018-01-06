using UnityEngine;

public class PlayPanelShortNote : PlayPanelNote {
	public void UpdateNote(Vector2 position, float size) {
		rectTrans.anchoredPosition = position;
		rectTrans.sizeDelta = new Vector2(size, size);
	}
}