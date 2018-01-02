using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;
using LoveLivePractice.Unity;

public class LiveInfoPanel : MonoBehaviour {
	public ApiLiveMap LiveMap;
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
		StartCoroutine(ChangeLiveHandler(Game.FocusLive));
	}

	IEnumerator ChangeLiveHandler(Live live) {
		bgHidable.Hide();

		yield return new WaitForSeconds(bgHidable.TransitionDuration + 0.1f);

		titleText.Swap(live.artist + " - " + live.name + " [LEVEL" + live.level.ToString("N0") + "]");
		uploaderText.Swap("Mapped by " + live.uploaderName);

		bgUiRawImage.texture = live.texture;
		bgFitter.aspectRatio = (float)live.texture.width / live.texture.height;
	
		bgHidable.Show();

		var www = new WWW(UrlBuilder.GetLiveUrl(live.id));
		yield return www;
		if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);

		var response = JsonUtility.FromJson<ApiLiveResponse>(www.text);
		var apiLive = response.content;

		songInfoText.Swap(string.Format("Category: {0} Likes: {1:N0} Played: {2:N0}", apiLive.category.name, apiLive.like_count, apiLive.click_count));
		playerInfoText.Swap(apiLive.live_info);

		yield return new WaitForSeconds(playerInfoText.TransitionDuration + 0.1f);

		var mapJob = ResourceStore.LoadText(apiLive.map_path);
		var bgmJob = ResourceStore.LoadAudioClip(apiLive.bgm_path);

		while (!mapJob.IsFinished()) yield return null;
		string json = mapJob.GetData();

		var map = JsonUtility.FromJson<ApiLiveMap>(ApiLiveMap.Transform(json));
		LiveMap = map;
		System.Array.Sort(LiveMap.lane);
		foreach (var note in LiveMap.lane) {
			note.starttime /= 1000;
			note.endtime /= 1000;
		}

		mapInfoText.Swap(string.Format("Notes: {0:N0} Long: {1:N0} Parallel: {2:N0}", map.lane.Length, map.lane.Count(a => a.longnote), map.lane.Count(a => a.parallel)));

		while (!bgmJob.IsFinished()) yield return null;
		var clip = bgmJob.GetData();
		MainSource.Stop();
		yield return new WaitForSeconds(0.1f);

		MainSource.clip = clip;
		mapPreviewPanel.Colors = live.colors;
		mapPreviewPanel.Init(LiveMap, MainSource);
		mapPreviewPanel.Play();
	}
}
