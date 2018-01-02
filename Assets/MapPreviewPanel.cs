using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoveLivePractice.Api;
using Uif;

public class MapPreviewPanel : MonoBehaviour {
	[System.Serializable]
	public class MapPreviewPanelNote {
		public RectTransform RectTrans;
		public Colorable Colorable;
		public ApiMapNote Note;
	}

	public ApiLiveMap LiveMap;
	public AudioSource Source;
	public Color[] Colors;

	public GameObject NotePrototype;
	public int LaneCount = 9;

	public EasingType EasingType = EasingType.Cubic;
	public EasingPhase EasingPhase = EasingPhase.Out;

	public float NoteWidth = 5, LaneSpacing = 0, CacheTime = 1;

	public RectTransform rectTrans;

	public double dspStartTime, lastTime, dspLastTime;

	public int index;
	public float panelHeight, panelWidth, laneHeight, laneSkip;
	public bool isPlaying;

	LinkedList<MapPreviewPanelNote> freeNodeList = new LinkedList<MapPreviewPanelNote>(), activeNoteList = new LinkedList<MapPreviewPanelNote>();

	public void OnValidate() {
		rectTrans = GetComponent<RectTransform>();
	}

	public void Init(ApiLiveMap liveMap, AudioSource source) {
		LiveMap = liveMap;
		Source = source;

		index = 0;
		panelHeight = rectTrans.rect.height;
		panelWidth = rectTrans.rect.width;
		laneHeight = (panelHeight + LaneSpacing) / LaneCount - LaneSpacing;
		laneSkip = -(laneHeight + LaneSpacing);
	}

	public void Play() {
		Source.Stop();
		Source.Play();

		dspLastTime = dspStartTime = AudioSettings.dspTime;
		lastTime = Time.unscaledTime;

		isPlaying = true;
	}

	public void Update() {
		if (!isPlaying) return;

		if (!Source.isPlaying) {
			isPlaying = false;
			return;
		}

		double dspTime = AudioSettings.dspTime, time;

		if (dspLastTime != dspTime) {
			dspLastTime = dspTime;
			lastTime = Time.unscaledTime;
			time = dspTime - dspStartTime;
		} else {
			time = dspTime - dspStartTime + (Time.unscaledTime - lastTime);
		}

		double cacheTime = time + CacheTime;

		var lane = LiveMap.lane;
		while (index < lane.Length && lane[index].starttime <= cacheTime) {
			var note = lane[index];
			MapPreviewPanelNote panelNote;

			// Show lane[mapNoteIndex]
			if (freeNodeList.Count <= 0) {  // Instantiate
				var noteGo = Instantiate(NotePrototype, rectTrans);
				panelNote = new MapPreviewPanelNote { 
					RectTrans = noteGo.GetComponent<RectTransform>(),
					Colorable = noteGo.GetComponent<Colorable>(),
					Note = note,
				};
			} else {  // Reuse
				panelNote = freeNodeList.First.Value;
				panelNote.Note = note;
				freeNodeList.RemoveFirst();
			}

			InitNote(panelNote);
			activeNoteList.AddLast(panelNote);

			index += 1;
		}

		var node = activeNoteList.First;
		while (node != null) {
			var nextNode = node.Next;
			var note = node.Value;

			note.RectTrans.anchoredPosition = new Vector2((float)(note.Note.starttime - time) / CacheTime * panelWidth, laneSkip * note.Note.lane);
			if (time - 0.1f > note.Note.endtime) {
				freeNodeList.AddLast(note);
				activeNoteList.Remove(node);
			}

			node = nextNode;
		}
	}

	float colorIndex;

	void InitNote(MapPreviewPanelNote note) {
		note.RectTrans.sizeDelta = note.Note.longnote ? 
			new Vector2((float)(note.Note.endtime - note.Note.starttime) / CacheTime * panelWidth, laneHeight) : 
			new Vector2(NoteWidth, laneHeight);
		note.Colorable.SetColor(Colors[(int)(colorIndex += 0.1f) % Colors.Length]);
//		note.RectTrans.anchoredPosition = new Vector2(panelWidth, laneSkip * note.Note.lane);
	}
}
