using System.IO;
using System.Collections.Generic;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

using UnityEngine;

using LoveLivePractice.Api;

public class ResourceStore : MonoBehaviour {
	public const int MaxConcurrentJobCount = 4;

	static readonly Dictionary<string, Texture2D> TextureDict = new Dictionary<string, Texture2D>();
	static readonly Dictionary<string, AudioClip> AudioClipDict = new Dictionary<string, AudioClip>();
	static readonly Dictionary<string, string> TextDict = new Dictionary<string, string>();

	static readonly LinkedList<ILoadJob> LoadJobPool = new LinkedList<ILoadJob>();
	static readonly LinkedList<IWwwLoadJob> WwwLoadJobQueue = new LinkedList<IWwwLoadJob>();

	public interface ILoadJob {
		float GetProgress();
		bool IsFinished();
		void ExecuteCallback();
	}

	public interface ILoadJob<T> : ILoadJob {
		T GetData();
	}

	public interface IWwwLoadJob : ILoadJob {
		void StartDownload();
	}

	public abstract class LoadJob<T> : ILoadJob<T> {
		readonly System.Action<ILoadJob<T>> callback;
		bool isCallbackCalled;

		public LoadJob(System.Action<ILoadJob<T>> callback) {
			this.callback = callback;

			LoadJobPool.AddLast(this);
		}

		public void ExecuteCallback() {
			if (isCallbackCalled || callback == null) return;

			callback(this);
			isCallbackCalled = true;
		}

		public abstract float GetProgress();
		public abstract bool IsFinished();
		public abstract T GetData();
	}

	public class SimpleLoadJob<T> : LoadJob<T> {
		readonly T data;
		readonly int delay;
		int counter;

		public SimpleLoadJob(T data, System.Action<ILoadJob<T>> callback, int delay = 2) : base(callback) {
			this.data = data;
			this.delay = delay;
		}

		public override float GetProgress() {
			return (float)counter / delay;
		}

		public override bool IsFinished() {
			counter += 1;
			return counter > delay;
		}

		public override T GetData() {
			return data;
		}
	}

	public abstract class WwwLoadJob<T> : LoadJob<T>, IWwwLoadJob {
		protected WWW www;
		protected T data;

		protected readonly string path, url, filePath;

		public WwwLoadJob(string path, string url, string filePath, System.Action<ILoadJob<T>> callback) : base(callback) {
			this.path = path;
			this.url = url;
			this.filePath = filePath;

			WwwLoadJobQueue.AddFirst(this);
		}

		public void StartDownload() {
			www = new WWW(url);
		}

		public override float GetProgress() {
			return www == null ? 0 : www.progress;
		}

		public override bool IsFinished() {
			return www != null && www.isDone;
		}

		public override T GetData() {
			if (!IsFinished()) throw new System.Exception("Job not finished!");

			if (data != null) return data;
			File.WriteAllBytes(GetCacheFilePath(path), www.bytes);
			return data = LoadData();
		}

		protected abstract T LoadData();
	}

	public class WwwLoadTextureJob : WwwLoadJob<Texture2D> {
		public WwwLoadTextureJob(string path, string url, string filePath, System.Action<ILoadJob<Texture2D>> callback) : base(path, url, filePath, callback) {
		}

		protected override Texture2D LoadData() {
			var texture = www.texture;
			TextureDict.Add(path, texture);
			return texture;
		}
	}

	public class WwwLoadAudioClipJob : WwwLoadJob<AudioClip> {
		public WwwLoadAudioClipJob(string path, string url, string filePath, System.Action<ILoadJob<AudioClip>> callback) : base(path, url, filePath, callback) {
		}

		protected override AudioClip LoadData() {
			var clip = www.GetAudioClip(false);
			AudioClipDict.Add(path, clip);
			return clip;
		}
	}

	public class WwwLoadTextJob : WwwLoadJob<string> {
		public WwwLoadTextJob(string path, string url, string filePath, System.Action<ILoadJob<string>> callback) : base(path, url, filePath, callback) {
		}

		protected override string LoadData() {
			var text = www.text;
			TextDict.Add(path, text);
			return text;
		}
	}

	public static ILoadJob<Texture2D> LoadTexture(string path, System.Action<ILoadJob<Texture2D>> callback = null) {
		if (TextureDict.ContainsKey(path)) {
			return new SimpleLoadJob<Texture2D>(TextureDict[path], callback);
		} else if (File.Exists(GetCacheFilePath(path))) {
			byte[] bytes = File.ReadAllBytes(GetCacheFilePath(path));

			var texture = new Texture2D(4, 4);
			texture.LoadImage(bytes);

			TextureDict.Add(path, texture);
			return new SimpleLoadJob<Texture2D>(texture, callback);
		} else return new WwwLoadTextureJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path), callback);
	}

	public static ILoadJob<AudioClip> LoadAudioClip(string path, System.Action<ILoadJob<AudioClip>> callback = null) {
		if (AudioClipDict.ContainsKey(path)) {
			return new SimpleLoadJob<AudioClip>(AudioClipDict[path], callback);
		} else if (File.Exists(GetCacheFilePath(path))) {
			return new WwwLoadAudioClipJob(path, GetCacheFileUrl(path), GetCacheFilePath(path), callback);
		} else return new WwwLoadAudioClipJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path), callback);
	}

	public static ILoadJob<string> LoadText(string path, System.Action<ILoadJob<string>> callback = null) {
		if (TextDict.ContainsKey(path)) {
			return new SimpleLoadJob<string>(TextDict[path], callback);
		} else if (File.Exists(GetCacheFilePath(path))) {
			string text = File.ReadAllText(GetCacheFilePath(path));
			TextDict.Add(path, text);
			return new SimpleLoadJob<string>(text, callback);
		} else return new WwwLoadTextJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path), callback);
	}

	public static string GetCacheFilePath(string path) {
		return Application.persistentDataPath + "/" + path;
	}

	public static string GetCacheFileUrl(string path) {
		return "file:///" + GetCacheFilePath(path);
	}
			
	public void Start() {
		DontDestroyOnLoad(this);
	}

	static readonly LinkedList<IWwwLoadJob> LoadingJobs = new LinkedList<IWwwLoadJob>();

	public void FixedUpdate() {
		if (LoadJobPool.Count > 0) {
			var node = LoadJobPool.First;
			while (node != null && !node.Value.IsFinished()) node = node.Next;
			if (node != null) {
				node.Value.ExecuteCallback();
				LoadJobPool.Remove(node);
//				Debug.LogFormat("Jobs: {0}", LoadJobPool.Count);
			}
		}

		if (LoadingJobs.Count > 0) {
			var node = LoadingJobs.First;
			while (node != null) {
				var next = node.Next;
				if (node.Value.IsFinished()) {
					LoadingJobs.Remove(node);
					Debug.LogFormat("Downloading: {0}, Queued: {1}", LoadingJobs.Count, WwwLoadJobQueue.Count);
				}
				node = next;
			}
		}

		while (WwwLoadJobQueue.Count > 0 && LoadingJobs.Count < MaxConcurrentJobCount) {
			var job = WwwLoadJobQueue.First.Value;
			WwwLoadJobQueue.RemoveFirst();
			job.StartDownload();
			LoadingJobs.AddLast(job);
			Debug.LogFormat("Downloading: {0}, Queued: {1}", LoadingJobs.Count, WwwLoadJobQueue.Count);
		}
	}
}
