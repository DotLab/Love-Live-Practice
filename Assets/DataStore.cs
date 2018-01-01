using System.IO;
using System.Collections;
using System.Collections.Generic;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

using UnityEngine;

using LoveLivePractice.Api;

namespace LoveLivePractice.Unity {
	public class DataStore : MonoBehaviour {
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

			public WwwLoadJob(string path, string url, string filePath) {
				this.path = path;
				this.url = url;
				this.filePath = filePath;

				WwwLoadJobQueue.AddFirst(this);
			}

			public void StartDownload() {
				www = new WWW(url);
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
				if (data != null) return data;
				File.WriteAllBytes(GetCacheFilePath(path), www.bytes);
				return data = LoadData();
			}

			protected abstract T LoadData();
		}

		public class WwwLoadTextureJob : WwwLoadJob<Texture2D> {
			public WwwLoadTextureJob(string path, string url, string filePath) : base(path, url, filePath) {
			}

			protected override Texture2D LoadData() {
				var texture = www.texture;
				TextureDict.Add(path, texture);
				return texture;
			}
		}

		public class WwwLoadAudioClipJob : WwwLoadJob<AudioClip> {
			public WwwLoadAudioClipJob(string path, string url, string filePath) : base(path, url, filePath) {
			}

			protected override AudioClip LoadData() {
				var clip = www.GetAudioClip(false);
				AudioClipDict.Add(path, clip);
				return clip;
			}
		}

		public class WwwLoadTextJob : WwwLoadJob<string> {
			public WwwLoadTextJob(string path, string url, string filePath) : base(path, url, filePath) {
			}

			protected override string LoadData() {
				var text = www.text;
				TextDict.Add(path, text);
				return text;
			}
		}

		public static ILoadJob<Texture2D> LoadTexture(string path) {
			if (TextureDict.ContainsKey(path)) {
				return new SimpleLoadJob<Texture2D>(TextureDict[path]);
			} else if (File.Exists(GetCacheFilePath(path))) {
				byte[] bytes = File.ReadAllBytes(GetCacheFilePath(path));

				var texture = new Texture2D(4, 4);
				texture.LoadImage(bytes);

				TextureDict.Add(path, texture);
				return new SimpleLoadJob<Texture2D>(texture);
			} else return new WwwLoadTextureJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path));
		}

		public static ILoadJob<AudioClip> LoadAudioClip(string path) {
			if (AudioClipDict.ContainsKey(path)) {
				return new SimpleLoadJob<AudioClip>(AudioClipDict[path]);
			} else if (File.Exists(GetCacheFilePath(path))) {
				return new WwwLoadAudioClipJob(path, GetCacheFileUrl(path), GetCacheFilePath(path));
			} else return new WwwLoadAudioClipJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path));
		}

		public static ILoadJob<string> LoadText(string path) {
			if (TextDict.ContainsKey(path)) {
				return new SimpleLoadJob<string>(TextDict[path]);
			} else if (File.Exists(GetCacheFilePath(path))) {
				string text = File.ReadAllText(GetCacheFilePath(path));
				TextDict.Add(path, text);
				return new SimpleLoadJob<string>(text);
			} else return new WwwLoadTextJob(path, UrlBuilder.GetUploadUrl(path), GetCacheFilePath(path));
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

		readonly LinkedList<IWwwLoadJob> loadingJobs = new LinkedList<IWwwLoadJob>();

		public void FixedUpdate() {
			if (WwwLoadJobQueue.Count <= 0) return;

			var node = loadingJobs.First;
			while (node != null) {
				var next = node.Next;
				if (node.Value.IsFinished()) loadingJobs.Remove(node);
				node = next;
			}

			if (loadingJobs.Count > MaxConcurrentJobCount) return;

			while (WwwLoadJobQueue.Count > 0 && loadingJobs.Count <= MaxConcurrentJobCount) {
				var job = WwwLoadJobQueue.First.Value;
				WwwLoadJobQueue.RemoveFirst();
				job.StartDownload();
				loadingJobs.AddLast(job);
			}

			Debug.LogFormat("Loading: {0}, Queued: {1}", loadingJobs.Count, WwwLoadJobQueue.Count);
		}
	}
}