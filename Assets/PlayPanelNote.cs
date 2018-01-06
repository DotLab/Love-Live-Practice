using UnityEngine;

public class PlayPanelNote : MonoBehaviour {
	public LiveMapNote Note;

	public bool Free;

	public GameObject paraGo;
	public RectTransform rectTrans;

	public void OnValidate() {
		rectTrans = GetComponent<RectTransform>();
	}

	public virtual void Init(LiveMapNote note) {
		Free = false;

		Note = note;

		paraGo.SetActive(note.parallel);

		rectTrans.SetAsFirstSibling();
	}

	public void Recycle() {
		Free = true;
	}
}
