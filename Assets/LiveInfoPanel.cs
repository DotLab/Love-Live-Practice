using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;
using LoveLivePractice.Unity;

public class LiveInfoPanel : MonoBehaviour {
	public Live Live;

	public RawImage bgUiRawImage;
	public AspectRatioFitter bgFitter;
	public EasedHidable bgHidable;
	public AudioSource MainSource;
	public MapPreviewPanel mapPreviewPanel;

	public TextSwapable titleText, uploaderText, songInfoText, mapInfoText, playerInfoText;

	public void ChangeLive() {
		if (Live == Game.FocusLive) return;
		Live = Game.FocusLive;

		StopAllCoroutines();
		StartCoroutine(ChangeLiveHandler());
	}

	IEnumerator ChangeLiveHandler() {
		bgHidable.ForceHide();
		MusicPlayer.Instance.Stop();
		mapPreviewPanel.Stop();

		yield return null;
//		yield return new WaitForSeconds(bgHidable.TransitionDuration + 0.1f);

		titleText.Swap(Live.artist + " - " + Live.name + " [LEVEL" + Live.level.ToString("N0") + "]");
		uploaderText.Swap("Mapped by " + Live.uploaderName);

		bgUiRawImage.texture = Live.texture;
		bgFitter.aspectRatio = (float)Live.texture.width / Live.texture.height;
	
		bgHidable.Show();

		if (!Live.cached) {
			var www = new WWW(UrlBuilder.GetLiveUrl(Live.id));
			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);
			
			var response = JsonUtility.FromJson<ApiLiveResponse>(www.text);
			Live.Update(response.content);
			Game.SetDirty();
		}
			
		var bgmJob = ResourceStore.LoadAudioClip(Live.bgmPath);

		songInfoText.Swap(string.Format("Category: {0} Likes: {1:N0} Played: {2:N0}", Live.categoryName, Live.likeCount, Live.clickCount));
		playerInfoText.Swap(Live.info);

		yield return new WaitForSeconds(playerInfoText.TransitionDuration + 0.1f);

		if (Live.notes == null || Live.notes.Length == 0) {
			Debug.Log("New map");
			var mapJob = ResourceStore.LoadText(Live.mapPath);
			while (!mapJob.IsFinished()) yield return null;
			string json = mapJob.GetData();
			var map = JsonUtility.FromJson<ApiLiveMap>(ApiLiveMap.Transform(json));
			Live.Update(map);
			Game.SetDirty();
		}

		mapInfoText.Swap(string.Format("Notes: {0:N0} Long: {1:N0} Parallel: {2:N0} Hold: {3:N0}", Live.noteCount, Live.longCount, Live.parallelCount, Live.holdCount));

		while (!bgmJob.IsFinished()) yield return null;
		var clip = bgmJob.GetData();

		MusicPlayer.Instance.SetClip(clip);
		mapPreviewPanel.Colors = Live.colors;
		mapPreviewPanel.Init(Live.notes);
		mapPreviewPanel.Play();
	}
}
