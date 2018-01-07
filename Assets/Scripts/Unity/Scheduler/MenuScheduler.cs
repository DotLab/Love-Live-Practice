using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;
using LoveLivePractice.Api;

public class MenuScheduler : MonoBehaviour {
	public static MenuScheduler Instance;

	public GameObject touchBlocker;

	public EasedHidable mainHidable, maskHidable, flashHidable;
	public LiveScroll liveScroll;
	public LiveInfoPanel liveInfoPanel;
	public InputField pageNumberInput;

	public void Start() {
		Instance = this;

		pageNumberInput.onEndEdit.AddListener(value => {
			int newPage;
			if (int.TryParse(value, out newPage)) {
				ChangePage(newPage);
			} else pageNumberInput.text = currentPage.ToString();
		});

		StartCoroutine(StartHandler());
	}
		
	IEnumerator StartHandler() {
#if UNITY_EDITOR
		if (Game.LiveDict.Keys.Count == 0) {
			Game.LoadGameData();
//
//			Game.AvailableLiveCount = Game.CachedLives.Count;
//			Game.IsOffline = true;

			Game.IsOffline = false;

			var www = new WWW(UrlBuilder.GetLiveListUrl(0, UrlBuilder.ApiLimit));
			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);
			var response = JsonUtility.FromJson<ApiLiveListResponse>(www.text);
			Game.ActivateApiLiveList(response.content.items);
		}
#endif

		if (Game.ActiveLive == null) {
			yield return new WaitForSeconds(1);
			
			maskHidable.Hide();
			yield return Wait(maskHidable.TransitionDuration);
		} else {
			liveInfoPanel.bgUiRawImage.texture = Game.ActiveLive.texture;
			liveInfoPanel.bgFitter.aspectRatio = (float)Game.ActiveLive.texture.width / Game.ActiveLive.texture.height;
			liveInfoPanel.bgHidable.ForceShow();

			maskHidable.ForceHide();
		}

		liveScroll.RebuildContent();
	}

	public void ChangePage(int newPage) {
		if (newPage < 0 || (Game.IsOffline && newPage >= Game.CachedLives.Count / UrlBuilder.ApiLimit) || (!Game.IsOffline && newPage >= Game.AvailableLiveCount / UrlBuilder.ApiLimit)) {
			pageNumberInput.text = currentPage.ToString();
			return;
		}

		currentPage = newPage;
		pageNumberInput.text = currentPage.ToString();

		StopAllCoroutines();
		StartCoroutine(ChangePageHandler());
	}

	IEnumerator ChangePageHandler() {
		liveScroll.hidable.Hide();

		yield return Wait(liveScroll.hidable.TransitionDuration + 0.1f);

		if (Game.IsOffline) {
			Game.ActivateCachedLives(UrlBuilder.ApiLimit * currentPage, UrlBuilder.ApiLimit);
		} else {
			var www = new WWW(UrlBuilder.GetLiveListUrl(UrlBuilder.ApiLimit * currentPage, UrlBuilder.ApiLimit));
			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);
			
			var apiLiveList = JsonUtility.FromJson<ApiLiveListResponse>(www.text).content;
			Game.ActivateApiLiveList(apiLiveList.items);
		}

		if (Game.ActiveLive != null && Game.ActiveLive.texture != null) LocalStorage.TextureDict.Remove(Game.ActiveLive.coverPath);
		LocalStorage.ClearStorage();
		if (Game.ActiveLive != null && Game.ActiveLive.texture != null) LocalStorage.TextureDict.Add(Game.ActiveLive.coverPath, Game.ActiveLive.texture);

		liveScroll.RebuildContent();

		liveScroll.hidable.Show();
	}

	[ContextMenu("Flash")]
	public void Flash() {
		flashHidable.ForceShow();
		flashHidable.Hide();
	}

	static WaitForSeconds Wait(float t) {
		return new WaitForSeconds(t);
	}

	public static void ChangeLive() {
		Instance.Flash();
		Instance.liveInfoPanel.ChangeLive();
	}

	public static void PlayLive() {
		Instance.touchBlocker.SetActive(true);

		Instance.StartCoroutine(Instance.PlayLiveHandler());
	}

	IEnumerator PlayLiveHandler() {
		yield return null;

		mainHidable.Hide();

		yield return Wait(mainHidable.TransitionDuration + 0.1f);

		UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
	}

	static int currentPage, maxPage;

	public static void NextPage() {
		Instance.ChangePage(currentPage + 1);
	}

	public static void PreviousPage() {
		Instance.ChangePage(currentPage - 1);
	}
}
