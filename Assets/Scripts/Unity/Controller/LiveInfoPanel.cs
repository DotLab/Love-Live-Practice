using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;

public class LiveInfoPanel : MonoBehaviour {
	public RawImage bgUiRawImage;
	public AspectRatioFitter bgFitter;
	public EasedHidable bgHidable;
	public AudioSource MainSource;
	public LiveMapPreviewPanel mapPreviewPanel;

	public TextSwapable titleText, uploaderText, songInfoText, mapInfoText, playerInfoText;

	public void ChangeLive() {
		StopAllCoroutines();
		StartCoroutine(ChangeLiveHandler(Game.ActiveLive));
	}

	IEnumerator ChangeLiveHandler(Live live) {
		bgHidable.ForceHide();
		MusicPlayer.Instance.Stop();
		mapPreviewPanel.Stop();

		yield return null;
//		yield return new WaitForSeconds(bgHidable.TransitionDuration + 0.1f);

		titleText.Swap(live.artist + " - " + live.name + " [LEVEL" + live.level.ToString("N0") + "]");
		uploaderText.Swap("Mapped by " + live.uploaderName);

		bgUiRawImage.texture = live.texture;
		bgFitter.aspectRatio = (float)live.texture.width / live.texture.height;
	
		bgHidable.Show();

		if (!live.cached) {
			var www = new WWW(UrlBuilder.GetLiveUrl(live.id));
			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);
			
			var response = JsonUtility.FromJson<ApiLiveResponse>(www.text);
			live.Update(response.content);
			Game.SetDirty();
		}
			
		var bgmJob = LocalStorage.LoadAudioClip(live.bgmPath);

		songInfoText.Swap(string.Format("Category: {0} Likes: {1:N0} Played: {2:N0}", live.categoryName, live.likeCount, live.clickCount));
		playerInfoText.Swap(live.info);

		yield return new WaitForSeconds(playerInfoText.TransitionDuration + 0.1f);

		if (live.notes == null || live.notes.Length == 0) {
			Debug.Log("New map");
			var mapJob = LocalStorage.LoadText(live.mapPath);
			while (!mapJob.IsFinished()) yield return null;
			string json = mapJob.GetData();
			var map = JsonUtility.FromJson<ApiLiveMap>(ApiLiveMap.Transform(json));
			live.Update(map);
			Game.SetDirty();
		}

		mapInfoText.Swap(string.Format("Notes: {0:N0} Long: {1:N0} Parallel: {2:N0} Hold: {3:N0}", live.noteCount, live.longCount, live.parallelCount, live.holdCount));

		while (!bgmJob.IsFinished()) yield return null;
		live.clip = bgmJob.GetData();

		MusicPlayer.Instance.SetClip(live.clip);
		mapPreviewPanel.Colors = live.colors;
		mapPreviewPanel.Init(live.notes);
		mapPreviewPanel.Play();
	}
}
