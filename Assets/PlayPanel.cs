using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Uif;

[ExecuteInEditMode]
public class PlayPanel : MonoBehaviour {
	public GameObject ButtonPrototype, ShortNotePrototype, LongNotePrototype;

	public float ButtonSize = 200;
	public int ButtonCount = 9;

	public EasingType CacheEasingType = EasingType.Cubic;
	public EasingPhase CacheEasingPhase = EasingPhase.Out;
	public float CacheTime = 2;

	public EasingType OutEasingType = EasingType.Cubic;
	public EasingPhase OutEasingPhase = EasingPhase.Out;
	public float OutDistance = 200;
	public Vector2 Center;
	public float StartSize, HitSize = 200, OutSize = 200;

	public float PerfectTime = 0.05f, GreatTime = 0.10f, GoodTime = 0.20f, BadTime = 0.40f;

	public LiveMapNote[] MapNotes;
	public float MapOffset;

	public RectTransform notePanelRectTrans;

	public float outTime;
	public double time, dspStartTime, lastTime, dspLastTime;
	public int index;
	public bool isPlaying;

	public float radius, angleSkip;

	public Vector2[] buttonPositions, outPositions;
	public PlayPanelButton[] buttons;
	public RectTransform rectTrans;

	public LinkedList<PlayPanelShortNote> freeShortNoteList = new LinkedList<PlayPanelShortNote>(), activeShortNoteList = new LinkedList<PlayPanelShortNote>();
	public LinkedList<PlayPanelLongNote> freeLongNoteList = new LinkedList<PlayPanelLongNote>(), activeLongNoteList = new LinkedList<PlayPanelLongNote>();

	public bool Toggle;

	public void OnValidate() {
		rectTrans = GetComponent<RectTransform>();

		if (buttons.Length != ButtonCount) return;

		Start();
	}

#if UNITY_EDITOR
	public void OnRectTransformDimensionsChange() {
		Start();
	}
#endif

	public void Start() {
		Debug.Log("Relocate buttons");
		if (rectTrans.rect.height * 2 > rectTrans.rect.width) {  // Use width / 2
			radius = rectTrans.rect.width / 2;
		} else {  // Use height
			radius = rectTrans.rect.height;
		}

		angleSkip = Mathf.PI / (ButtonCount - 1);

		buttonPositions = new Vector2[ButtonCount];
		outPositions = new Vector2[ButtonCount];
		for (int i = 0; i < ButtonCount; i++) {
			buttonPositions[i] = radius * new Vector2(Mathf.Cos(Mathf.PI + angleSkip * i), Mathf.Sin(Mathf.PI + angleSkip * i));
			outPositions[i] = (radius + OutDistance) * new Vector2(Mathf.Cos(Mathf.PI + angleSkip * i), Mathf.Sin(Mathf.PI + angleSkip * i));

			buttons[i].Position = buttonPositions[i];
			buttons[i].Size = new Vector2(ButtonSize, ButtonSize);
			buttons[i].particleSystem.transform.rotation = Quaternion.Euler(0, 0, (angleSkip * i - Mathf.PI / 2) * Mathf.Rad2Deg);
		}

		outTime = (OutDistance / radius) * CacheTime;
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

	public void Update() {
		if (!isPlaying) return;

		if (!MusicPlayer.Instance.IsPlaying()) {
			isPlaying = false;
			return;
		}

		double dspTime = AudioSettings.dspTime;

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
			if (note.longnote) {
				PlayPanelLongNote longNote;
				if (freeLongNoteList.Count <= 0) {  // Instantiate
					var noteGo = Instantiate(LongNotePrototype, notePanelRectTrans);
					longNote = noteGo.GetComponent<PlayPanelLongNote>();
				} else {  // Reuse
					longNote = freeLongNoteList.First.Value;
					freeLongNoteList.RemoveFirst();
				}

				longNote.Init(note, (angleSkip * note.lane - Mathf.PI / 2) * Mathf.Rad2Deg);
				activeLongNoteList.AddLast(longNote);
			} else {
				PlayPanelShortNote shortNote;
				if (freeShortNoteList.Count <= 0) {  // Instantiate
					var noteGo = Instantiate(ShortNotePrototype, notePanelRectTrans);
					shortNote = noteGo.GetComponent<PlayPanelShortNote>();
				} else {  // Reuse
					shortNote = freeShortNoteList.First.Value;
					freeShortNoteList.RemoveFirst();
				}

				shortNote.Init(note);
				activeShortNoteList.AddLast(shortNote);
			}

			index += 1;
		}

		foreach (var touch in Input.touches) {
			var touchDown = touch.phase == TouchPhase.Began;
			var touchUp = touch.phase == TouchPhase.Ended;

			var direction = (Vector2)(Camera.main.ScreenToWorldPoint(touch.position) - notePanelRectTrans.position);
			var angle = (Mathf.Atan2(direction.y, direction.x) + Mathf.PI + angleSkip / 2) % (Mathf.PI * 2);
			ProcessTouch((int)(angle / angleSkip), touchDown, touchUp);
		}

#if UNITY_EDITOR
		var mousePress = Input.GetMouseButton(0);
		var mouseDown = Input.GetMouseButtonDown(0);
		var mouseUp = Input.GetMouseButtonUp(0);

		if (mousePress || mouseDown || mouseUp) {
			var direction = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - notePanelRectTrans.position);
			var angle = (Mathf.Atan2(direction.y, direction.x) + Mathf.PI + angleSkip / 2) % (Mathf.PI * 2);
			ProcessTouch((int)(angle / angleSkip), mouseDown, mouseUp);
		}	
#endif

