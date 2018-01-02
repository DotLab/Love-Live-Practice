using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Store = PreviewLabs.PlayerPrefs;

using LoveLivePractice.Api;

public class Game : MonoBehaviour {
	public const string LiveListKey = "game_livelist";

	public static bool IsOffline;
	public static readonly Dictionary<string, Live> LiveMap = new Dictionary<string, Live>();

	public static readonly List<Live> FocusLives = new List<Live>(), CachedLives = new List<Live>();
	public static Live FocusLive;

	public static bool Dirty;

	public static bool LoadCachedData() {
		if (Store.HasKey(LiveListKey)) {
			var lives = JsonUtility.FromJson<LiveList>(Store.GetString(LiveListKey)).lives;
			foreach (var live in lives) {
				LiveMap.Add(live.id, live);
				if (live.cached) CachedLives.Add(live);
			}

			return true;
		}

		return false;
	}

	public static void CacheLiveList(ApiLiveListItem[] items) {
		FocusLives.Clear();

		foreach (var item in items) {
			if (Game.LiveMap.ContainsKey(item.live_id)) LiveMap[item.live_id].Update(item);
			else Game.LiveMap.Add(item.live_id, new Live(item));

			FocusLives.Add(LiveMap[item.live_id]);
		}

		Dirty = true;
	}

	public static void SetDirty() {
		Dirty = true;
	}

	public void OnApplicationFocus() {
		if (LiveMap.Values.Count == 0 || !Dirty) return;
		Dirty = false;

		Store.SetString(LiveListKey, JsonUtility.ToJson(new LiveList{ lives = LiveMap.Values.ToArray() }));

		Store.Flush();

		Debug.Log(LiveMap.Values.Count + " lives stored");
	}
}

[System.Serializable]
public class LiveList {
	public Live[] lives;
}

[System.Serializable]
public class Live {
	public string id;

	public string name, artist, coverPath, uploaderName;
	public int clickCount, level;

	public Color color;
	public Color[] colors;

	public bool cached, isDark;

	[System.NonSerialized]
	public Texture2D texture;

	public Live() {
	}

	public Live(ApiLiveListItem item) {
		Update(item);
	}

	public void Update(ApiLiveListItem item) {
		id = item.live_id;

		name = item.live_name;
		artist = item.artist;
		coverPath = item.cover_path;

		clickCount = item.click_count;
		level = item.level;

		uploaderName = item.upload_user.username;
	}
}