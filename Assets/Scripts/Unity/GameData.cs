using UnityEngine;
using LoveLivePractice.Api;

[System.Serializable]
public class GameData {
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