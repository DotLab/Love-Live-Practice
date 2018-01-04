using System;
using System.IO;
using System.Collections.Generic;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

using UnityEngine;

using LoveLivePractice.Api;

public class LocalStorage : MonoBehaviour {
	public const int MaxConcurrentJobCount = 4;

	static readonly Dictionary<string, Texture2D> TextureDict = new Dictionary<string, Texture2D>();
	static readonly Dictionary<string, AudioClip> AudioClipDict = new Dictionary<string, AudioClip>();
	static readonly Dictionary<string, string> TextDict = new Dictionary<string, string>();

	static readonly LinkedList<ILoadJob> PendingLoadJobPool = new LinkedList<ILoadJob>();
	static readonly Dictionary<string, ILoadJob> PendingLoadJobDict = new Dictionary<string, ILoadJob>();

	static readonly LinkedList<IWwwLoadJob> ActiveWwwLoadJobList = new LinkedList<IWwwLoadJob>(), WwwLoadJobQueue = new LinkedList<IWwwLoadJob>();

	public interface ILoadJob {
		string GetKey();
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
		readonly Action<ILoadJob<T>> callback;
		readonly string key;
		bool isCallbackCalled;

		public LoadJob(string key, Action<ILoadJob<T>> callback) {
			this.callback = callback;
			this.key = key;

			PendingLoadJobPool.AddLast(this);
			PendingLoadJobDict.Add(key, this);
		}

		public void ExecuteCallback() {
			if (isCallbackCalled || callback == null) return;

			callback(this);
			isCallbackCalled = true;
		}

		public string GetKey() {
			return key;
		}

		public abstract float GetProgress();
		public abstract bool IsFinished();
		public abstract T GetData();
	}

	public class SimpleLoadJob<T> : LoadJob<T> {
		readonly T data;
		readonly int delay;
		int counter;

		public SimpleLoadJob(string key, T data, Action<ILoadJob<T>> callback, int delay = 2) : base(key, callback) {
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

		public WwwLoadJob(string path, string url, string filePath, Action<ILoadJob<T>> callback) : base(path, callback) {
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
			if (!IsFinished()) throw new Exception("Job not finished!");

			if (data != null) return data;
			File.WriteAllBytes(GetLocalFilePath(path), www.bytes);
			return data = LoadData();
		}

		protected abstract T LoadData();
	}

	public class WwwLoadTextureJob : WwwLoadJob<Texture2D> {
		public WwwLoadTextureJob(string path, string url, string filePath, Action<ILoadJob<Texture2D>> callback) : base(path, url, filePath, callback) {
		}

		protected override Texture2D LoadData() {
			var texture = www.texture;
			TextureDict.Add(path, texture);
			return texture;
		}
	}

	public class WwwLoadAudioClipJob : WwwLoadJob<AudioClip> {
		public WwwLoadAudioClipJob(string path, string url, string filePath, Action<ILoadJob<AudioClip>> callback) : base(path, url, filePath, callback) {
		}

		protected override AudioClip LoadData() {
			var clip = www.GetAudioClip(false);
			AudioClipDict.Add(path, clip);
			return clip;
		}
	}

	public class WwwLoadTextJob : WwwLoadJob<string> {
		public WwwLoadTextJob(string path, string url, string filePath, Action<ILoadJob<string>> callback) : base(path, url, filePath, callback) {
		}

		protected override string LoadData() {
			var text = www.text;
			TextDict.Add(path, text);
			return text;
		}
	}

	public static bool IsJobPending(string key) {
		return PendingLoadJobDict.ContainsKey(key);
	}

