using System.Collections;
using UnityEngine;

using LoveLivePractice.Api;

using Uif;

public class WelcomeScheduler : MonoBehaviour {
	public EasedHidable mainHidable;
	public EasedColorSwapable backgroundColor;

	public EasedStringSwapable statusText;

	public void Start() {
		StartCoroutine(StartHandler());
	}

	IEnumerator StartHandler() {
		yield return null;

		var wait = new WaitForSeconds(statusText.TransitionDuration + 0.1f);
		yield return new WaitForSeconds(1);

		backgroundColor.Swap(Color.black);
		mainHidable.Show();
		yield return new WaitForSeconds(mainHidable.TransitionDuration + 0.1f);

		statusText.Swap("Loading cached data...");
		yield return wait;
		Game.LoadCachedData();

		statusText.Swap(Game.LiveMap.Values.Count + " lives loaded");
		yield return wait;

		statusText.Swap("Loading first page...");
		yield return wait;

		var www = new WWW(UrlBuilder.GetLiveListUrl(0, UrlBuilder.ApiLimit));
		yield return www;

		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log(www.error);
			statusText.Swap("Network error, entering Offline Mode");
			yield return wait;

			Game.AvailableLiveCount = Game.CachedLives.Count;
			Game.IsOffline = true;
		} else {
			var apiLiveList = JsonUtility.FromJson<ApiLiveListResponse>(www.text).content;
			Game.CacheLiveList(apiLiveList.items);

			statusText.Swap("Good to go, entering Online Mode");
			yield return wait;

			Game.AvailableLiveCount = apiLiveList.count;
			Game.IsOffline = false;
		}

		mainHidable.Hide();
		yield return new WaitForSeconds(mainHidable.TransitionDuration + 0.1f);

		UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
	}
}
