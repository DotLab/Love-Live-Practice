namespace LoveLivePractice.Api {
	[System.Serializable]
	public class LiveListResponse {
		public bool succeed;
		public LiveList content;
	}

	[System.Serializable]
	public class LiveList {
		public int count;
		public LiveListItem[] items;
	}

	[System.Serializable]
	public class LiveListItem {
		public string live_name, live_id, artist, cover_path;
		public int click_count, level;
		public bool memberonly;
		public LiveListItemUser upload_user;
	}

	[System.Serializable]
	public class LiveListItemUser {
		public string username;
	}
}