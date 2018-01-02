using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Uif;
using LoveLivePractice.Api;

public class MenuScheduler : MonoBehaviour {
	public static MenuScheduler Instance;

	public EasedHidable maskHidable, flashHidable;
	public LiveScroll2 liveScroll;
	public LiveInfoPanel liveInfoPanel;

	public void Start() {
		Instance = this;

		StartCoroutine(StartHandler());
	}

	IEnumerator StartHandler() {
#if UNITY_EDITOR
		Game.LoadCachedData();
		Game.IsOffline = false;

		var www = new WWW(UrlBuilder.GetLiveListUrl(0, UrlBuilder.ApiLimit));
		yield return www;
		var response = JsonUtility.FromJson<ApiLiveListResponse>(www.text);
		Game.CacheLiveList(response.content.items);
#endif

		yield return new WaitForSeconds(1);

		maskHidable.Hide();
		yield return Wait(maskHidable.TransitionDuration);

		liveScroll.RebuildContent();
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
}
