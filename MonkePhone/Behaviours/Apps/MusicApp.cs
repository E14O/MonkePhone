using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MonkePhone.Behaviours.UI;
using MonkePhone.Extensions;
using MonkePhone.Models;
using MonkePhone.Tools;
using MonkePhone.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace MonkePhone.Behaviours.Apps
{
	public class MusicApp : PhoneApp
	{
		public override string AppId => "Music";

		private List<string> _musicList;
		private Dictionary<string, AudioClip> _musicComparison = new();

		private int _currentMusic;
		private int _currentPage;

		private bool _isLoadingMusic;
		private bool _wasPlaying = true;
		private bool _inDownloadView = true;

		private Transform _vinylDisk;
		private PhoneSlider _timelineSlider;

		private Text _songTitle;
		private Text _songMissing;
		private Text _songTimePosition;
		private Text _songLengthText;

		public AudioSource MusicSource;

		private List<StreamableMusicComponent> _streamableMusicComponents = new();

		public override void Initialize()
		{
			Logging.Log("[MusicApp] Initialize called");

			MusicSource = GorillaLocomotion.GTPlayer.Instance.gameObject.AddComponent<AudioSource>();
			Logging.Log("[MusicApp] AudioSource created and added to player");

			_songTitle = transform.Find("CurrentlyPlayingContents/AudioTitle").GetComponent<Text>();
			_songTimePosition = transform.Find("CurrentlyPlayingContents/Timeline/Slider/Text (Legacy)").GetComponent<Text>();
			_timelineSlider = (PhoneSlider)GetObject("Timeline");
			_vinylDisk = transform.Find("CurrentlyPlayingContents/VinylRecord");
			_songMissing = transform.Find("CurrentlyPlayingContents/NoMusicWarning")?.GetComponent<Text>();
			_songLengthText = transform.Find("CurrentlyPlayingContents/Timeline/Song Length")?.GetComponent<Text>();

			SetVolumeMultiplier(Configuration.MusicMultiplier.Value);
			SetSpatialBlend(Configuration.UseSpatialBlend.Value);

			Logging.Log("[MusicApp] Finished setting up UI references and audio settings");

			EvaluateMusicList();
			_inDownloadView = _musicList.Count == 0;

			Logging.Log($"[MusicApp] Music list evaluated. Found {_musicList.Count} songs.");
		}

		public void Update()
		{
			if (MusicSource && MusicSource.clip != null && MusicSource.isPlaying)
			{
				float progress = MusicSource.time / MusicSource.clip.length;
				var parameters = _timelineSlider.Parameters;
				parameters.z = progress * 100;
				_timelineSlider.Parameters = parameters;
				_timelineSlider.UpdatePosition();

				_songTimePosition.text = MusicSource.time.ToString("F2");

				if (_songLengthText != null)
				{
					_songLengthText.text = MusicSource.clip.length.ToString("F2");
				}
			}
		}

		public override void AppOpened()
		{
			Logging.Log("[MusicApp] AppOpened called");
			EvaluateMusicList();
			RefreshSuitableContainer();
		}

		private void EvaluateMusicList()
		{
			Logging.Log("[MusicApp] Evaluating music list...");
			var current = _musicList;
			_musicList = Directory.GetFiles(PhoneManager.Instance.MusicPath)
				.Where(file => file.ToLower().EndsWith(".mp3") || file.ToLower().EndsWith(".ogg") || file.ToLower().EndsWith(".wav"))
				.ToList();

			var missingItems = current?.Where(str => !_musicList.Contains(str) && _musicComparison.ContainsKey(str)) ?? Enumerable.Empty<string>();
			foreach (var str in missingItems)
			{
				_musicComparison.Remove(str);
			}

			Logging.Log($"[MusicApp] Music list evaluation complete. Found {_musicList.Count} files.");
		}

		private void RefreshSuitableContainer()
		{
			Logging.Log($"[MusicApp] Refreshing container. _inDownloadView={_inDownloadView}");
			transform.Find("MusicDownloadContainer").gameObject.SetActive(_inDownloadView);
			transform.Find("MusicPlayerContainer").gameObject.SetActive(!_inDownloadView);
			if (_inDownloadView)
			{
				RefreshDownloadables();
			}
			else
			{
				RefreshSongList();
				RefreshApp();
			}
		}

		private void RefreshDownloadables()
		{
			Logging.Log("[MusicApp] Refreshing downloadable songs");
			_currentPage = MathEx.Wrap(_currentPage, 0, Mathf.CeilToInt(PhoneManager.Instance.Data.songs.Length / 3f));
			Song[] songs = {
				PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 0),
				PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 1),
				PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 2)
			};

			Transform table = transform.Find("MusicDownloadContainer/Table");
			_streamableMusicComponents.Clear();

			for (int i = 0; i < table.childCount; i++)
			{
				Transform item = table.GetChild(i);
				Song song = songs.ElementAtOrDefault(i);
				bool noSong = song == null || song == default;
				item.gameObject.SetActive(!noSong);
				if (noSong) continue;

				if (item.TryGetComponent(out StreamableMusicComponent component))
				{
					Destroy(component);
				}

				component = item.AddComponent<StreamableMusicComponent>();
				component.song = song;
				component.coverArt = item.Find("AlbumCover").GetComponent<RawImage>();
				component.nameText = item.Find("AudioTitle").GetComponent<Text>();
				component.buttonText = item.FindChildRecursive("download").GetComponentInChildren<Text>();
				_streamableMusicComponents.Add(component);
			}
		}

		private void RefreshSongList()
		{
			Logging.Log("[MusicApp] Refreshing local song list UI");
			Transform table = transform.Find("MusicPlayerContainer/Table");
			Transform noSongs = transform.Find("MusicPlayerContainer/NoMusic");
			table.gameObject.SetActive(_musicList.Count > 0);
			noSongs.gameObject.SetActive(_musicList.Count == 0);
			if (_musicList.Count == 0) return;

			_currentPage = MathEx.Wrap(_currentPage, 0, Mathf.CeilToInt(_musicList.Count / 3f));
			string[] songs = {
				_musicList.ElementAtOrDefault((_currentPage * 3) + 0),
				_musicList.ElementAtOrDefault((_currentPage * 3) + 1),
				_musicList.ElementAtOrDefault((_currentPage * 3) + 2)
			};

			for (int i = 0; i < table.childCount; i++)
			{
				Transform item = table.GetChild(i);
				string song = songs.ElementAtOrDefault(i);
				bool noSong = song == null || song == default;
				item.gameObject.SetActive(!noSong);
				if (noSong) continue;

				item.Find("AudioTitle").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(song);
				item.Find("Format Label").GetComponent<Text>().text = Path.GetExtension(song).Replace(".", "").ToUpper();
				item.Find("Length Label").GetComponent<Text>().text = "--:--";
			}
		}

		public override async void ButtonClick(PhoneUIObject phoneUIObject, bool isLeftHand)
		{
			Logging.Log($"[MusicApp] Button clicked: {phoneUIObject.name}");
			if (phoneUIObject.name.StartsWith("download"))
			{
				int index = int.Parse(phoneUIObject.name[^1].ToString()) - 1;
				var component = _streamableMusicComponents.ElementAtOrDefault(index);
				if (!component) return;

				component.UpdateText(component.song.currentState != Song.DownloadState.Downloaded ? Song.DownloadState.Awaiting : Song.DownloadState.Downloaded);
				var state = await component.song.Download();
				if (state == Song.DownloadState.None) return;

				EvaluateMusicList();
				component.UpdateText(state);
				PlaySound(state == Song.DownloadState.Downloaded ? "RequestSuccess" : "RequestDenied");
				return;
			}

			switch (phoneUIObject.name)
			{
				case "tab library":
					_inDownloadView = false;
					_currentPage = 0;
					RefreshSuitableContainer();
					break;
				case "tag catalog":
					_inDownloadView = true;
					_currentPage = 0;
					RefreshSuitableContainer();
					break;
				case "pageleft":
					_currentPage--;
					RefreshSuitableContainer();
					break;
				case "pageright":
					_currentPage++;
					RefreshSuitableContainer();
					break;
				case "Music Last":
					_currentMusic--;
					RefreshApp();
					break;
				case "Music Next":
					_currentMusic++;
					RefreshApp();
					break;
				case "play1":
					HandlePlayButton(_currentPage * 3 + 0);
					break;
				case "play2":
					HandlePlayButton(_currentPage * 3 + 1);
					break;
				case "play3":
					HandlePlayButton(_currentPage * 3 + 2);
					break;
				case "music toggle":
					if (MusicSource.clip != null && !_isLoadingMusic)
					{
						if (MusicSource.isPlaying)
						{
							Logging.Log("[MusicApp] Pausing music");
							MusicSource.Pause();
						}
						else
						{
							Logging.Log("[MusicApp] Playing music");
							MusicSource.Play();
						}
					}
					break;
			}
		}

		private void HandlePlayButton(int trackIndex)
		{
			if (_isLoadingMusic || _musicList.Count <= trackIndex) return;

			if (_currentMusic != trackIndex)
			{
				MusicSource.clip = null;
				_currentMusic = trackIndex;
				_songTitle.text = Path.GetFileNameWithoutExtension(_musicList[_currentMusic]);
			}

			Logging.Log("[MusicApp] Music toggle clicked");

			if (MusicSource.clip == null || MusicSource.clip.name != Path.GetFileName(_musicList[_currentMusic]))
			{
				Logging.Log("[MusicApp] Setting new track...");
				_isLoadingMusic = true;
				StartCoroutine(SetTrack(_musicList[_currentMusic]));
				return;
			}

			if (MusicSource.isPlaying)
			{
				Logging.Log("[MusicApp] Pausing music");
				MusicSource.Pause();
			}
			else
			{
				Logging.Log("[MusicApp] Playing music");
				MusicSource.Play();
			}
		}

		public override void ButtonTicked(PhoneUIObject phoneUIObject, bool currentValue, bool isLeftHand)
		{
			if (phoneUIObject.name == "music toggle")
			{
				if (MusicSource != null && MusicSource.clip != null)
				{
					if (MusicSource.isPlaying)
						MusicSource.Pause();
					else
						MusicSource.Play();
				}
			}
		}

		public void RefreshApp()
		{
			try
			{
				Logging.Log("[MusicApp] Refreshing app view");
				if (_songMissing != null)
				{
					_songMissing.gameObject.SetActive(_musicList.Count == 0);
				}

				if (_musicList.Count == 0)
				{
					_songTitle.text = "";
					_timelineSlider.gameObject.SetActive(false);
					return;
				}

				_currentMusic = MathEx.Wrap(_currentMusic, 0, _musicList.Count);
				_songTitle.text = Path.GetFileNameWithoutExtension(_musicList[_currentMusic]);
				_timelineSlider.gameObject.SetActive(true);

				if (_timelineSlider.Value != 0 && MusicSource.clip != null && MusicSource.clip.name != Path.GetFileNameWithoutExtension(_musicList[_currentMusic]))
				{
					_timelineSlider.Value = 0;
					_timelineSlider.UpdatePosition();
					_songTimePosition.text = "00:00";
				}
			}
			catch (Exception ex)
			{
				Logging.Error($"[MusicApp] Error when refreshing app: {ex}");
			}
		}

		public IEnumerator SetTrack(string path)
		{
			Logging.Log($"[MusicApp] Setting track: {path}");

			AudioType audioType = AudioType.UNKNOWN;
			if (path.EndsWith(".mp3")) audioType = AudioType.MPEG;
			else if (path.EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
			else if (path.EndsWith(".wav")) audioType = AudioType.WAV;

			DownloadHandlerAudioClip downloadHandler = new DownloadHandlerAudioClip(path, audioType);
			UnityWebRequest webRequest = new UnityWebRequest(path, "GET", downloadHandler, null);

			yield return webRequest.SendWebRequest();
			_isLoadingMusic = false;

			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				Logging.Log("[MusicApp] SetTrack successful, starting playback");
				downloadHandler.audioClip.name = Path.GetFileName(path);
				MusicSource.clip = downloadHandler.audioClip;

				if (_wasPlaying)
				{
					MusicSource.volume = 0.1f * Configuration.MusicMultiplier.Value;
					MusicSource.Play();
					MusicSource.time = 0f;
				}
			}
			else
			{
				Logging.Error($"[MusicApp] Error setting track: {webRequest.error}");
			}
		}

		public void SetVolumeMultiplier(float multiplier)
		{
			Logging.Log($"[MusicApp] Setting volume multiplier to {multiplier}");
			MusicSource.volume = 0.1f * multiplier;
		}

		public void SetSpatialBlend(bool useSpatialBlend)
		{
			Logging.Log($"[MusicApp] Setting spatial blend: {useSpatialBlend}");
			MusicSource.spatialBlend = (useSpatialBlend ? 1f : 0f);
		}

		public class StreamableMusicComponent : MonoBehaviour
		{
			public Song song;
			public Text nameText, buttonText;
			public RawImage coverArt;

			public async void Start()
			{
				nameText.text = song.title;
				coverArt.texture = null;

				if (!song.IsDownloaded && song.currentState == Song.DownloadState.Downloaded)
				{
					song.currentState = Song.DownloadState.None;
				}

				UpdateText(song.currentState);

				if (string.IsNullOrWhiteSpace(song.coverUrl)) return;

				UnityWebRequest request = UnityWebRequestTexture.GetTexture(song.coverUrl);
				await YieldUtils.Yield(request);

				if (request.result == UnityWebRequest.Result.Success)
				{
					Texture tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
					coverArt.texture = tex;
				}
			}

			public void UpdateText(Song.DownloadState dlState)
			{
				switch (dlState)
				{
					case Song.DownloadState.None:
						buttonText.text = "Download";
						break;
					case Song.DownloadState.Awaiting:
						buttonText.text = "<color=grey>Progressing..</color>";
						break;
					case Song.DownloadState.Downloaded:
						buttonText.text = $"<color=green>Saved {song.title}!</color>";
						break;
					case Song.DownloadState.Failed:
						buttonText.text = "<color=red>Music could not be saved.</color>";
						break;
				}
			}

			public void Download() { }
		}
	}
}