	public static ILoadJob<Texture2D> LoadTexture(string path, Action<ILoadJob<Texture2D>> callback = null) {
		if (PendingLoadJobDict.ContainsKey(path)) {  // Duplicated job
			return (ILoadJob<Texture2D>)PendingLoadJobDict[path];
		} else if (TextureDict.ContainsKey(path)) {  // From memory
			return new SimpleLoadJob<Texture2D>(path, TextureDict[path], callback);
		} else if (File.Exists(GetLocalFilePath(path))) {  // From disk
			byte[] bytes = File.ReadAllBytes(GetLocalFilePath(path));
			var texture = new Texture2D(4, 4);
			texture.LoadImage(bytes);
			TextureDict.Add(path, texture);
			return new SimpleLoadJob<Texture2D>(path, texture, callback);
		} else {  // Create new
			return new WwwLoadTextureJob(path, UrlBuilder.GetUploadUrl(path), GetLocalFilePath(path), callback);
		}
	}

	public static ILoadJob<string> LoadText(string path, Action<ILoadJob<string>> callback = null) {
		return LoadText(path, UrlBuilder.GetUploadUrl(path), callback);
	}

	public static ILoadJob<string> LoadText(string key, string url, Action<ILoadJob<string>> callback = null) {
		if (PendingLoadJobDict.ContainsKey(key)) {  // Duplicated job
			return (ILoadJob<string>)PendingLoadJobDict[key];
		} else if (TextDict.ContainsKey(key)) {  // From memory
			return new SimpleLoadJob<string>(key, TextDict[key], callback);
		} else if (File.Exists(GetLocalFilePath(key))) {  // From disk
			string text = File.ReadAllText(GetLocalFilePath(key));
			TextDict.Add(key, text);
			return new SimpleLoadJob<string>(key, text, callback);
		} else {  // Create new
			return new WwwLoadTextJob(key, url, GetLocalFilePath(key), callback);
		}
	}

	public static ILoadJob<AudioClip> LoadAudioClip(string path, Action<ILoadJob<AudioClip>> callback = null) {
		if (PendingLoadJobDict.ContainsKey(path)) {  // Duplicated job
			return (ILoadJob<AudioClip>)PendingLoadJobDict[path];
		} else if (AudioClipDict.ContainsKey(path)) {  // From memory
			return new SimpleLoadJob<AudioClip>(path, AudioClipDict[path], callback);
		} else if (File.Exists(GetLocalFilePath(path))) {  // From disk
			return new WwwLoadAudioClipJob(path, GetLocalFileUrl(path), GetLocalFilePath(path), callback);
		} else {  // Create new
			return new WwwLoadAudioClipJob(path, UrlBuilder.GetUploadUrl(path), GetLocalFilePath(path), callback);
		}
	}

	static string GetLocalFilePath(string path) {
		return Application.persistentDataPath + "/" + path;
	}

	static string GetLocalFileUrl(string path) {
		return "file:///" + GetLocalFilePath(path);
	}

	public void FixedUpdate() {
		if (PendingLoadJobPool.Count > 0) {
			var node = PendingLoadJobPool.First;
			while (node != null && !node.Value.IsFinished()) node = node.Next;
			if (node != null) {
				node.Value.ExecuteCallback();
				PendingLoadJobDict.Remove(node.Value.GetKey());
				PendingLoadJobPool.Remove(node);
				Debug.LogFormat("Job Finish ({0})", PendingLoadJobPool.Count);
			}
		}

		if (ActiveWwwLoadJobList.Count > 0) {
			var node = ActiveWwwLoadJobList.First;
			while (node != null) {
				var next = node.Next;
				if (node.Value.IsFinished()) {
					ActiveWwwLoadJobList.Remove(node);
					Debug.LogFormat("DL Finish ({0} / {1})", ActiveWwwLoadJobList.Count, ActiveWwwLoadJobList.Count + WwwLoadJobQueue.Count);
				}
				node = next;
			}
		}

		while (WwwLoadJobQueue.Count > 0 && ActiveWwwLoadJobList.Count < MaxConcurrentJobCount) {
			var job = WwwLoadJobQueue.First.Value;
			WwwLoadJobQueue.RemoveFirst();
			job.StartDownload();
			ActiveWwwLoadJobList.AddLast(job);
			Debug.LogFormat("DL Start ({0} / {1})", ActiveWwwLoadJobList.Count, ActiveWwwLoadJobList.Count + WwwLoadJobQueue.Count);
		}
	}
}
