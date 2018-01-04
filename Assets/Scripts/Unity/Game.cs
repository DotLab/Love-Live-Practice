using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Store = PreviewLabs.PlayerPrefs;

using LoveLivePractice.Api;

public class Game : MonoBehaviour {
	const string GameDataKey = "game_data";
	const int GameDataVersion = 1;

	public static bool IsOffline;
	public static readonly Dictionary<string, Live> LiveDict = new Dictionary<string, Live>();

	public static int AvailableLiveCount;
	public static readonly List<Live> ActiveLives = new List<Live>(), CachedLives = new List<Live>();
	public static Live ActiveLive;

	static bool Dirty;

	public static void LoadGameData() {
		if (!Store.HasKey(GameDataKey)) return;

		var data = JsonUtility.FromJson<GameData>(Store.GetString(GameDataKey));
		if (data.version != GameDataVersion) {
			Store.DeleteKey(GameDataKey);
			return;
		}

		AvailableLiveCount = data.availableLiveCount;
		foreach (var live in data.lives) {
			LiveDict.Add(live.id, live);
			if (live.cached) CachedLives.Add(live);
		}

		ActivateCachedLives(0, UrlBuilder.ApiLimit);
	}

	public static void ActivateCachedLives(int offset, int limit) {
		ActiveLives.Clear();

		if (offset > CachedLives.Count) return; 
		ActiveLives.AddRange(CachedLives.GetRange(offset, Mathf.Min(limit, CachedLives.Count - offset)));
	}

	public static void ActivateApiLiveList(ApiLiveListItem[] items) {
		ActiveLives.Clear();

		foreach (var item in items) {
			if (Game.LiveDict.ContainsKey(item.live_id)) LiveDict[item.live_id].Update(item);
			else Game.LiveDict.Add(item.live_id, new Live(item));

			ActiveLives.Add(LiveDict[item.live_id]);
		}

		Dirty = true;
	}

	public static void SetDirty() {
		Dirty = true;
	}

	public void OnApplicationFocus() {
		if (LiveDict.Values.Count == 0 || !Dirty) return;
		Dirty = false;

		Store.SetString(GameDataKey, JsonUtility.ToJson(new GameData { 
			version = GameDataVersion,
			lives = LiveDict.Values.ToArray(),
			availableLiveCount = AvailableLiveCount,
		}));

		Store.Flush();

		Debug.Log(LiveDict.Values.Count + " lives stored");
	}
}