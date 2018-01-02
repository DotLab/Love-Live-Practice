using System.IO;
using System.Collections;
using System.Collections.Generic;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

using UnityEngine;

using LoveLivePractice.Api;

public class ResourceStore : MonoBehaviour {
	public const int MaxConcurrentJobCount = 4;

	static readonly Dictionary<string, Texture2D> TextureDict = new Dictionary<string, Texture2D>();
	static readonly Dictionary<string, AudioClip> AudioClipDict = new Dictionary<string, AudioClip>();
	static readonly Dictionary<string, string> TextDict = new Dictionary<string, string>();
	static readonly LinkedList<IWwwLoadJob> WwwLoadJobQueue = new LinkedList<IWwwLoadJob>();

	public interface ILoadJob<T> {
		float GetProgress();
		bool IsFinished();
		T GetData();
	}

	public interface IWwwLoadJob {
		void StartDownload();
		void FinishDownload();

		float GetProgress();
		bool IsFinished();
	}

	public class SimpleLoadJob<T> : ILoadJob<T> {
		readonly T data;

		public SimpleLoadJob(T data) {
			this.data = data;
		}

		public float GetProgress() {
			return 1;
		}

		public bool IsFinished() {
			return true;
		}

		public T GetData() {
			return data;
		}
	}

	public abstract class WwwLoadJob<T> : IWwwLoadJob, ILoadJob<T> {
		protected WWW www;
		protected T data;

		protected readonly string path, url, filePath;

		protected readonly System.Action<ILoadJob<T>> callback;

		public WwwLoadJob(string path, string url, string filePath, System.Action<ILoadJob<T>> callback) {
			this.path = path;
			this.url = url;
			this.filePath = filePath;

			this.callback = callback;

			WwwLoadJobQueue.AddFirst(this);
		}

		public void StartDownload() {
			www = new WWW(url);
		}

		public void FinishDownload() {
			if (callback != null) callback(this);
		}

		public float GetProgress() {
			if (www == null) return 0;
			return www.progress;
		}

		public bool IsFinished() {
			if (www == null) return false;
			return www.isDone;
		}

		public T GetData() {
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
			var job = new SimpleLoadJob<Texture2D>(TextureDict[path]);
			if (callback != null) callback(job);
			return job;
		} else if (File.Exists(GetCacheFilePath(path))) {
			byte[] bytes = File.ReadAllBytes(GetCacheFilePath(path));

			var texture = new Texture2D(4, 4);
			texture.LoadImage(bytes);

			TextureDict.Add(path, texture);
			var job = new SimpleLoadJob<Texture2D>(texture);
			if (callback != null) callback(job);
			return job;
		} else return new WwwLoadTextureJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path), callback);
	}

	public static ILoadJob<AudioClip> LoadAudioClip(string path, System.Action<ILoadJob<AudioClip>> callback = null) {
		if (AudioClipDict.ContainsKey(path)) {
			var job = new SimpleLoadJob<AudioClip>(AudioClipDict[path]);
			if (callback != null) callback(job);
			return job;
		} else if (File.Exists(GetCacheFilePath(path))) {
			return new WwwLoadAudioClipJob(path, GetCacheFileUrl(path), GetCacheFilePath(path), callback);
		} else return new WwwLoadAudioClipJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path), callback);
	}

	public static ILoadJob<string> LoadText(string path, System.Action<ILoadJob<string>> callback = null) {
		if (TextDict.ContainsKey(path)) {
			var job = new SimpleLoadJob<string>(TextDict[path]);
			if (callback != null) callback(job);
			return job;
		} else if (File.Exists(GetCacheFilePath(path))) {
			string text = File.ReadAllText(GetCacheFilePath(path));
			TextDict.Add(path, text);
			var job = new SimpleLoadJob<string>(text);
			if (callback != null) callback(job);
			return job;
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
		if (WwwLoadJobQueue.Count <= 0 && LoadingJobs.Count <= 0) return;

		var node = LoadingJobs.First;
		while (node != null) {
			var next = node.Next;
			if (node.Value.IsFinished()) {
				node.Value.FinishDownload();	
				LoadingJobs.Remove(node);
				Debug.LogFormat("Loading: {0}, Queued: {1}", LoadingJobs.Count, WwwLoadJobQueue.Count);
			}
			node = next;
		}

		if (LoadingJobs.Count > MaxConcurrentJobCount) return;

		while (WwwLoadJobQueue.Count > 0 && LoadingJobs.Count <= MaxConcurrentJobCount) {
			var job = WwwLoadJobQueue.First.Value;
			WwwLoadJobQueue.RemoveFirst();
			job.StartDownload();
			LoadingJobs.AddLast(job);
			Debug.LogFormat("Loading: {0}, Queued: {1}", LoadingJobs.Count, WwwLoadJobQueue.Count);
		}
	}
}
