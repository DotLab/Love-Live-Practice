using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

public class PlayScheduler : MonoBehaviour {
	public RawImage bgUiRawImage;
	public AspectRatioFitter bgFitter;
	//	public EasedHidable bgOverlayHidable;

	public PlayPanel playPanel;

	public void Start() {
		StartCoroutine(StartHandler());
	}

	IEnumerator StartHandler() {
		if (Game.ActiveLive == null) {
			Game.LoadGameData();
			Game.ActiveLive = Game.ActiveLives[5];
			var coverJob = LocalStorage.LoadTexture(Game.ActiveLive.coverPath);
			var bgmJob = LocalStorage.LoadAudioClip(Game.ActiveLive.bgmPath);
			
			while (!coverJob.IsFinished()) yield return null;
			while (!bgmJob.IsFinished()) yield return null;
			
			Game.ActiveLive.texture = coverJob.GetData();
			Game.ActiveLive.clip = bgmJob.GetData();
		}
#if UNITY_EDITOR
#endif

		bgUiRawImage.texture = Game.ActiveLive.texture;
		bgFitter.aspectRatio = (float)Game.ActiveLive.texture.width / Game.ActiveLive.texture.height;

		yield return null;

		MusicPlayer.Instance.SetClip(Game.ActiveLive.clip);
		playPanel.Init(Game.ActiveLive.notes);
		playPanel.Play();
	}
}
