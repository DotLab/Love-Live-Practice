using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

public class PlayScheduler : MonoBehaviour {
	public static PlayScheduler Instance;

	public static long Score;
	public static int PerfectCount, GreatCount, GoodCount, BadCount, MissCount;
	public static int Combo, MaxCombo;

	public static void ResetScore() {
		Score = 0;
		PerfectCount = GreatCount = GoodCount = BadCount = MissCount = 0;
		Combo = MaxCombo = 0;
	}

	public static float GetComboMultiplier() {
		if (Combo <= 50) return 1;
		if (Combo <= 100) return 1.1f;
		if (Combo <= 200) return 1.15f;
		if (Combo <= 400) return 1.2f;
		if (Combo <= 600) return 1.25f;
		if (Combo <= 800) return 1.3f;
		return 1.35f;
	}

	public RawImage bgUiRawImage;
	public AspectRatioFitter bgFitter;
	public EasedHidable mainHidable;

	public PlayPanel playPanel;

	public void Start() {
		Instance = this;

		StartCoroutine(StartHandler());
	}

	IEnumerator StartHandler() {
		if (Game.ActiveLive == null) {
			Game.LoadGameData();
			Game.ActiveLive = Game.ActiveLives[10];
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

		yield return 1;

		mainHidable.Show();
		yield return new WaitForSeconds(mainHidable.TransitionDuration + 0.1f);

		MusicPlayer.Instance.SetClip(Game.ActiveLive.clip);
		playPanel.Init(Game.ActiveLive.notes);
		playPanel.Play();
	}

	public static void EndPlay() {
		Instance.StartCoroutine(Instance.EndPlayHandler());
	}

	IEnumerator EndPlayHandler() {
		mainHidable.Hide();
		yield return new WaitForSeconds(mainHidable.TransitionDuration + 0.1f);

		UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
	}
}
