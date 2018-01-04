using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Store = PreviewLabs.PlayerPrefs;

using LoveLivePractice.Api;

public class Game : MonoBehaviour {
	public const string LiveListKey = "game_livelist";

	public static bool IsOffline;
	public static readonly Dictionary<string, Live> LiveMap = new Dictionary<string, Live>();

	public static int AvailableLiveCount;

	public static int FocusLiveLimit = 24;
	public static readonly List<Live> FocusLives = new List<Live>(), CachedLives = new List<Live>();
	public static Live FocusLive;

	public static bool Dirty;

	public static void LoadCachedData() {
		if (!Store.HasKey(LiveListKey)) return;

		var liveList = JsonUtility.FromJson<LiveList>(Store.GetString(LiveListKey));
		AvailableLiveCount = liveList.availableLiveCount;
		foreach (var live in liveList.lives) {
			LiveMap.Add(live.id, live);
			if (live.cached) CachedLives.Add(live);
		}

		FocusCachedLives(0, FocusLiveLimit);
	}

	public static void FocusCachedLives(int offset, int limit) {
		FocusLives.Clear();

		if (offset > CachedLives.Count) return; 
		FocusLives.AddRange(CachedLives.GetRange(offset, Mathf.Min(limit, CachedLives.Count - offset)));
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

		Store.SetString(LiveListKey, JsonUtility.ToJson(new LiveList { 
			lives = LiveMap.Values.ToArray(),
			availableLiveCount = AvailableLiveCount,
		}));

		Store.Flush();

		Debug.Log(LiveMap.Values.Count + " lives stored");
	}
}

[System.Serializable]
public class LiveList {
	public int availableLiveCount;
	public Live[] lives;
}

[System.Serializable]
public class Live {
	public string id;

	public string name, artist, coverPath, uploaderName, categoryName, info;
	public int clickCount, level, likeCount;
	public string mapPath, bgmPath;

	public Color color;
	public Color[] colors;

	public LiveMapNote[] notes;
	public int noteCount, longCount, parallelCount, holdCount;

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

	public void Update(ApiLive live) {
		cached = true;

		categoryName = live.category.name;
		likeCount = live.like_count;
		info = live.live_info;

		mapPath = live.map_path;
		bgmPath = live.bgm_path;
	}

	public void Update(ApiLiveMap map) {
		noteCount = map.lane.Length;
		notes = new LiveMapNote[map.lane.Length];

		longCount = 0;
		parallelCount = 0;
		holdCount = 0;

		System.Array.Sort(map.lane);

		for (int i = 0; i < noteCount; i++) {
			notes[i] = new LiveMapNote { 
				lane = map.lane[i].lane,
				starttime = map.lane[i].starttime / 1000,
				endtime = map.lane[i].endtime / 1000,
				longnote = map.lane[i].longnote,
				parallel = map.lane[i].parallel,
				hold = map.lane[i].hold,
			};

			if (notes[i].longnote) longCount += 1;
			if (notes[i].parallel) parallelCount += 1;
			if (notes[i].hold) holdCount += 1;
		}
	}
}

[System.Serializable]
public class LiveMapNote {
	public int lane;
	public float starttime, endtime;
	public bool longnote, parallel, hold;
}