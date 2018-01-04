using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoveLivePractice.Api;
using LoveLivePractice.Net;

namespace LoveLivePractice.Unity {
	public class LocalStorageTest : MonoBehaviour {
		public Texture2D texture;
		public AudioClip clip;

		[ContextMenu("Test")]
		public void Start() {
			texture = null;
			clip = null;

			StartCoroutine(TestHandler());
		}

		public IEnumerator TestHandler() {
			var liveJson = SyncUtil.Get(UrlBuilder.GetLiveUrl("h4SFEBaRxDXz3bzB"));
			var liveResponse = JsonUtility.FromJson<ApiLiveResponse>(liveJson);

			var mapJob = LocalStorage.LoadText(liveResponse.content.map_path);
			while (!mapJob.IsFinished()) {
				Debug.Log("mapJob: " + mapJob.GetProgress());
				yield return new WaitForSeconds(0.5f);
			}

			var mapJson = mapJob.GetData();
			Debug.Log(mapJson.Length);

			var coverJob = LocalStorage.LoadTexture(liveResponse.content.cover_path);
			while (!coverJob.IsFinished()) {
				Debug.Log("coverJob: " + coverJob.GetProgress());
				yield return new WaitForSeconds(0.5f);
			}

			texture = coverJob.GetData();

			var bgmJob = LocalStorage.LoadAudioClip(liveResponse.content.bgm_path);
			while (!bgmJob.IsFinished()) {
				Debug.Log("bgmJob: " + bgmJob.GetProgress());
				yield return new WaitForSeconds(0.5f);
			}

			clip = bgmJob.GetData();
			AudioSource.PlayClipAtPoint(clip, Vector3.zero);
		}
	}
}

