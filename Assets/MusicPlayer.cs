using UnityEngine;

using Uif;

public class MusicPlayer : MonoBehaviour {
	public static MusicPlayer Instance;

	public float Volume {
		get { return volumeHidable.ShowVolume; }
		set {
			volumeHidable.ShowVolume = value;
			volumeHidable.Show();
		}
	}

	public AudioSource audioSource;
	public AudioSourceHidable volumeHidable;

	public void OnValidate() {
		audioSource = GetComponent<AudioSource>();
		volumeHidable = GetComponent<AudioSourceHidable>();
	}

	public void Start() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(this);
	}

	public void SetClip(AudioClip clip) {
		audioSource.clip = clip;
	}

	public void Play() {
		volumeHidable.ForceHide();
		audioSource.Play();
		volumeHidable.Show();
	}

	public void Stop() {
		volumeHidable.Callback = audioSource.Stop;
		volumeHidable.Hide();
	}

	public void Pause() {
		volumeHidable.Callback = audioSource.Pause;
		volumeHidable.Hide();
	}

	public bool IsPlaying() {
		return audioSource.isPlaying;
	}
}