		var shortNode = activeShortNoteList.First;
		while (shortNode != null) {  // Update ShortNotes
			var nextNode = shortNode.Next;
			var note = shortNode.Value;

			if (note.Free) {
				freeShortNoteList.AddLast(note);
				activeShortNoteList.Remove(shortNode); 
				shortNode = nextNode;
				continue;
			}

			float stepStart = Easing.Ease(CacheEasingType, CacheEasingPhase, time + CacheTime - note.Note.starttime, CacheTime);
			float stepOutStart = Easing.Ease(OutEasingType, OutEasingPhase, time - note.Note.starttime, outTime);

			Vector2 startPosition;
			float startSize;
			if (note.Note.starttime > time) {  // Cache
				startPosition = Vector2.LerpUnclamped(Center, buttonPositions[note.Note.lane], stepStart);
				startSize = Mathf.LerpUnclamped(StartSize, HitSize, stepStart);
			} else if (note.Note.starttime > time - outTime) {  // Out
				startPosition = Vector2.LerpUnclamped(buttonPositions[note.Note.lane], outPositions[note.Note.lane], stepOutStart);
				startSize = Mathf.LerpUnclamped(HitSize, OutSize, stepOutStart);
			} else {  // Beyond Out => Recycle
				startPosition = outPositions[note.Note.lane];
				startSize = OutSize;

				note.Recycle();

				freeShortNoteList.AddLast(note);
				activeShortNoteList.Remove(shortNode);
			}

			note.UpdateNote(startPosition, startSize);
				
			shortNode = nextNode;
		}

