using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoveLivePractice.Api;
using Uif;

public class LiveMapPreviewPanel : MonoBehaviour {
	[System.Serializable]
	public class MapPreviewPanelNote {
		public RectTransform RectTrans;
		public Colorable Colorable;
		public LiveMapNote Note;
		public bool FlashStart;
	}

	[System.Serializable]
	public class MapPreviewPanelFlash {
		public RectTransform RectTrans;
		public Colorable Colorable;
		public EasedHidable Hidable;
		public AudioSource Source;
	}

	public AudioClip HitClip;

	public LiveMapNote[] MapNotes;
	public Color[] Colors;

	public GameObject NotePrototype, FlashPrototype;
	public int LaneCount = 9;

	public EasingType EasingType = EasingType.Cubic;
	public EasingPhase EasingPhase = EasingPhase.Out;

	public float FlashWidth = 300, NoteWidth = 5, LaneSpacing = 0, CacheTime = 1;
	public float MapOffset;

	public MapPreviewPanelFlash[] flashes;

	public RectTransform rectTrans;

	public double dspStartTime, lastTime, dspLastTime;

	public int index;
	public float panelHeight, panelWidth, laneHeight, laneSkip;
	public bool isPlaying;

	LinkedList<MapPreviewPanelNote> freeNodeList = new LinkedList<MapPreviewPanelNote>(), activeNoteList = new LinkedList<MapPreviewPanelNote>();

	public void OnValidate() {
		rectTrans = GetComponent<RectTransform>();
	}

	public void Start() {
		panelHeight = rectTrans.rect.height;
		panelWidth = rectTrans.rect.width;
		laneHeight = (panelHeight + LaneSpacing) / LaneCount - LaneSpacing;
		laneSkip = -(laneHeight + LaneSpacing);
		
		flashes = new MapPreviewPanelFlash[LaneCount];
		for (int i = 0; i < LaneCount; i++) {
			var flashGo = Instantiate(FlashPrototype, rectTrans);

			flashes[i] = new MapPreviewPanelFlash { 
				RectTrans = flashGo.GetComponent<RectTransform>(),
				Hidable = flashGo.GetComponent<EasedHidable>(),
				Colorable = flashGo.GetComponent<Colorable>(),
				Source = flashGo.GetComponent<AudioSource>(),
			};

			flashes[i].RectTrans.anchoredPosition = new Vector2(0, laneSkip * i);
			flashes[i].RectTrans.sizeDelta = new Vector2(FlashWidth, laneHeight);
		}
	}

	public void Init(LiveMapNote[] notes) {
		MapNotes = notes;

		index = 0;
	}

	public void Play() {
		MusicPlayer.Instance.Play();

		dspLastTime = dspStartTime = AudioSettings.dspTime;
		lastTime = Time.unscaledTime;

		isPlaying = true;
	}

	public void Stop() {
		isPlaying = false;

		var node = activeNoteList.First;
		while (node != null) {
			var next = node.Next;
			node.Value.RectTrans.anchoredPosition = new Vector2(1000, 0);
			freeNodeList.AddLast(node.Value);
			activeNoteList.Remove(node);
			node = next;
		}
	}

	public void Update() {
		if (!isPlaying) return;

		if (!MusicPlayer.Instance.IsPlaying()) {
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

		time += MapOffset;

		double cacheTime = time + CacheTime;

		while (index < MapNotes.Length && MapNotes[index].starttime <= cacheTime) {
			var note = MapNotes[index];
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

			float stepStart = Easing.Ease(EasingType, EasingPhase, note.Note.starttime - time, CacheTime);
			float stepEnd = Easing.Ease(EasingType, EasingPhase, note.Note.endtime - time, CacheTime);

			note.RectTrans.anchoredPosition = new Vector2(stepStart * panelWidth, laneSkip * note.Note.lane);
			if (note.Note.longnote) note.RectTrans.sizeDelta = new Vector2((stepEnd - stepStart) * panelWidth, laneHeight);

			if (note.Note.longnote && !note.FlashStart && time > note.Note.starttime) {
				note.FlashStart = true;
				Flash(note.Note.lane, note.Colorable.GetColor());	
			}

			if (time > note.Note.endtime) {
				freeNodeList.AddLast(note);
				activeNoteList.Remove(node);
				Flash(note.Note.lane, note.Colorable.GetColor());
				note.RectTrans.anchoredPosition = new Vector2(note.RectTrans.anchoredPosition.x - 100, 0);
			}

			node = nextNode;
		}
	}

	void Flash(int lane, Color color) {
		flashes[lane].Source.Play();
		flashes[lane].Colorable.SetColor(color);
		flashes[lane].Hidable.ForceShow();
		flashes[lane].Hidable.Hide();
	}

	float colorIndex;
	void InitNote(MapPreviewPanelNote note) {
		note.FlashStart = false;
		note.RectTrans.SetAsFirstSibling();
		note.RectTrans.sizeDelta = new Vector2(NoteWidth, laneHeight);
		note.Colorable.SetColor(Colors[(int)(colorIndex += 0.1f) % Colors.Length]);
	}
}
