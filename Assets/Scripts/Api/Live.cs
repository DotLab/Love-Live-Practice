namespace LoveLivePractice.Api {
	[System.Serializable]
	public class LiveResponse {
		public bool succeed;
		public Live content;
	}

	[System.Serializable]
	public class Live {
		public string live_name, live_info, live_id, update_time, state;
		public string artist, bgimg_path, bgm_path, cover_path, map_path;
		public int level, like_count, click_count;
		public bool memberonly;

		public LiveCategory category;
		public LiveUser upload_user;
	}

	[System.Serializable]
	public class LiveCategory {
		public string name;
		public int id;
	}

	[System.Serializable]
	public class LiveUser {
		public string username, avatar_path;
		public int post_count;
	}
}