		var longNode = activeLongNoteList.First;
		while (longNode != null) {  // Update LongNotes
			var nextNode = longNode.Next;
			var note = longNode.Value;

			if (note.Free) {
				freeLongNoteList.AddLast(note);
				activeLongNoteList.Remove(longNode); 
				longNode = nextNode;
				continue;
			}

			float stepStart = Easing.Ease(CacheEasingType, CacheEasingPhase, time + CacheTime - note.Note.starttime, CacheTime);
			float stepOutStart = Easing.Ease(OutEasingType, OutEasingPhase, time - note.Note.starttime, outTime);

			float stepEnd = Easing.Ease(CacheEasingType, CacheEasingPhase, time + CacheTime - note.Note.endtime, CacheTime);
			float stepOutEnd = Easing.Ease(OutEasingType, OutEasingPhase, time - note.Note.endtime, outTime);

			Vector2 startPosition;
			float startSize;
			if (note.Holding) {
				startPosition = buttonPositions[note.Note.lane];
				startSize = ButtonSize;
			} else if (note.Note.starttime > time) {  // Cache
				startPosition = Vector2.LerpUnclamped(Center, buttonPositions[note.Note.lane], stepStart);
				startSize = Mathf.LerpUnclamped(StartSize, HitSize, stepStart);
			} else if (note.Note.starttime > time - outTime) {  // Out
				startPosition = Vector2.LerpUnclamped(buttonPositions[note.Note.lane], outPositions[note.Note.lane], stepOutStart);
				startSize = Mathf.LerpUnclamped(HitSize, OutSize, stepOutStart);
			} else {  // Beyond Out
				startPosition = outPositions[note.Note.lane];
				startSize = OutSize;
			}

			Vector2 endPosition;
			float endSize;
			if (note.Note.endtime > time + CacheTime) {  // Pre Cache
				endPosition = Center;
				endSize = StartSize;
			} else if (note.Note.endtime > time) {  // Cache
				endPosition = Vector2.LerpUnclamped(Center, buttonPositions[note.Note.lane], stepEnd);
				endSize = Mathf.LerpUnclamped(StartSize, HitSize, stepEnd);
			} else if (note.Note.endtime > time - outTime) {  // Out
				if (note.Holding) {
					startPosition = outPositions[note.Note.lane];
					startSize = 1;
					endPosition = outPositions[note.Note.lane];
					endSize = 1;

					note.Recycle();
					freeLongNoteList.AddLast(note);
					activeLongNoteList.Remove(longNode);
				} else {
					endPosition = Vector2.LerpUnclamped(buttonPositions[note.Note.lane], outPositions[note.Note.lane], stepOutEnd);
					endSize = Mathf.LerpUnclamped(HitSize, OutSize, stepOutEnd);
				}
			} else {  // Beyond Out => Recycle
				endPosition = outPositions[note.Note.lane];
				endSize = OutSize;

				note.Recycle();

				freeLongNoteList.AddLast(note);
				activeLongNoteList.Remove(longNode); 
			}

			note.UpdateNote(startPosition, endPosition, startSize, endSize);

			longNode = nextNode;
		}
	}

	public void ProcessTouch(int lane, bool down, bool up) {
		if (lane < 0 || lane >= ButtonCount) return;

//		Debug.Log("Touch on " + lane);

		PlayPanelLongNote longCandidate = null;
		foreach (var note in activeLongNoteList) {
			if (note.Note.lane != lane) continue;

			if (note.Holding) {
				if (up) {
					note.UpdateNote(outPositions[lane], outPositions[lane], 1, 1);
					note.Recycle();
				} else {
					note.HoldTime = time;
				}

				return;
			} else if (longCandidate == null || System.Math.Abs(note.Note.starttime - time) < System.Math.Abs(longCandidate.Note.starttime - time)) {
				longCandidate = note;
			}
		}

		PlayPanelShortNote shortCandidate = null;
		foreach (var note in activeShortNoteList) {
			if (note.Note.lane != lane) continue;

			if (shortCandidate == null || System.Math.Abs(note.Note.starttime - time) < System.Math.Abs(shortCandidate.Note.starttime - time)) {
//				Debug.Log(note.Note.starttime);
				shortCandidate = note;
			}
		}

		if (longCandidate == null && shortCandidate == null) return;

		if (longCandidate != null && (shortCandidate == null || System.Math.Abs(longCandidate.Note.starttime - time) < System.Math.Abs(shortCandidate.Note.starttime - time))) {  // Process longCandidate
			if (down) {
				longCandidate.Holding = true;
				longCandidate.HoldTime = time;
			}
		} else {  // Process shortCandidate
			if (down) {
				shortCandidate.UpdateNote(outPositions[lane], 1);
				shortCandidate.Recycle();
			}
		}
	}
}
