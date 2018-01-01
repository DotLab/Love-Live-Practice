using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;
using LoveLivePractice.Unity;

public class LiveInfoPanel : MonoBehaviour {
	public Map LiveMap;
	public LiveListItem Item;

	public Color[] Colors;

	public RawImage bgUiRawImage;
	public AspectRatioFitter bgFitter;
	public EasedHidable bgHidable;
	public AudioSource MainSource;
	public MapPreviewPanel mapPreviewPanel;

	public TextSwapable titleText, uploaderText, songInfoText, mapInfoText, playerInfoText;

	public void ChangeLive(Texture2D texture, LiveListItem liveListItem) {
		if (Item == liveListItem) return;
		Item = liveListItem;

		StopAllCoroutines();

		StartCoroutine(ChangeLiveHandler(texture, liveListItem));
	}

	IEnumerator ChangeLiveHandler(Texture2D texture, LiveListItem liveListItem) {
		bgHidable.Hide();

		yield return new WaitForSeconds(bgHidable.TransitionDuration + 0.1f);

		titleText.Swap(liveListItem.artist + " - " + liveListItem.live_name + " [LEVEL" + liveListItem.level.ToString("N0") + "]");
		uploaderText.Swap("Mapped by " + liveListItem.upload_user.username);

		bgUiRawImage.texture = texture;
		bgFitter.aspectRatio = (float)texture.width / texture.height;
	
		bgHidable.Show();

		var www = new WWW(UrlBuilder.GetLiveUrl(liveListItem.live_id));
		yield return www;
		if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);

		var response = JsonUtility.FromJson<LiveResponse>(www.text);
		var live = response.content;

		songInfoText.Swap(string.Format("Category: {0} Likes: {1:N0} Played: {2:N0}", live.category.name, live.like_count, live.click_count));
		playerInfoText.Swap(live.live_info);

		yield return new WaitForSeconds(playerInfoText.TransitionDuration + 0.1f);

		var mapJob = DataStore.LoadText(live.map_path);
		var bgmJob = DataStore.LoadAudioClip(live.bgm_path);

		while (!mapJob.IsFinished()) yield return null;
		string json = mapJob.GetData();

		var map = JsonUtility.FromJson<Map>(Map.Transform(json));
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
		mapPreviewPanel.Colors = Colors;
		mapPreviewPanel.Init(LiveMap, MainSource);
		mapPreviewPanel.Play();
	}
}
